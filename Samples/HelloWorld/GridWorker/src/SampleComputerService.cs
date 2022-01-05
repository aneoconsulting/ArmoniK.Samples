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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using ArmoniK.Core.gRPC.V1;

using Google.Protobuf;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace ArmoniK.HelloWorld.Worker
{
  public class SampleComputerService : ComputerService.ComputerServiceBase
  {
    [SuppressMessage("CodeQuality",
                     "IDE0052:Remove unread private members",
                     Justification = "Used for side effects")]
    private readonly ApplicationLifeTimeManager applicationLifeTime_;

    private readonly ILogger<SampleComputerService> logger_;

    public SampleComputerService(ILoggerFactory loggerFactory, ApplicationLifeTimeManager applicationLifeTime)
    {
      logger_              = loggerFactory.CreateLogger<SampleComputerService>();
      applicationLifeTime_ = applicationLifeTime;
    }

    /// <inheritdoc />
    public override Task<ComputeReply> Execute(ComputeRequest request, ServerCallContext context)
    {
      logger_.LogInformation($"Processing Task {request.TaskId} of Session {request.Session}");
      logger_.LogInformation("request: {request}",
                             request.Payload.ToStringUtf8());
      var output = "World";
      logger_.LogInformation("reply: {result}",
                             output);
      return Task.FromResult(new ComputeReply { Result = ByteString.CopyFromUtf8(output) });
    }
  }
}