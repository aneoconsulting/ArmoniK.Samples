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

using ArmoniK.DevelopmentKit.Common.Exceptions;
using ArmoniK.DevelopmentKit.SymphonyApi;
using ArmoniK.DevelopmentKit.SymphonyApi.api;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Htc.Mock.Core;

namespace ArmoniK.Samples.HtcMockSymphony.Packages
{
  public class ServiceContainer : ServiceContainerBase
  {
    private readonly IConfiguration configuration_;
    private ILogger<ServiceContainer> _logger;

    public override void OnCreateService(ServiceContext serviceContext)
    {
      _logger = LoggerFactory.CreateLogger<ServiceContainer>();
    }

    public override void OnSessionEnter(SessionContext sessionContext)
    {
      //END USER PLEASE FIXME
    }

    public override byte[] OnInvoke(SessionContext sessionContext, TaskContext taskContext)
    {
      try
      {
        var (runConfiguration, request) = DataAdapter.ReadPayload(taskContext.TaskInput);
        var inputs = request.Dependencies
          .ToDictionary(id => id,
            id =>
            {
              _logger.LogInformation("Looking for result for Id {id}",
                id);
              var isOkay = taskContext.DataDependencies.TryGetValue(id, out var data);
              if (!isOkay)
              {
                throw new KeyNotFoundException(id);
              }

              return Encoding.Default.GetString(data);
            });

        var requestProcessor = new RequestProcessor(true,
          true,
          true,
          runConfiguration,
          _logger);
        var res = requestProcessor.GetResult(request, inputs);
        _logger.LogDebug("Result for processing request is HasResult={hasResult}, Value={value}",
          res.Result.HasResult,
          res.Result.Value);

        if (res.Result.HasResult)
        {
          return Encoding.Default.GetBytes(res.Result.Value);
        }

        var requests = res.SubRequests.GroupBy(r => r.Dependencies is null || r.Dependencies.Count == 0)
          .ToDictionary(g => g.Key,
            g => g);
        var readyRequests = requests[true];
        var requestsCount = readyRequests.Count();
        _logger.LogDebug("Will submit {count} new tasks", requestsCount);


        var payloads = new List<byte[]>(requestsCount);
        payloads.AddRange(readyRequests.Select(readyRequest => DataAdapter.BuildPayload(runConfiguration,
                                                                                        readyRequest)));

        var taskIds = SubmitTasks(payloads);
        var req = requests[false].Single();
        req.Dependencies.Clear();
        foreach (var t in taskIds)
        {
          req.Dependencies.Add(t);
        }
        SubmitTasksWithDependencies(new List<Tuple<byte[], IList<string>>>(
          new List<Tuple<byte[], IList<string>>>
          {
            new(
              DataAdapter.BuildPayload(runConfiguration, req),
              req.Dependencies
            ),
          }), true);

        return null;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error while computing task");
        throw new WorkerApiException("Error while computing task", ex);
      }
      
    }

    public override void OnSessionLeave(SessionContext sessionContext)
    {
      //END USER PLEASE FIXME
    }

    public override void OnDestroyService(ServiceContext serviceContext)
    {
      //END USER PLEASE FIXME
    }
  }
}