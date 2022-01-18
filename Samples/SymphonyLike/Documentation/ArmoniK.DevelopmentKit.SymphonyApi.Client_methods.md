## Class ArmonikSymphonyClient

---

### [`String CreateSession(TaskOptions taskOptions = null)`](ArmonikSymphonyClient.md#string-createsessiontaskoptions-taskoptions--null)

#### Description

Create the session to submit task

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskOptions**|`TaskOptions`|Optional parameter to set TaskOptions during the Session creation|

#### Returns

`String`Not Documented

---

### [`void OpenSession(SessionId session)`](ArmonikSymphonyClient.md#void-opensessionsessionid-session)

#### Description

Set connection to an already opened Session

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`SessionId`|SessionId previously opened|

#### Returns

`void`

---

### [`void WaitSubtasksCompletion(String parentTaskId)`](ArmonikSymphonyClient.md#void-waitsubtaskscompletionstring-parenttaskid)

#### Description

Wait for the taskIds and all its dependencies taskIds

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **parentTaskId**|`String`|The taskIds to|

#### Returns

`void`

---

### [`void WaitCompletion(String taskId)`](ArmonikSymphonyClient.md#void-waitcompletionstring-taskid)

#### Description

User method to wait for only the parent task from the client

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task id of the task to wait for|

#### Returns

`void`

---

### [`Task<IEnumerable<Tuple<String, Byte[]>>> GetResults(IEnumerable<String> taskIds)`](ArmonikSymphonyClient.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)

#### Description

Method to GetResults when the result is returned by a task  The method WaitForCompletion should called before these method

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskIds**|`IEnumerable<String>`|The Task Ids list of the tasks which the result is expected|

#### Returns

`Task<IEnumerable<Tuple<String, Byte[]>>>` : return a dictionary with key taskId and payload

---

### [`IEnumerable<String> SubmitTasks(IEnumerable<Byte[]> payloads)`](ArmonikSymphonyClient.md#ienumerablestring-submittasksienumerablebyte-payloads)

#### Description

User method to submit task from the client  Need a client Service. In case of ServiceContainer  controlPlaneService can be null until the OpenSession is called

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **payloads**|`IEnumerable<Byte[]>`|The user payload list to execute. General used for subTasking.|

#### Returns

`IEnumerable<String>`Not Documented

---

### [`IEnumerable<String> SubmitSubTasks(String session, String parentTaskId, IEnumerable<Byte[]> payloads)`](ArmonikSymphonyClient.md#ienumerablestring-submitsubtasksstring-session-string-parenttaskid-ienumerablebyte-payloads)

#### Description

The method to submit sub task inside a parent task  Use this method only on server side developpement

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`String`|The session Id to attached the task|
| **parentTaskId**|`String`|The task Id of a parent task|
| **payloads**|`IEnumerable<Byte[]>`|A lists of payloads creating a list of subTask|

#### Returns

`IEnumerable<String>` : Return a list of taskId

---

### [`IEnumerable<String> SubmitTasksWithDependencies(String session, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmonikSymphonyClient.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)

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

### [`String SubmitSubtaskWithDependencies(String session, String parentId, Byte[] payload, IList<String> dependencies)`](ArmonikSymphonyClient.md#string-submitsubtaskwithdependenciesstring-session-string-parentid-byte-payload-iliststring-dependencies)

#### Description

The method to submit One SubTask with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`String`|The session Id where the task will be attached|
| **parentId**|`String`|The parent Task who want to create the SubTask|
| **payload**|`Byte[]`|The payload to submit|
| **dependencies**|`IList<String>`|A list of task Id in dependence of this created SubTask|

#### Returns

`String` : return the taskId of the created SubTask

---

### [`IEnumerable<String> SubmitSubtasksWithDependencies(String session, String parentTaskId, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ArmonikSymphonyClient.md#ienumerablestring-submitsubtaskswithdependenciesstring-session-string-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)

#### Description

The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`String`|The session Id where the Subtask will be attached|
| **parentTaskId**|`String`|The parent Task who want to create the SubTasks|
| **payloadWithDependencies**|`IEnumerable<Tuple<Byte[], IList<String>>>`|A list of Tuple(taskId, Payload) in dependence of those created Subtasks|

#### Returns

`IEnumerable<String>` : return a list of taskIds of the created Subtasks

---

### [`Byte[] TryGetResult(String taskId)`](ArmonikSymphonyClient.md#byte-trygetresultstring-taskid)

#### Description

Try to find the result of One task. If there no result, the function return byte[0]

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task Id trying to get result|

#### Returns

`Byte[]` : Returns the result or byte[0] if there no result

