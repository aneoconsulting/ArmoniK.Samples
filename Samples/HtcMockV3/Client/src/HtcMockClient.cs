// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System.Diagnostics;
using System.Text;

using Htc.Mock;
using Htc.Mock.Core;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.Client
{
  public class HtcMockClient
  {
    private readonly GridClient               gridClient_;
    private readonly ILogger<Htc.Mock.Client> logger_;

    public HtcMockClient(GridClient               gridClient,
                         ILogger<Htc.Mock.Client> logger)
    {
      gridClient_ = gridClient;
      logger_     = logger;
    }

    public void Start(RunConfiguration runConfiguration)
    {
      logger_.LogInformation("Start new run with {configuration}",
                             runConfiguration.ToString());
      var watch = Stopwatch.StartNew();

      var sessionClient = gridClient_.CreateSession();

      var request = runConfiguration.BuildRequest(out var shape,
                                                  logger_);

      var taskId = sessionClient.SubmitTask(DataAdapter.BuildPayload(runConfiguration,
                                                                     request));

      logger_.LogInformation("Submitted root task {taskId}",
                             taskId);
      sessionClient.WaitSubtasksCompletion(taskId)
                   .Wait();

      var result = Encoding.Default.GetString(sessionClient.GetResult(taskId));

      logger_.LogWarning("Final result is {result}",
                         result);
      logger_.LogWarning("Expected result is 1.{result}",
                         string.Join(".",
                                     shape));

      watch.Stop();
      logger_.LogWarning("Client was executed in {time}s",
                         watch.Elapsed.TotalSeconds);
    }
  }
}
