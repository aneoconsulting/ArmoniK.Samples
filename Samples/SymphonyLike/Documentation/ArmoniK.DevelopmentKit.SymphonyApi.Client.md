## `ArmonikSymphonyClient`

The main object to communicate with the control Plane from the client side  The class will connect to the control plane to createSession, SubmitTask,  Wait for result and get the result.  See an example in the project ArmoniK.Samples in the sub project  https://github.com/aneoconsulting/ArmoniK.Samples/tree/main/Samples/SymphonyLike  Samples.ArmoniK.Sample.SymphonyClient
```csharp
public class ArmoniK.DevelopmentKit.SymphonyApi.Client.ArmonikSymphonyClient

```

Properties

| Type | Name | Summary |
| --- | --- | --- |
| `String`|SectionControlPlan|Returns the section key Grpc from appSettings.json|

Methods

| Type | Name | Summary |
| --- | --- | --- |
| `SessionService`|[`CreateSession(TaskOptions taskOptions = null)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#sessionservice-createsessiontaskoptions-taskoptions--null)|Create the session to submit task|
| `SessionService`|[`OpenSession(SessionId sessionId, IDictionary<String, String> clientOptions)`](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#sessionservice-opensessionsessionid-sessionid-idictionarystring-string-clientoptions)|Open the session already created to submit task|

