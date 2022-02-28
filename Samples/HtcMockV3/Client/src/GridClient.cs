// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022. All rights reserved.
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