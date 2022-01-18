## Class ArmonikDataSynapseClientService

---

### [`SessionId CreateSession(TaskOptions taskOptions = null)`](ArmonikDataSynapseClientService.md#sessionid-createsessiontaskoptions-taskoptions--null)

#### Description

Create the session to submit task

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskOptions**|`TaskOptions`|Optional parameter to set TaskOptions during the Session creation|

#### Returns

`SessionId`Not Documented

---

### [`void OpenSession(String session)`](ArmonikDataSynapseClientService.md#void-opensessionstring-session)

#### Description

Set connection to an already opened Session

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`String`|SessionId previously opened|

#### Returns

`void`

---

### [`void WaitCompletion(String taskId)`](ArmonikDataSynapseClientService.md#void-waitcompletionstring-taskid)

#### Description

User method to wait for only the parent task from the client

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task id of the task to wait for|

#### Returns

`void`

---

### [`Task<IEnumerable<Tuple<String, Byte[]>>> GetResults(IEnumerable<String> taskIds)`](ArmonikDataSynapseClientService.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)

#### Description

Method to GetResults when the result is returned by a task  The method WaitForCompletion should called before these method

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskIds**|`IEnumerable<String>`|The Task Ids list of the tasks which the result is expected|

#### Returns

`Task<IEnumerable<Tuple<String, Byte[]>>>` : return a dictionary with key taskId and payload

---

### [`IEnumerable<String> SubmitTasks(IEnumerable<Byte[]> payloads)`](ArmonikDataSynapseClientService.md#ienumerablestring-submittasksienumerablebyte-payloads)

#### Description

User method to submit task from the client  Need a client Service. In case of ServiceContainer  controlPlaneService can be null until the OpenSession is called

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **payloads**|`IEnumerable<Byte[]>`|The user payload list to execute. General used for subTasking.|

#### Returns

`IEnumerable<String>`Not Documented

---

### [`IEnumerable<String> SubmitTasksWithDependencies(String session, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmonikDataSynapseClientService.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)

#### Description

The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`String`|The session Id where the task will be attached|
| **payloadWithDependencies**|`IEnumerable<Tuple<Byte[], IList<String>>>`|A list of Tuple(taskId, Payload) in dependence of those created tasks|

#### Returns

`IEnumerable<String>` : return a list of taskIds of the created tasks

---

### [`Byte[] TryGetResult(String taskId)`](ArmonikDataSynapseClientService.md#byte-trygetresultstring-taskid)

#### Description

Try to find the result of One task. If there no result, the function return byte[0]

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task Id trying to get result|

#### Returns

`Byte[]` : Returns the result or byte[0] if there no result

---

### [`void CloseSession()`](ArmonikDataSynapseClientService.md#void-closesession)

#### Description

Close Session. This function will disabled in nex Release. The session is automatically  closed after an other creation or after a disconnection or after end of timeout the tasks submitted

#### Parameters


#### Returns

`void`

---

### [`void CancelSession()`](ArmonikDataSynapseClientService.md#void-cancelsession)

#### Description

Cancel the current Session where the SessionId is the one created previously

#### Parameters


#### Returns

`void`

## Class ServiceInvocationContext

---

### [`Boolean IsEquals(String session)`](ServiceInvocationContext.md#boolean-isequalsstring-session)

#### Description

Check if the session is the same as previously created

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`String`||

#### Returns

`Boolean` : Return boolean True if SessionId is null or equals to session parameters

