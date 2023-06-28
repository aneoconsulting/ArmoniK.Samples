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
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArmoniK.Api.Client.Options;
using ArmoniK.Api.Client.Submitter;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.Submitter;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using static System.Console;


namespace ArmoniK.Samples.HelloWorld.Client
{
  internal class Program
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
      // Create gRPC channel to connect with ArmoniK control plane
      var channel = GrpcChannelFactory.CreateChannel(new GrpcClient
                                                     {
                                                       Endpoint = endpoint,
                                                     });

      // Create client for task submission
      var submitterClient = new Submitter.SubmitterClient(channel);

      // Create client for result creation
      var resultClient = new Results.ResultsClient(channel);

      // Default task options that will be used by each task if not overwritten when submitting tasks
      var taskOptions = new TaskOptions
                        {
                          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                          MaxRetries  = 2,
                          Priority    = 1,
                          PartitionId = partition,
                        };

      // Request for session creation with default task options and allowed partitions for the session
      var createSessionReply = submitterClient.CreateSession(new CreateSessionRequest
                                                             {
                                                               DefaultTaskOption = taskOptions,
                                                               PartitionIds =
                                                               {
                                                                 partition,
                                                               },
                                                             });

      // Create the result id
      var resultId = resultClient.CreateResultsMetaData(new CreateResultsMetaDataRequest
                                                        {
                                                          SessionId = createSessionReply.SessionId,
                                                          Results =
                                                          {
                                                            new CreateResultsMetaDataRequest.Types.ResultCreate()
                                                            {
                                                              Name = Guid.NewGuid()
                                                                         .ToString(),
                                                            },
                                                          },
                                                        }).Results.First().ResultId;

      // Task request with payload for the task
      // Also contains the list of results that will be created by the task
      var taskRequest = new TaskRequest
                        {
                          ExpectedOutputKeys =
                          {
                            resultId,
                          },
                          // Avoid unnecessary copy but data could be modified during sending if the
                          // reference to the object is still available
                          // This is not the case here
                          Payload = UnsafeByteOperations.UnsafeWrap(Encoding.ASCII.GetBytes("Hello")),
                        };

      var createTaskReply = await submitterClient.CreateTasksAsync(createSessionReply.SessionId,
                                                                   null,
                                                                   new[]
                                                                   {
                                                                     taskRequest,
                                                                   })
                                                 .ConfigureAwait(false);

      WriteLine($"sessionId: {createSessionReply.SessionId}");


      // Result request that the describes the result we want
      var resultRequest = new ResultRequest
                          {
                            ResultId = resultId,
                            Session  = createSessionReply.SessionId,
                          };

      // Blocking call that waits for the availability of the result
      var availabilityReply = submitterClient.WaitForAvailability(resultRequest);

      // Process the result according the the result of the wait
      switch (availabilityReply.TypeCase)
      {
        // In this case, the reply is not properly completed by the control plane
        case AvailabilityReply.TypeOneofCase.None:
          throw new Exception("Issue with Server !");

        // The result is available and we can retrieve it
        case AvailabilityReply.TypeOneofCase.Ok:
          // Download the result from the control plane, convert it from byte to string and print it on the console
          var result = await submitterClient.GetResultAsync(resultRequest)
                                            .ConfigureAwait(false);
          WriteLine($"resultId: {resultId}, data: {Encoding.ASCII.GetString(result)}");

          break;

        // The result is in error, meaning the task responsible to create it encountered an error
        case AvailabilityReply.TypeOneofCase.Error:
          throw new Exception($"Task in Error - {availabilityReply.Error.TaskId} : {availabilityReply.Error.Errors}");

        // The task was not completed, should not happen since we wait for the availability of the result
        case AvailabilityReply.TypeOneofCase.NotCompletedTask:
          throw new Exception($"Task not completed - result id {resultId}");

        // An unexpected response was received
        default:
          throw new ArgumentOutOfRangeException(nameof(availabilityReply.TypeCase));
      }
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
