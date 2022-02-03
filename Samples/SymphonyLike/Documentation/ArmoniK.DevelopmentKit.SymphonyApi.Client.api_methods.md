## Class SessionService

---

### [`String ToString()`](SessionService.md#string-tostring)

#### Description

Returns a string that represents the current object.

#### Parameters


#### Returns

`String` : A string that represents the current object.

---

### [`void OpenSession(SessionId session)`](SessionService.md#void-opensessionsessionid-session)

#### Description

Set connection to an already opened Session

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **session**|`SessionId`|SessionId previously opened|

#### Returns

`void`

---

### [`Task<IEnumerable<Tuple<String, Byte[]>>> GetResults(IEnumerable<String> taskIds)`](SessionService.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)

#### Description

Method to GetResults when the result is returned by a task  The method WaitForCompletion should called before these method

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskIds**|`IEnumerable<String>`|The Task Ids list of the tasks which the result is expected|

#### Returns

`Task<IEnumerable<Tuple<String, Byte[]>>>` : return a dictionary with key taskId and payload

---

### [`IEnumerable<String> SubmitTasks(IEnumerable<Byte[]> payloads)`](SessionService.md#ienumerablestring-submittasksienumerablebyte-payloads)

#### Description

User method to submit task from the client  Need a client Service. In case of ServiceContainer  controlPlaneService can be null until the OpenSession is called

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **payloads**|`IEnumerable<Byte[]>`|The user payload list to execute. General used for subTasking.|

#### Returns

`IEnumerable<String>`Not Documented

---

### [`IEnumerable<String> SubmitSubTasks(String parentTaskId, IEnumerable<Byte[]> payloads)`](SessionService.md#ienumerablestring-submitsubtasksstring-parenttaskid-ienumerablebyte-payloads)

#### Description

The method to submit sub task inside a parent task  Use this method only on server side developpement

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **parentTaskId**|`String`|The task Id of a parent task|
| **payloads**|`IEnumerable<Byte[]>`|A lists of payloads creating a list of subTask|

#### Returns

`IEnumerable<String>` : Return a list of taskId

---

### [`IEnumerable<String> SubmitTasksWithDependencies(IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](SessionService.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)

#### Description

The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **payloadWithDependencies**|`IEnumerable<Tuple<Byte[], IList<String>>>`|A list of Tuple(taskId, Payload) in dependence of those created tasks|

#### Returns

`IEnumerable<String>` : return a list of taskIds of the created tasks

---

### [`String SubmitSubtaskWithDependencies(String parentId, Byte[] payload, IList<String> dependencies)`](SessionService.md#string-submitsubtaskwithdependenciesstring-parentid-byte-payload-iliststring-dependencies)

#### Description

The method to submit One SubTask with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **parentId**|`String`|The parent Task who want to create the SubTask|
| **payload**|`Byte[]`|The payload to submit|
| **dependencies**|`IList<String>`|A list of task Id in dependence of this created SubTask|

#### Returns

`String` : return the taskId of the created SubTask

---

### [`IEnumerable<String> SubmitSubtasksWithDependencies(String parentTaskId, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](SessionService.md#ienumerablestring-submitsubtaskswithdependenciesstring-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)

#### Description

The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **parentTaskId**|`String`|The parent Task who want to create the SubTasks|
| **payloadWithDependencies**|`IEnumerable<Tuple<Byte[], IList<String>>>`|A list of Tuple(taskId, Payload) in dependence of those created Subtasks|

#### Returns

`IEnumerable<String>` : return a list of taskIds of the created Subtasks

---

### [`Byte[] TryGetResult(String taskId)`](SessionService.md#byte-trygetresultstring-taskid)

#### Description

Try to find the result of One task. If there no result, the function return byte[0]

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task Id trying to get result|

#### Returns

`Byte[]` : Returns the result or byte[0] if there no result

---

### [`IEnumerable<Tuple<String, Byte[]>> TryGetResults(IEnumerable<String> taskIds)`](SessionService.md#ienumerabletuplestring-byte-trygetresultsienumerablestring-taskids)

#### Description

Try to get result of a list of taskIds

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskIds**|`IEnumerable<String>`||

#### Returns

`IEnumerable<Tuple<String, Byte[]>>` : Returns an Enumerable pair of

---

### [`void WaitForTaskCompletion(String taskId)`](SessionService.md#void-waitfortaskcompletionstring-taskid)

#### Description

User method to wait for only the parent task from the client

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task id of the task to wait for|

#### Returns

`void`

---

### [`void WaitSubtasksCompletion(String parentTaskId)`](SessionService.md#void-waitsubtaskscompletionstring-parenttaskid)

#### Description

Wait for the taskIds and all its dependencies taskIds

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **parentTaskId**|`String`|The taskIds to|

#### Returns

`void`

---

### [`void WaitForTasksCompletion(IEnumerable<String> taskIds)`](SessionService.md#void-waitfortaskscompletionienumerablestring-taskids)

#### Description

User method to wait for only the parent task from the client

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskIds**|`IEnumerable<String>`|List of taskIds|

#### Returns

`void`

