// See https://aka.ms/new-console-template for more information

using ArmoniK.Api.gRPC.V1;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Empty = ArmoniK.Api.gRPC.V1.Empty;

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


var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("Grpc__Endpoint") ?? string.Empty);
var client = new Submitter.SubmitterClient(channel);

var sessionId = Guid.NewGuid().ToString();

var createSessionReply = client.CreateSession(new CreateSessionRequest
{
  DefaultTaskOption = taskOptions,
  Id = sessionId,
});

switch (createSessionReply.ResultCase)
{
  case CreateSessionReply.ResultOneofCase.Ok:
    Console.WriteLine($"Session {sessionId} created with success");
    break;
  case CreateSessionReply.ResultOneofCase.None:
  case CreateSessionReply.ResultOneofCase.Error:
    throw new Exception("Issue while creating session");
  default:
    throw new ArgumentOutOfRangeException();
}


var serviceConfiguration = await client.GetServiceConfigurationAsync(new Empty());

using var asyncClientStreamingCall = client.CreateLargeTasks();

await asyncClientStreamingCall.RequestStream.WriteAsync(new CreateLargeTaskRequest
{
  InitRequest = new CreateLargeTaskRequest.Types.InitRequest
  {
    SessionId = sessionId,
    TaskOptions = taskOptions,
  },
});


for (int i = 0; i < 2000; i++)
{
  var taskId = Guid.NewGuid().ToString();

  await asyncClientStreamingCall.RequestStream.WriteAsync(new CreateLargeTaskRequest
  {
    InitTask = new InitTaskRequest
    {
      Header = new TaskRequestHeader
      {
        Id = taskId,
        ExpectedOutputKeys =
        {
          taskId,
        },
      },
    },
  });

  var payloadSize = 100 * 1024;

  for (int j = 0; j < payloadSize; j += serviceConfiguration.DataChunkMaxSize)
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

switch (createTaskReply.DataCase)
{
  case CreateTaskReply.DataOneofCase.Successfull:
    Console.WriteLine("Tasks created successfully");
    break;
  case CreateTaskReply.DataOneofCase.None:
  case CreateTaskReply.DataOneofCase.NonSuccessfullIds:
    throw new Exception("Issue while creating tasks");
  default:
    throw new ArgumentOutOfRangeException();
}