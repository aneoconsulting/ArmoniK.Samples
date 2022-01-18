## `ArmonikSymphonyClient`

The main object to communicate with the control Plane from the client side  The class will connect to the control plane to createSession, SubmitTask,  Wait for result and get the result.  See an example in the project ArmoniK.Samples in the sub project  https://github.com/aneoconsulting/ArmoniK.Samples/tree/main/Samples/SymphonyLike  Samples.ArmoniK.Sample.SymphonyClient
```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.Client.ArmonikSymphonyClient

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `String`|ParentTaskId||
| `String`|SectionControlPlan|Returns the section key Grpc from appSettings.json|
| `SessionId`|SessionId|Only used for internal DO NOT USED IT  Get or Set SessionId object stored during the call of SubmitTask, SubmitSubTask,  SubmitSubTaskWithDependencies or WaitForCompletion, WaitForSubTaskCompletion or GetResults|
| `SessionId`|SubSessionId|Only used for internal DO NOT USED IT  Get or Set SubSessionId object stored during the call of SubmitTask, SubmitSubTask,  SubmitSubTaskWithDependencies or WaitForCompletion, WaitForSubTaskCompletion or GetResults|
| `TaskOptions`|TaskOptions|Set or Get TaskOptions with inside MaxDuration, Priority, AppName, VersionName and AppNamespace|

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `String`|[`CreateSession(TaskOptions taskOptions = null)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-createsessiontaskoptions-taskoptions--null)|Create the session to submit task|
| `Task<IEnumerable<Tuple<String, Byte[]>>>`|[`GetResults(IEnumerable<String> taskIds)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)|Method to GetResults when the result is returned by a task  The method WaitForCompletion should called before these method|
| `void`|[`OpenSession(SessionId session)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-opensessionsessionid-session)|Set connection to an already opened Session|
| `IEnumerable<String>`|[`SubmitSubTasks(String session, String parentTaskId, IEnumerable<Byte[]> payloads)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtasksstring-session-string-parenttaskid-ienumerablebyte-payloads)|The method to submit sub task inside a parent task  Use this method only on server side developpement|
| `IEnumerable<String>`|[`SubmitSubtasksWithDependencies(String session, String parentTaskId, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtaskswithdependenciesstring-session-string-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)|The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `String`|[`SubmitSubtaskWithDependencies(String session, String parentId, Byte[] payload, IList<String> dependencies)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-submitsubtaskwithdependenciesstring-session-string-parentid-byte-payload-iliststring-dependencies)|The method to submit One SubTask with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `IEnumerable<String>`|[`SubmitTasks(IEnumerable<Byte[]> payloads)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)|User method to submit task from the client  Need a client Service. In case of ServiceContainer  controlPlaneService can be null until the OpenSession is called|
| `IEnumerable<String>`|[`SubmitTasksWithDependencies(String session, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)|The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `Byte[]`|[`TryGetResult(String taskId)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#byte-trygetresultstring-taskid)|Try to find the result of One task. If there no result, the function return byte[0]|
| `void`|[`WaitCompletion(String taskId)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitcompletionstring-taskid)|User method to wait for only the parent task from the client|
| `void`|[`WaitSubtasksCompletion(String parentTaskId)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitsubtaskscompletionstring-parenttaskid)|Wait for the taskIds and all its dependencies taskIds|

