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
      return Task.FromResult(new ComputeReply
      {
        Result = ByteString.CopyFromUtf8(output),
      });
    }
  }
}