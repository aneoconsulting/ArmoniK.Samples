// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2023. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArmoniK.Api.Common.Channel.Utils;
using ArmoniK.Api.Common.Utils;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.Worker.Worker;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HelloWorld.Worker
{
  public class HelloWorldWorker : WorkerStreamWrapper
  {
    /// <summary>
    ///   Initializes an instance of <see cref="HelloWorldWorker" />
    /// </summary>
    /// <param name="loggerFactory">Factory to create loggers</param>
    /// <param name="provider">gRPC channel provider to send tasks and results to ArmoniK Scheduler</param>
    public HelloWorldWorker(ILoggerFactory      loggerFactory,
                            GrpcChannelProvider provider)
      : base(loggerFactory,
             provider)
      => logger_ = loggerFactory.CreateLogger<HelloWorldWorker>();

    /// <summary>
    ///   Function that represents the processing of a task.
    /// </summary>
    /// <param name="taskHandler">Handler that holds the payload, the task metadata and helpers to submit tasks and results</param>
    /// <returns>
    ///   An <see cref="Output" /> representing the status of the current task. This is the final step of the task.
    /// </returns>
    public override async Task<Output> Process(ITaskHandler taskHandler)
    {
      // Logger scope that will add metadata (session and task ids) for each use of the logger
      // It will facilitate the search for information related to the execution of the task/session
      using var scopedLog = logger_.BeginNamedScope("Execute task",
                                                    ("sessionId", taskHandler.SessionId),
                                                    ("taskId", taskHandler.TaskId));

      try
      {
        // We convert the binary payload from the handler back to the string sent by the client
        var input = Encoding.ASCII.GetString(taskHandler.Payload);

        // We get the result that the task should produce
        // The handler has this information
        // It also contains other information such as the data dependencies (id and binary data) if any
        var resultId = taskHandler.ExpectedResults.Single();
        // We the result of the task using through the handler
        await taskHandler.SendResult(resultId,
                                     Encoding.ASCII.GetBytes($"{input} World_ {resultId}"))
                         .ConfigureAwait(false);
      }
      // If there is an exception, we put the task in error
      // The task will not be retried by ArmoniK
      // An uncatched exception means that the task will be retried
      catch (Exception e)
      {
        logger_.LogError(e,
                         "Error during task computing.");
        return new Output
               {
                 Error = new Output.Types.Error
                         {
                           Details = e.Message,
                         },
               };
      }

      // Return an OK output
      // The task finished successfully
      return new Output
             {
               Ok = new Empty(),
             };
    }
  }
}
