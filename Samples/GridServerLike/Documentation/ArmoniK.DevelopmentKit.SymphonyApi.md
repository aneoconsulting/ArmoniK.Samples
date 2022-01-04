## `ServiceContainerBase`

The Class ServiceContainerBase (Old name was IServiceContainer) is an abstract class  that have to be implemented by each class wanted to be loaded as new Application  See an example in the project ArmoniK.Samples in the sub project  https://github.com/aneoconsulting/ArmoniK.Samples/tree/main/Samples/SymphonyLike  Samples.ArmoniK.Sample.SymphonyPackages
```csharp
public abstract class ArmoniK.DevelopmentKit.SymphonyApi.ServiceContainerBase

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `ArmonikSymphonyClient` | ClientService |  | 
| `IConfiguration` | Configuration | Get or Set Configuration | 
| `SessionId` | SessionId | Get or Set SubSessionId object stored during the call of SubmitTask, SubmitSubTask,  SubmitSubTaskWithDependencies or WaitForCompletion, WaitForSubTaskCompletion or GetResults | 
| `String` | TaskId | Get or set the taskId (ONLY INTERNAL USED) | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Configure(`IConfiguration` configuration, `IDictionary<String, String>` clientOptions) | The configure method is an internal call to prepare the ServiceContainer.  Its holds several configuration coming from the Client call | 
| `Byte[]` | GetResult(`String` taskId) | Get Result from compute reply | 
| `void` | OnCreateService(`ServiceContext` serviceContext) | The middleware triggers the invocation of this handler just after a Service Instance is started.  The application developer must put any service initialization into this handler.  Default implementation does nothing. | 
| `void` | OnDestroyService(`ServiceContext` serviceContext) | The middleware triggers the invocation of this handler just before a Service Instance is destroyed.  This handler should do any cleanup for any resources that were used in the onCreateService() method. | 
| `Byte[]` | OnInvoke(`SessionContext` sessionContext, `TaskContext` taskContext) | The middleware triggers the invocation of this handler every time a task input is  sent to the service to be processed.  The actual service logic should be implemented in this method. This is the only  method that is mandatory for the application developer to implement. | 
| `void` | OnSessionEnter(`SessionContext` sessionContext) | This handler is executed once after the callback OnCreateService and before the OnInvoke | 
| `void` | OnSessionLeave(`SessionContext` sessionContext) | The middleware triggers the invocation of this handler to unbind the Service Instance from its owning Session.  This handler should do any cleanup for any resources that were used in the onSessionEnter() method. | 
| `IEnumerable<String>` | SubmitSubTasks(`IEnumerable<Byte[]>` payloads, `String` parentTaskIds) | User method to submit task from the service | 
| `IEnumerable<String>` | SubmitSubtasksWithDependencies(`String` parentId, `IEnumerable<Tuple<Byte[], IList<String>>>` payloadWithDependencies) | The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully | 
| `String` | SubmitSubtaskWithDependencies(`String` parentId, `Byte[]` payload, `IList<String>` dependencies) | The method to submit One Subtask with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully | 
| `IEnumerable<String>` | SubmitTasks(`IEnumerable<Byte[]>` payloads) | User method to submit task from the service | 
| `IEnumerable<String>` | SubmitTasksWithDependencies(`IEnumerable<Tuple<Byte[], IList<String>>>` payloadWithDependencies) | The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully | 
| `void` | WaitForCompletion(`String` taskId) | User method to wait for only the parent task from the client | 
| `void` | WaitForSubTasksCompletion(`String` taskId) | User method to wait for SubTasks from the client | 


## `ServiceContainerBaseExt`

This is the ServiceContainerBase extensions used to extend SubmitSubTasks
```csharp
public static class ArmoniK.DevelopmentKit.SymphonyApi.ServiceContainerBaseExt

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | SubmitSubTask(this `ServiceContainerBase` serviceContainerBase, `Byte[]` payload, `String` parentId) | User method to submit task from the service | 
| `IEnumerable<String>` | SubmitSubTasks(this `ServiceContainerBase` serviceContainerBase, `IEnumerable<Byte[]>` payload, `String` parentId) | User method to submit task from the service | 
| `String` | SubmitTask(this `ServiceContainerBase` serviceContainerBase, `Byte[]` payload) | User method to submit task from the service | 
| `String` | SubmitTaskWithDependencies(this `ServiceContainerBase` serviceContainerBase, `Byte[]` payload, `IList<String>` dependencies) | The method to submit One task with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully | 


## `ServiceContext`

```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.ServiceContext

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | ApplicationName |  | 
| `String` | AppNamespace |  | 
| `String` | ClientLibVersion |  | 
| `String` | ServiceName |  | 


## `SessionContext`

Container for the information associated with a particular Session.  Such information may be required during the servicing of a task from a Session.
```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.SessionContext

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | TimeRemoteDebug |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | ClientLibVersion |  | 
| `Boolean` | IsDebugMode |  | 
| `String` | SessionId |  | 


## `TaskContext`

Provides the context for the task that is bound to the given service invocation
```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.TaskContext

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Byte[]` | Payload |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `IDictionary<String, String>` | ClientOptions |  | 
| `IEnumerable<String>` | DependenciesTaskIds |  | 
| `String` | SessionId |  | 
| `String` | TaskId |  | 
| `Byte[]` | TaskInput | The customer payload to deserialize by the customer | 


