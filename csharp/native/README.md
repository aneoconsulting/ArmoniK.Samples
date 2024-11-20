# ArmoniK Samples, how to create one from scratch

## Prerequisites 

To get started, ensure you have: 
1. **.NET SDK 6.0** or later installed on your machine.
2. Followed the tutorial **How to launch HelloWorld SampleHelloWorld Sample**

## How it works 

### Overview: Client & Worker

#### 1. The Client

The client is responsible for the initial setup and task submission. It interacts with the ArmoniK control plane to:

- Create a session.
- Define and submit the task payload (data for computation).
- Wait for the result and retrieve it once the task is completed.

#### 2. The Worker

The worker handles the execution of tasks and creation of subtasks. It:

- Processes the payload sent by the client.
- If needed, splits the computation into smaller subtasks and submits them back 
- Returns the final computed result once all subtasks are processed.


### Summary of Responsibilities
 

| Component	                    | Responsibilities |
| :----------------------------: | :--------------: |
| **Client** | 	Session creation, task submission, waiting for results, validating final computation.| 
| **Worker** | Task processing, subtask creation, result aggregation, sending final result.



## Workflow

### Step 1: The Client submits a task

The client creates a session, defines the computation parameters (e.g., two integers for multiplication), and submits the task. Here's what happens:

- **Session Creation**: The client initializes a session to group related tasks.
- **Payload Creation**: The client prepares a payload containing the input data.
- **Task Submission**: The task is sent to the ArmoniK compute plane.

```csharp
var payload = new int[] { x, y, 0, sign }; // x and y are integers to multiply
var payloadBytes = payload.SelectMany(BitConverter.GetBytes).ToArray();

var createSessionReply = sessionClient.CreateSession(new CreateSessionRequest
{
    DefaultTaskOption = taskOptions,
    PartitionIds = { partition },
});

var submitTasksResponse = taskClient.SubmitTasks(new SubmitTasksRequest
{
    SessionId = createSessionReply.SessionId,
    TaskCreations = { new SubmitTasksRequest.Types.TaskCreation
    {
        PayloadId = payloadId,
        ExpectedOutputKeys = { resultId },
    }},
});
```

### Step 2: The Worker processes the task

When the worker receives the task, it:

1. **Parses the Payload**: Extracts the computation parameters (e.g., integers x, y, etc.).
2. **Performs Computation**: Computes a partial result or completes the task.
3. **Creates Subtasks (if needed)**: If the computation is complex (e.g., recursive multiplication), the worker generates subtasks with updated parameters and submits them back to ArmoniK.
4. **Sends Final Result**: Once all subtasks are resolved, the worker computes the final result and sends it back to the client.

```csharp
if (y > 0)
{
    // Create subtask payload
    var subTaskPayload = new int[] { x, y - 1, z + x, sign };
    var subTaskPayloadBytes = subTaskPayload.SelectMany(BitConverter.GetBytes).ToArray();

    // Submit subtask
    var subTaskResultId = (await taskHandler.CreateResultsAsync(new[]
    {
        new CreateResultsRequest.Types.ResultCreate
        {
            Data = UnsafeByteOperations.UnsafeWrap(subTaskPayloadBytes)
        }
    })).Results.Single().ResultId;

    await taskHandler.SubmitTasksAsync(new[]
    {
        new SubmitTasksRequest.Types.TaskCreation
        {
            PayloadId = subTaskResultId,
            ExpectedOutputKeys = { taskHandler.ExpectedResults.Single() }
        }
    }, taskHandler.TaskOptions);
}
else
{
    // Final result
    var resultId = taskHandler.ExpectedResults.Single();
    int finalResult = z * sign;
    await taskHandler.SendResult(resultId, BitConverter.GetBytes(finalResult)).ConfigureAwait(false);
}
```` 

### Step 3: The Client retrieves the result

After all tasks and subtasks are processed, the client retrieves the result using the result client. It can then validate the result against the expected value.

```csharp
await eventClient.WaitForResultsAsync(createSessionReply.SessionId, new List<string> { resultId }, 100, 1, CancellationToken.None);

var resultData = await resultClient.DownloadResultData(createSessionReply.SessionId, resultId, CancellationToken.None);
if (resultData == null || !resultData.Any())
{
    throw new Exception("No result available.");
}

var finalResult = BitConverter.ToInt32(resultData, 0);
Console.WriteLine($"Final result: {finalResult}");
```
## Example Run

For detailed instructions on how to run the example, refer to the  documentation: [How to Launch HelloWorld Sample](https://aneoconsulting.github.io/ArmoniK/guide/how-to/how-to-launch-helloworld-sample).