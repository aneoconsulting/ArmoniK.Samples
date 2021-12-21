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
using System.Linq;

using ArmoniK.Core.gRPC.V1;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

namespace ArmoniK.HelloWorld.Client
{
  internal class Program
  {
    private static void Main()
    {
      Console.WriteLine("Create channel");
      var channel = GrpcChannel.ForAddress("http://localhost:80");
      Console.WriteLine("Create client");
      var client = new ClientService.ClientServiceClient(channel);

      Console.WriteLine("Create session");
      var session = client.CreateSession(new SessionOptions
                                         {
                                           DefaultTaskOption = new TaskOptions
                                                               {
                                                                 MaxDuration = Duration.FromTimeSpan(TimeSpan.FromMinutes(1)),
                                                                 MaxRetries  = 2,
                                                                 Priority    = 1,
                                                               },
                                         });

      Console.WriteLine("Create task");
      var task = client.CreateTask(new CreateTaskRequest
                                   {
                                     SessionId = session,
                                     TaskRequests =
                                     {
                                       new TaskRequest
                                       {
                                         Payload = new Payload { Data = ByteString.CopyFromUtf8("Hello") },
                                       },
                                     },
                                   })
                       .TaskIds
                       .Single();

      Console.WriteLine("Wait for task");
      client.WaitForCompletion(new TaskFilter
                               {
                                 SessionId = task.Session, SubSessionId = task.SubSession, IncludedTaskIds = { task.Task },
                               });
    }
  }
}