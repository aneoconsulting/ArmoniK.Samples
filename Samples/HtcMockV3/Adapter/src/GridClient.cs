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

using ArmoniK.Core.gRPC.V1;

using Google.Protobuf.WellKnownTypes;

using Htc.Mock;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.Adapter
{
  public class GridClient : IGridClient
  {
    private readonly ClientService.ClientServiceClient client_;
    private readonly ILogger<GridClient>               logger_;

    public GridClient(ClientService.ClientServiceClient client, ILoggerFactory loggerFactory)
    {
      client_ = client;
      logger_ = loggerFactory.CreateLogger<GridClient>();
    }

    public ISessionClient CreateSubSession(string taskId)
    {
      using var _ = logger_.LogFunction(taskId);
      var sessionOptions = new SessionOptions
      {
        DefaultTaskOption = new TaskOptions
        {
          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromMinutes(20)),
          MaxRetries  = 2,
          Priority    = 1,
        },
        ParentTask = taskId.ToTaskId(),
      };
      var subSessionId = client_.CreateSession(sessionOptions);
      return new SessionClient(client_,
                               subSessionId,
                               logger_);
    }

    public ISessionClient CreateSession()
    {
      using var _ = logger_.LogFunction();
      var sessionOptions = new SessionOptions
      {
        DefaultTaskOption = new TaskOptions
        {
          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromMinutes(20)),
          MaxRetries  = 2,
          Priority    = 1,
        },
      };
      var sessionId = client_.CreateSession(sessionOptions);
      return new SessionClient(client_,
                               sessionId,
                               logger_);
    }
  }
}