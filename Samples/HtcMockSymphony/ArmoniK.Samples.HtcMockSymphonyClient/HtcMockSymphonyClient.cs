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

using ArmoniK.DevelopmentKit.SymphonyApi.Client.api;

using Htc.Mock;
using Htc.Mock.Core;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMockSymphonyClient
{
  public class HtcMockSymphonyClient
  {
    private readonly ILogger<Client> _logger;

    private readonly SessionService _sessionService;

    public HtcMockSymphonyClient(SessionService  sessionService,
                                 ILogger<Client> logger)
    {
      _sessionService = sessionService;
      _logger         = logger;
    }

    public void Start(RunConfiguration config)
    {
      _logger.LogInformation("Start new run with {configuration}",
                             config.ToString());
      var watch = Stopwatch.StartNew();

      var request = config.BuildRequest(out var shape,
                                        _logger);

      var taskId = _sessionService.SubmitTask(DataAdapter.BuildPayload(config,
                                                                       request));

      _logger.LogInformation("Submitted root task {taskId}",
                             taskId);
      _sessionService.WaitForTaskCompletion(taskId);

      var result = Encoding.Default.GetString(_sessionService.GetResult(taskId));

      _logger.LogWarning("Final result is {result}",
                         result);
      _logger.LogWarning("Expected result is 1.{result}",
                         string.Join(".",
                                     shape));

      watch.Stop();
      _logger.LogWarning("Client was executed in {time}s",
                         watch.Elapsed.TotalSeconds);
    }
  }
}
