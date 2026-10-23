// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-2026. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Globalization;

using ArmoniK.Extensions.CSharp.Common.Common.Domain.Task;
using ArmoniK.Extensions.CSharp.Worker.Interfaces;
using ArmoniK.Extensions.CSharp.Worker.Interfaces.Common.Domain.Blob;
using ArmoniK.Extensions.CSharp.Worker.Interfaces.Common.Domain.Task;

using Microsoft.Extensions.Logging;

using TaskDefinition = ArmoniK.Extensions.CSharp.Worker.Interfaces.Common.Domain.Task.TaskDefinition;

namespace SubTasking.Worker;

/// <summary>
///   Computes S(lo, hi) = sum of i^2 for i in [lo, hi) by recursive splitting.
///   <para>
///     A task with inputs <c>lo</c>, <c>hi</c>, <c>grain</c> either computes the sum directly when
///     <c>hi - lo &lt;= grain</c>, or submits three subtasks:
///     two children on [lo, mid) and [mid, hi), and a combine task that adds their results.
///     The combine task takes over the parent's <c>result</c> output, so the parent ends without writing it.
///   </para>
///   <para>
///     A task with inputs <c>left</c>, <c>right</c> is a combine task: it writes left + right to <c>result</c>.
///   </para>
///   All subtasks run this same class from the same library blob, on the same partition as the parent.
/// </summary>
public class RangeSumWorker : IWorker
{
  public Task<HealthCheckResult> CheckHealth(CancellationToken cancellationToken = default)
    => Task.FromResult(HealthCheckResult.Healthy());

  public async Task<TaskResult> ExecuteAsync(ISdkTaskHandler   taskHandler,
                                             ILogger           logger,
                                             CancellationToken cancellationToken)
  {
    var inputs = taskHandler.Inputs;

    if (inputs.ContainsKey("left") && inputs.ContainsKey("right"))
    {
      await CombineAsync(taskHandler,
                         logger,
                         cancellationToken)
        .ConfigureAwait(false);
      return TaskResult.Success;
    }

    if (inputs.ContainsKey("lo") && inputs.ContainsKey("hi") && inputs.ContainsKey("grain"))
    {
      await SplitOrComputeAsync(taskHandler,
                                logger,
                                cancellationToken)
        .ConfigureAwait(false);
      return TaskResult.Success;
    }

    return TaskResult.Failure($"Unexpected inputs: {string.Join(", ", inputs.Keys)}");
  }

  private static async Task SplitOrComputeAsync(ISdkTaskHandler   taskHandler,
                                                ILogger           logger,
                                                CancellationToken cancellationToken)
  {
    var lo    = ReadLong(taskHandler, "lo");
    var hi    = ReadLong(taskHandler, "hi");
    var grain = ReadLong(taskHandler, "grain");

    if (hi - lo <= grain)
    {
      long sum = 0;
      for (var i = lo; i < hi; i++)
      {
        sum = checked(sum + i * i);
      }

      logger.LogInformation("Leaf [{lo}, {hi}) = {sum}", lo, hi, sum);
      await taskHandler.Outputs["result"]
                       .SendStringResultAsync(sum.ToString(CultureInfo.InvariantCulture),
                                              cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
      return;
    }

    var mid = lo + (hi - lo) / 2;
    logger.LogInformation("Split [{lo}, {hi}) at {mid}", lo, hi, mid);

    // Subtasks run the same library and symbol as this task.
    var library = taskHandler.TaskOptions.GetDynamicLibrary();

    // Blobs reused from this task: lo, hi and grain are already in the object storage.
    var loBlob    = BlobDefinition.FromBlobHandle(taskHandler.Inputs["lo"]);
    var hiBlob    = BlobDefinition.FromBlobHandle(taskHandler.Inputs["hi"]);
    var grainBlob = BlobDefinition.FromBlobHandle(taskHandler.Inputs["grain"]);
    // New blob shared by both children: upper bound of the left half, lower bound of the right half.
    var midBlob = BlobDefinition.FromString("mid",
                                            mid.ToString(CultureInfo.InvariantCulture));

    var left = new TaskDefinition().WithInput("lo",    loBlob)
                                   .WithInput("hi",    midBlob)
                                   .WithInput("grain", grainBlob)
                                   .WithOutput("result",
                                               BlobDefinition.CreateOutput("left"))
                                   .WithLibrary(library)
                                   .WithTaskOptions(taskHandler.TaskOptions);

    var right = new TaskDefinition().WithInput("lo",    midBlob)
                                    .WithInput("hi",    hiBlob)
                                    .WithInput("grain", grainBlob)
                                    .WithOutput("result",
                                                BlobDefinition.CreateOutput("right"))
                                    .WithLibrary(library)
                                    .WithTaskOptions(taskHandler.TaskOptions);

    // The combine task depends on the children's outputs and produces this task's output.
    // ArmoniK only starts it once both children have completed.
    var combine = new TaskDefinition().WithInput("left",  left.OutputDefinitions["result"])
                                      .WithInput("right", right.OutputDefinitions["result"])
                                      .WithOutput("result",
                                                  BlobDefinition.FromBlobHandle(taskHandler.Outputs["result"]))
                                      .WithLibrary(library)
                                      .WithTaskOptions(taskHandler.TaskOptions);

    await foreach (var task in taskHandler.SubmitTasksAsync([left, right, combine],
                                                            taskHandler.TaskOptions,
                                                            cancellationToken)
                                          .ConfigureAwait(false))
    {
      logger.LogInformation("Submitted subtask {taskId}", task.TaskId);
    }
  }

  private static async Task CombineAsync(ISdkTaskHandler   taskHandler,
                                         ILogger           logger,
                                         CancellationToken cancellationToken)
  {
    var left  = ReadLong(taskHandler, "left");
    var right = ReadLong(taskHandler, "right");
    var sum   = checked(left + right);

    logger.LogInformation("Combine {left} + {right} = {sum}", left, right, sum);
    await taskHandler.Outputs["result"]
                     .SendStringResultAsync(sum.ToString(CultureInfo.InvariantCulture),
                                            cancellationToken: cancellationToken)
                     .ConfigureAwait(false);
  }

  private static long ReadLong(ISdkTaskHandler taskHandler,
                               string          name)
    => long.Parse(taskHandler.Inputs[name]
                             .GetStringData(),
                  CultureInfo.InvariantCulture);
}
