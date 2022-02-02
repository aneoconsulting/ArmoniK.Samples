## `SessionService`

The class SessionService will be create each time the function CreateSession or OpenSession will  be called by client or by the worker.
```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.Client.api.SessionService

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `ILogger<SessionService>`|Logger||
| `String`|ParentTaskId||
| `SessionId`|SessionId|Only used for internal DO NOT USED IT  Get or Set SessionId object stored during the call of SubmitTask, SubmitSubTask,  SubmitSubTaskWithDependencies or WaitForCompletion, WaitForSubTaskCompletion or GetResults|
| `SessionOptions`|SessionOptions||
| `SessionId`|SubSessionId||
| `TaskOptions`|TaskOptions|Set or Get TaskOptions with inside MaxDuration, Priority, AppName, VersionName and AppNamespace|

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `Task<IEnumerable<Tuple<String, Byte[]>>>`|[`GetResults(IEnumerable<String> taskIds)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)|Method to GetResults when the result is returned by a task  The method WaitForCompletion should called before these method|
| `void`|[`OpenSession(SessionId session)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#void-opensessionsessionid-session)|Set connection to an already opened Session|
| `IEnumerable<String>`|[`SubmitSubTasks(String parentTaskId, IEnumerable<Byte[]> payloads)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#ienumerablestring-submitsubtasksstring-parenttaskid-ienumerablebyte-payloads)|The method to submit sub task inside a parent task  Use this method only on server side developpement|
| `IEnumerable<String>`|[`SubmitSubtasksWithDependencies(String parentTaskId, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#ienumerablestring-submitsubtaskswithdependenciesstring-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)|The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `String`|[`SubmitSubtaskWithDependencies(String parentId, Byte[] payload, IList<String> dependencies)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#string-submitsubtaskwithdependenciesstring-parentid-byte-payload-iliststring-dependencies)|The method to submit One SubTask with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `IEnumerable<String>`|[`SubmitTasks(IEnumerable<Byte[]> payloads)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)|User method to submit task from the client  Need a client Service. In case of ServiceContainer  controlPlaneService can be null until the OpenSession is called|
| `IEnumerable<String>`|[`SubmitTasksWithDependencies(IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)|The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `String`|[`ToString()`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#string-tostring)|Returns a string that represents the current object.|
| `Byte[]`|[`TryGetResult(String taskId)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#byte-trygetresultstring-taskid)|Try to find the result of One task. If there no result, the function return byte[0]|
| `IEnumerable<Tuple<String, Byte[]>>`|[`TryGetResults(IEnumerable<String> taskIds)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#ienumerabletuplestring-byte-trygetresultsienumerablestring-taskids)|Try to get result of a list of taskIds|
| `void`|[`WaitForTaskCompletion(String taskId)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#void-waitfortaskcompletionstring-taskid)|User method to wait for only the parent task from the client|
| `void`|[`WaitForTasksCompletion(IEnumerable<String> taskIds)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#void-waitfortaskscompletionienumerablestring-taskids)|User method to wait for only the parent task from the client|
| `void`|[`WaitSubtasksCompletion(String parentTaskId)`](ArmoniK.DevelopmentKit.SymphonyApi.Client.api_methods.md#void-waitsubtaskscompletionstring-parenttaskid)|Wait for the taskIds and all its dependencies taskIds|

