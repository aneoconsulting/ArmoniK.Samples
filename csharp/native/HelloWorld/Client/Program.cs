// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2023. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Api.Client;
using ArmoniK.Api.Client.Options;
using ArmoniK.Api.Client.Submitter;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Api.gRPC.V1.Tasks;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;

using static System.Console;


namespace ArmoniK.Samples.HelloWorld.Client
{
  internal static class Program
  {
    /// <summary>
    ///   Method for sending task and retrieving their results from ArmoniK
    /// </summary>
    /// <param name="endpoint">The endpoint url of ArmoniK's control plane</param>
    /// <param name="partition">Partition Id of the matching worker</param>
    /// <returns>
    ///   Task representing the asynchronous execution of the method
    /// </returns>
    /// <exception cref="Exception">Issues with results from tasks</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown response type from control plane</exception>
    internal static async Task Run(string endpoint,
                                   string partition)
    {
      var builder       = new ConfigurationBuilder().AddEnvironmentVariables();
      var configuration = builder.Build();

      var options = configuration.GetSection(GrpcClient.SettingSection)
                                 .Get<GrpcClient>() ?? new GrpcClient();

      // Use grpc options with entrypoint from command line
      options.Endpoint = endpoint;

      // Create gRPC channel to connect with ArmoniK control plane
      var channel = GrpcChannelFactory.CreateChannel(options!);

      // Create client for task submission
      var taskClient = new Tasks.TasksClient(channel);

      // Create client for result creation
      var resultClient = new Results.ResultsClient(channel);

      // Create client for session creation
      var sessionClient = new Sessions.SessionsClient(channel);

      // Create client for events listening
      var eventClient = new Events.EventsClient(channel);

      // Default task options that will be used by each task if not overwritten when submitting tasks
      var taskOptions = new TaskOptions
                        {
                          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                          MaxRetries  = 2,
                          Priority    = 1,
                          PartitionId = partition,
                        };

      // Request for session creation with default task options and allowed partitions for the session
      var createSessionReply = sessionClient.CreateSession(new CreateSessionRequest
                                                           {
                                                             DefaultTaskOption = taskOptions,
                                                             PartitionIds =
                                                             {
                                                               partition,
                                                             },
                                                           });

      WriteLine($"sessionId: {createSessionReply.SessionId}");

      // Create the result metadata and keep the id for task submission
      var resultId = resultClient.CreateResultsMetaData(new CreateResultsMetaDataRequest
                                                        {
                                                          SessionId = createSessionReply.SessionId,
                                                          Results =
                                                          {
                                                            new CreateResultsMetaDataRequest.Types.ResultCreate
                                                            {
                                                              Name = "Result",
                                                            },
                                                          },
                                                        })
                                 .Results.Single()
                                 .ResultId;

      // Create the payload metadata (a result) and upload data at the same time
      var payloadId = resultClient.CreateResults(new CreateResultsRequest
                                                 {
                                                   SessionId = createSessionReply.SessionId,
                                                   Results =
                                                   {
                                                     new CreateResultsRequest.Types.ResultCreate
                                                     {
                                                       Data = UnsafeByteOperations.UnsafeWrap(Encoding.ASCII.GetBytes("Hello")),
                                                       Name = "Payload",
                                                     },
                                                   },
                                                 })
                                  .Results.Single()
                                  .ResultId;

      // Submit task with payload and result ids
      var submitTasksResponse = taskClient.SubmitTasks(new SubmitTasksRequest
                                                       {
                                                         SessionId = createSessionReply.SessionId,
                                                         TaskCreations =
                                                         {
                                                           new SubmitTasksRequest.Types.TaskCreation
                                                           {
                                                             PayloadId = payloadId,
                                                             ExpectedOutputKeys =
                                                             {
                                                               resultId,
                                                             },
                                                           },
                                                         },
                                                       });

      WriteLine($"Task id {submitTasksResponse.TaskInfos.Single().TaskId}");

      // Wait for task end and result availability
      await eventClient.WaitForResultsAsync(createSessionReply.SessionId,
                                            new List<string>
                                            {
                                              resultId,
                                            },
                                            CancellationToken.None);

      // Download result
      var result = await resultClient.DownloadResultData(createSessionReply.SessionId,
                                                         resultId,
                                                         CancellationToken.None);

      WriteLine($"resultId: {resultId}, data: {Encoding.ASCII.GetString(result)}");
    }

    public static async Task<int> Main(string[] args)
    {
      // Define the options for the application with their description and default value
      var endpoint = new Option<string>("--endpoint",
                                        description: "Endpoint for the connection to ArmoniK control plane.",
                                        getDefaultValue: () => "http://localhost:5001");
      var partition = new Option<string>("--partition",
                                         description: "Name of the partition to which submit tasks.",
                                         getDefaultValue: () => "default");

      // Describe the application and its purpose
      var rootCommand =
        new
          RootCommand($"Hello World demo for ArmoniK.\nIt sends a task to ArmoniK in the given partition <{partition.Name}>. The task receives 'Hello' as input string and, for the result that will be returned by the task, append the word 'World' and the resultID to the input. Then, the client retrieves and prints the result of the task.\nArmoniK endpoint location is provided through <{endpoint.Name}>");

      // Add the options to the parser
      rootCommand.AddOption(endpoint);
      rootCommand.AddOption(partition);

      // Configure the handler to call the function that will do the work
      rootCommand.SetHandler(Run,
                             endpoint,
                             partition);

      // Parse the command line parameters and call the function that represents the application
      return await rootCommand.InvokeAsync(args);
    }
  }
}
