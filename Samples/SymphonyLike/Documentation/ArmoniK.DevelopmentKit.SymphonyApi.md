## `ServiceContext`

```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.ServiceContext

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `String`|ApplicationName||
| `String`|AppNamespace||
| `String`|ClientLibVersion||
| `String`|ServiceName||

## `SessionContext`

Container for the information associated with a particular Session.  Such information may be required during the servicing of a task from a Session.
```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.SessionContext

```

Fields

| Type | Name | Summary |
| --- | --- | --- |
| `Int32`|TimeRemoteDebug||

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `String`|ClientLibVersion||
| `Boolean`|IsDebugMode||
| `String`|SessionId||

## `TaskContext`

Provides the context for the task that is bound to the given service invocation
```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.TaskContext

```

Fields

| Type | Name | Summary |
| --- | --- | --- |
| `Byte[]`|Payload||

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `IDictionary<String, String>`|ClientOptions||
| `IEnumerable<String>`|DependenciesTaskIds||
| `String`|SessionId||
| `String`|TaskId||
| `Byte[]`|TaskInput|The customer payload to deserialize by the customer|

