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
                                         Payload = new Payload
                                                   {
                                                     Data = ByteString.CopyFromUtf8("Hello"),
                                                   },
                                       },
                                     },
                                   })
                       .TaskIds.Single();

      Console.WriteLine("Wait for task");
      client.WaitForCompletion(new WaitRequest
                               {
                                 Filter = new TaskFilter
                                          {
                                            SessionId    = task.Session,
                                            SubSessionId = task.SubSession,
                                            IncludedTaskIds =
                                            {
                                              task.Task,
                                            },
                                          },
                                 ThrowOnTaskCancellation = true,
                                 ThrowOnTaskError        = true,
                               });
    }
  }
}
