// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2025. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Diagnostics;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.Submitter;

using DocoptNet;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Empty = ArmoniK.Api.gRPC.V1.Empty;

const string usage = @"Test to send large payloads to the control plane

Usage:
  ArmoniK.Samples.LargePayloads <nbTasks> <sizeKb>

Options:
  -h --help     Show this screen.
  --version     Show version.
";


var arguments = new Docopt().Apply(usage,
                                   args,
                                   version: "Large Payloads",
                                   exit: true)!;

var nbTasks = arguments["<nbTasks>"].AsInt;

var payloadSizeByte = arguments["<sizeKb>"].AsInt * 10;

var rnd = new Random();

TaskOptions taskOptions = new()
                          {
                            MaxDuration = new Duration
                                          {
                                            Seconds = 300,
                                          },
                            MaxRetries = 2,
                            Priority   = 1,
                          };

taskOptions.Options.Add("GridAppName",
                        "ArmoniK.Samples.SymphonyPackage");

taskOptions.Options.Add("GridAppVersion",
                        "2.0.0");

taskOptions.Options.Add("GridAppNamespace",
                        "ArmoniK.Samples.Symphony.Packages");


var channel      = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("Grpc__Endpoint") ?? string.Empty);
var client       = new Submitter.SubmitterClient(channel);
var resultClient = new Results.ResultsClient(channel);

var createSessionReply = client.CreateSession(new CreateSessionRequest
                                              {
                                                DefaultTaskOption = taskOptions,
                                              });

var sessionId = createSessionReply.SessionId;


var serviceConfiguration = await client.GetServiceConfigurationAsync(new Empty());

var sw = Stopwatch.StartNew();

using var asyncClientStreamingCall = client.CreateLargeTasks();

await asyncClientStreamingCall.RequestStream.WriteAsync(new CreateLargeTaskRequest
                                                        {
                                                          InitRequest = new CreateLargeTaskRequest.Types.InitRequest
                                                                        {
                                                                          SessionId   = sessionId,
                                                                          TaskOptions = taskOptions,
                                                                        },
                                                        });

var resultIds = (await resultClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
                                                               {
                                                                 SessionId = sessionId,
                                                                 Results =
                                                                 {
                                                                   Enumerable.Range(0,
                                                                                    nbTasks)
                                                                             .Select(_ => new CreateResultsMetaDataRequest.Types.ResultCreate
                                                                                          {
                                                                                            Name = Guid.NewGuid()
                                                                                                       .ToString(),
                                                                                          }),
                                                                 },
                                                               })
                                   .ConfigureAwait(false)).Results.Select(res => res.ResultId)
                                                          .ToList();


for (var i = 0; i < nbTasks; i++)
{
  await asyncClientStreamingCall.RequestStream.WriteAsync(new CreateLargeTaskRequest
                                                          {
                                                            InitTask = new InitTaskRequest
                                                                       {
                                                                         Header = new TaskRequestHeader
                                                                                  {
                                                                                    ExpectedOutputKeys =
                                                                                    {
                                                                                      resultIds[i],
                                                                                    },
                                                                                  },
                                                                       },
                                                          });

  var payloadSize = payloadSizeByte * 1024;

  for (var j = 0; j < payloadSize; j += serviceConfiguration.DataChunkMaxSize)
  {
    var chunkSize = Math.Min(serviceConfiguration.DataChunkMaxSize,
                             payloadSize - j);
    var dataBytes = new byte[chunkSize];
    rnd.NextBytes(dataBytes);

    await asyncClientStreamingCall.RequestStream.WriteAsync(new CreateLargeTaskRequest
                                                            {
                                                              TaskPayload = new DataChunk
                                                                            {
                                                                              Data = ByteString.CopyFrom(dataBytes),
                                                                            },
                                                            });
  }

  await asyncClientStreamingCall.RequestStream.WriteAsync(new CreateLargeTaskRequest
                                                          {
                                                            TaskPayload = new DataChunk
                                                                          {
                                                                            DataComplete = true,
                                                                          },
                                                          });
}

await asyncClientStreamingCall.RequestStream.WriteAsync(new CreateLargeTaskRequest
                                                        {
                                                          InitTask = new InitTaskRequest
                                                                     {
                                                                       LastTask = true,
                                                                     },
                                                        });

await asyncClientStreamingCall.RequestStream.CompleteAsync();

var createTaskReply = await asyncClientStreamingCall.ResponseAsync;

switch (createTaskReply.ResponseCase)
{
  case CreateTaskReply.ResponseOneofCase.CreationStatusList:
    Console.WriteLine("Tasks created successfully");
    break;
  case CreateTaskReply.ResponseOneofCase.None:
  case CreateTaskReply.ResponseOneofCase.Error:
    throw new Exception("Issue while creating tasks");
  default:
    throw new ArgumentOutOfRangeException();
}

var elapsedMilliseconds = sw.ElapsedMilliseconds;
Console.WriteLine($"Client submitted {nbTasks} with payloads of size {payloadSizeByte} KBytes tasks in {elapsedMilliseconds / 1000} s");
