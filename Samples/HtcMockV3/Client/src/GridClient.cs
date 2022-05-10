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

using System;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Samples.HtcMock.Adapter;

using Google.Protobuf.WellKnownTypes;

using Htc.Mock;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.Client
{
  public class GridClient : IGridClient
  {
    private readonly Submitter.SubmitterClient client_;
    private readonly ILogger<GridClient>       logger_;

    public GridClient(Submitter.SubmitterClient client, ILoggerFactory loggerFactory)
    {
      client_ = client;
      logger_ = loggerFactory.CreateLogger<GridClient>();
    }

    public ISessionClient CreateSubSession(string taskId)
    {
      return CreateSession();
    }

    public ISessionClient CreateSession()
    {
      using var _         = logger_.LogFunction();
      var       sessionId = Guid.NewGuid().ToString();
      var createSessionRequest = new CreateSessionRequest
      {
        DefaultTaskOption = new TaskOptions
        {
          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
          MaxRetries  = 2,
          Priority    = 1,
        },
        Id = sessionId,
      };
      var session = client_.CreateSession(createSessionRequest);
      switch (session.ResultCase)
      {
        case CreateSessionReply.ResultOneofCase.Error:
          throw new Exception("Error while creating session : " + session.Error);
        case CreateSessionReply.ResultOneofCase.None:
          throw new Exception("Issue with Server !");
        case CreateSessionReply.ResultOneofCase.Ok:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return new SessionClient(client_,
                               sessionId,
                               logger_);
    }
  }
}