## `IServiceInvocationHandler`

The interface from which the handler must inherit to be considered as a handler of service  in the method LocalExecute, Execute or Submit
```csharp
public interface ArmoniK.DevelopmentKit.GridServer.Client.IServiceInvocationHandler

```

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void`|[`HandleError(ServiceInvocationException e, String taskId)`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#void-handleerrorserviceinvocationexception-e-string-taskid)|The callBack method which has to be implemented to retrieve error or exception|
| `void`|[`HandleResponse(Object response, String taskId)`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#void-handleresponseobject-response-string-taskid)|The callBack method which has to be implemented to retrieve response from the server|

## `Properties`

```csharp
public class ArmoniK.DevelopmentKit.GridServer.Client.Properties

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `IConfiguration`|Configuration||
| `String`|ConnectionAddress||
| `Int32`|ConnectionPort||
| `String`|ConnectionString||
| `String`|Protocol||
| `TaskOptions`|TaskOptions||

Static Fields

| Type | Name | Summary |
| --- | --- | --- |
| `TaskOptions`|DefaultTaskOptions||

## `Service`

This class is instantiated by ServiceFactory and allows to execute task on ArmoniK  Grid.
```csharp
public class ArmoniK.DevelopmentKit.GridServer.Client.Service
    : IDisposable

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `Task`|HandlerResponse||
| `SessionId`|SessionId|Propoerty Get the SessionId|
| `Dictionary<String, Task>`|TaskWarehouse||

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void`|[`Destroy()`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#void-destroy)|The method to destroy the service and close the session|
| `void`|[`Dispose()`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#void-dispose)|Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.|
| `Tuple<String, Object>`|[`Execute(String methodName, Object[] arguments)`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#tuplestring-object-executestring-methodname-object-arguments)|This method is used to execute task and waiting after the result.  the method will return the result of the execution until the grid returns the task result|
| `Boolean`|[`IsDestroyed()`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#boolean-isdestroyed)|Check if this service has been destroyed before that call|
| `Object`|[`LocalExecute(Object service, String methodName, Object[] arguments)`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#object-localexecuteobject-service-string-methodname-object-arguments)|This function execute code locally with the same configuration as Armonik Grid execution  The method needs the Service to execute, the method name to call and arguments of method to pass|
| `String`|[`Submit(String methodName, Object[] arguments, IServiceInvocationHandler handler)`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#string-submitstring-methodname-object-arguments-iserviceinvocationhandler-handler)|The method submit will execute task asynchronously on the server|

## `ServiceFactory`

```csharp
public class ArmoniK.DevelopmentKit.GridServer.Client.ServiceFactory

```

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `Service`|[`CreateService(String serviceType, Properties props)`](ArmoniK.DevelopmentKit.GridServer.Client_methods.md#service-createservicestring-servicetype-properties-props)|The method to create new Service|

Static Methods

| Type | Name | Summary |
| --- | --- | --- |
| `ServiceFactory`|`GetInstance()`|Get a single instance of ServiceFactory to create new Service|

## `ServiceInvocationException`

```csharp
public class ArmoniK.DevelopmentKit.GridServer.Client.ServiceInvocationException
    : Exception, ISerializable

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `String`|Message||

