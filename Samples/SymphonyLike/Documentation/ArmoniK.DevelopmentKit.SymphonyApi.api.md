## `ServiceContainerBase`

The Class ServiceContainerBase (Old name was IServiceContainer) is an abstract class  that have to be implemented by each class wanted to be loaded as new Application  See an example in the project ArmoniK.Samples in the sub project  https://github.com/aneoconsulting/ArmoniK.Samples/tree/main/Samples/SymphonyLike  Samples.ArmoniK.Sample.SymphonyPackages
```csharp
public abstract class ArmoniK.DevelopmentKit.SymphonyApi.api.ServiceContainerBase

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `ArmonikSymphonyClient`|ClientService||
| `IConfiguration`|Configuration|Get or Set Configuration|
| `ILogger<ServiceContainerBase>`|Log|Get access to Logger with Log.Lo.|
| `SessionId`|SessionId|Get or Set SubSessionId object stored during the call of SubmitTask, SubmitSubTask,  SubmitSubTaskWithDependencies or WaitForCompletion, WaitForSubTaskCompletion or GetResults|
| `String`|TaskId|Get or set the taskId (ONLY INTERNAL USED)|

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void`|[`Configure(IConfiguration configuration, IDictionary<String, String> clientOptions)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-configureiconfiguration-configuration-idictionarystring-string-clientoptions)|The configure method is an internal call to prepare the ServiceContainer.  Its holds several configuration coming from the Client call|
| `Byte[]`|[`GetResult(String taskId)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-getresultstring-taskid)|Get Result from compute reply|
| `void`|[`OnCreateService(ServiceContext serviceContext)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-oncreateserviceservicecontext-servicecontext)|The middleware triggers the invocation of this handler just after a Service Instance is started.  The application developer must put any service initialization into this handler.  Default implementation does nothing.|
| `void`|[`OnDestroyService(ServiceContext serviceContext)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-ondestroyserviceservicecontext-servicecontext)|The middleware triggers the invocation of this handler just before a Service Instance is destroyed.  This handler should do any cleanup for any resources that were used in the onCreateService() method.|
| `Byte[]`|[`OnInvoke(SessionContext sessionContext, TaskContext taskContext)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-oninvokesessioncontext-sessioncontext-taskcontext-taskcontext)|The middleware triggers the invocation of this handler every time a task input is  sent to the service to be processed.  The actual service logic should be implemented in this method. This is the only  method that is mandatory for the application developer to implement.|
| `void`|[`OnSessionEnter(SessionContext sessionContext)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionentersessioncontext-sessioncontext)|This handler is executed once after the callback OnCreateService and before the OnInvoke|
| `void`|[`OnSessionLeave(SessionContext sessionContext)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionleavesessioncontext-sessioncontext)|The middleware triggers the invocation of this handler to unbind the Service Instance from its owning Session.  This handler should do any cleanup for any resources that were used in the onSessionEnter() method.|
| `IEnumerable<String>`|[`SubmitTasks(IEnumerable<Byte[]> payloads)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)|User method to submit task from the service|
| `IEnumerable<String>`|[`SubmitTasksWithDependencies(IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)|The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully|
| `void`|[`WaitForCompletion(String taskId)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforcompletionstring-taskid)|User method to wait for only the parent task from the client|
| `void`|[`WaitForSubTasksCompletion(String taskId)`](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforsubtaskscompletionstring-taskid)|User method to wait for SubTasks from the client|

