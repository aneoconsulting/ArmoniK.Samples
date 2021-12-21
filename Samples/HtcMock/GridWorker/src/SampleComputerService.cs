// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2021. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using ArmoniK.Core.gRPC.V1;
using ArmoniK.Samples.HtcMock.Adapter;

using Google.Protobuf;

using Grpc.Core;

using Htc.Mock.RequestRunners;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.GridWorker
{
  public class SampleComputerService : ComputerService.ComputerServiceBase
  {
    private static Htc.Mock.GridWorker gridWorker_;

    [SuppressMessage("CodeQuality",
                     "IDE0052:Remove unread private members",
                     Justification = "Used for side effects")]
    private readonly ApplicationLifeTimeManager applicationLifeTime_;

    private readonly ILogger<SampleComputerService> logger_;

    public SampleComputerService(ILogger<SampleComputerService> logger,
                                 ApplicationLifeTimeManager     applicationLifeTime,
                                 RedisDataClient                redisDataClient,
                                 GridClient                     gridClient)
    {
      logger_              = logger;
      applicationLifeTime_ = applicationLifeTime;

      gridWorker_ = new Htc.Mock.GridWorker(new DelegateRequestRunnerFactory((runConfiguration, session)
                                                                                       => new DistributedRequestRunner(redisDataClient,
                                                                                                                       gridClient,
                                                                                                                       runConfiguration,
                                                                                                                       session,
                                                                                                                       false,
                                                                                                                       false,
                                                                                                                       false)));
    }

    /// <inheritdoc />
    public override Task<ComputeReply> Execute(ComputeRequest request, ServerCallContext context)
    {
      using var scoledLog = logger_.BeginNamedScope("Execute task",
                                                       ("Session", request.Session),
                                                       ("SubSession", request.Subsession),
                                                       ("taskId", request.TaskId));
      logger_.LogDebug("Begin execution : payload size = {size}",
                       request.Payload.Length);
      var sessionId = new SessionId { Session = request.Session, SubSession = request.Subsession };
      try
      {
        var output = gridWorker_.Execute(sessionId.ToHtcMockId(),
                            request.TaskId,
                            request.Payload.ToByteArray());
        return Task.FromResult(new ComputeReply { Result = ByteString.CopyFrom(output) });
      }
      catch (Exception e)
      {
        logger_.LogWarning(e,
                           "Error while executing task");
        throw;
      }
      finally
      {
        logger_.LogDebug("Executed task : Session => {session}, Task => {task}",
                         request.Session,
                         request.TaskId);
      }
    }
  }
}
