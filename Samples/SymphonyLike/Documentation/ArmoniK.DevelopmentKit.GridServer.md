## `ArmonikDataSynapseClientService`

The main object to communicate with the control Plane from the client side  The class will connect to the control plane to createSession, SubmitTask,  Wait for result and get the result.  See an example in the project ArmoniK.Samples in the sub project  https://github.com/aneoconsulting/ArmoniK.Samples/tree/main/Samples/GridServerLike  Samples.ArmoniK.Sample.SymphonyClient
```csharp
public class ArmoniK.DevelopmentKit.GridServer.ArmonikDataSynapseClientService

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `SessionId`|SessionId|Only used for internal DO NOT USED IT  Get or Set SessionId object stored during the call of SubmitTask, SubmitSubTask,  SubmitSubTaskWithDependencies or WaitForCompletion, WaitForSubTaskCompletion or GetResults|
| `SessionId`|SubSessionId|Only used for internal DO NOT USED IT  Get or Set SubSessionId object stored during the call of SubmitTask, SubmitSubTask,  SubmitSubTaskWithDependencies or WaitForCompletion, WaitForSubTaskCompletion or GetResults|
| `TaskOptions`|TaskOptions|Set or Get TaskOptions with inside MaxDuration, Priority, AppName, VersionName and AppNamespace|

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void`|[`CancelSession()`](ArmoniK.DevelopmentKit.GridServer_methods.md#void-cancelsession)|Cancel the current Session where the SessionId is the one created previously|
| `void`|[`CloseSession()`](ArmoniK.DevelopmentKit.GridServer_methods.md#void-closesession)|Close Session. This function will disabled in nex Release. The session is automatically  closed after an other creation or after a disconnection or after end of timeout the tasks submitted|
| `SessionId`|[`CreateSession(TaskOptions taskOptions = null)`](ArmoniK.DevelopmentKit.GridServer_methods.md#sessionid-createsessiontaskoptions-taskoptions--null)|Create the session to submit task|
| `Task<IEnumerable<Tuple<String, Byte[]>>>`|[`GetResults(IEnumerable<String> taskIds)`](ArmoniK.DevelopmentKit.GridServer_methods.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)|Method to GetResults when the result is returned by a task  The method WaitForCompletion should called before these method|
| `void`|[`OpenSession(String session)`](ArmoniK.DevelopmentKit.GridServer_methods.md#void-opensessionstring-session)|Set connection to an already opened Session|
| `IEnumerable<String>`|[`SubmitTasks(IEnumerable<Byte[]> payloads)`](ArmoniK.DevelopmentKit.GridServer_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)|User method to submit task from the client  Need a client Service. In case of ServiceContainer  controlPlaneService can be null until the OpenSession is called|
| `IEnumerable<String>`|[`SubmitTasksWithDependencies(String session, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmoniK.DevelopmentKit.GridServer_methods.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)|The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `Byte[]`|[`TryGetResult(String taskId)`](ArmoniK.DevelopmentKit.GridServer_methods.md#byte-trygetresultstring-taskid)|Try to find the result of One task. If there no result, the function return byte[0]|
| `void`|[`WaitCompletion(String taskId)`](ArmoniK.DevelopmentKit.GridServer_methods.md#void-waitcompletionstring-taskid)|User method to wait for only the parent task from the client|

Static Properties

| Type | Name | Summary |
| --- | --- | --- |
| `String`|SectionControlPlan|Returns the section key Grpc from appSettings.json|

Static Methods

| Type | Name | Summary |
| --- | --- | --- |
| `TaskOptions`|`InitializeDefaultTaskOptions()`|This method is creating a default taskOptions initialization where  MaxDuration is 40 seconds, MaxRetries = 2 The app name is ArmoniK.DevelopmentKit.GridServer  The version is 1.0.0 the namespace ArmoniK.DevelopmentKit.GridServer and simple service FallBackServerAdder|

## `ServiceInvocationContext`

The ServiceInvocationContext class provides an interface for interacting with  an invocation, such as getting the session and task IDs, while it is running on an  Engine.This is an alternative to using, for example, the system properties when  running a Java Service.Using this class enables immediate updating of invocation  information.In contrast, setting the INVOCATION_INFO system property only  updates at the end of the invocation.  The ServiceInvocationContext object can be reused; the method calls always  apply to the currently executing Service Session and invocation.Make all method  calls by a service, update, or init method; if not, the method call might throw  an IllegalStateException or return invalid data.Note that you cannot call this  method from a different thread; it will fail if it is not called from the main thread.
```csharp
public class ArmoniK.DevelopmentKit.GridServer.ServiceInvocationContext

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `SessionId`|SessionId|Get the sessionId created by an createSession call before.|

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `Boolean`|[`IsEquals(String session)`](ArmoniK.DevelopmentKit.GridServer_methods.md#boolean-isequalsstring-session)|Check if the session is the same as previously created|

