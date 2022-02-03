## Class ServiceContainerBase

---

### [`void OnCreateService(ServiceContext serviceContext)`](ServiceContainerBase.md#void-oncreateserviceservicecontext-servicecontext)

#### Description

The middleware triggers the invocation of this handler just after a Service Instance is started.  The application developer must put any service initialization into this handler.  Default implementation does nothing.

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **serviceContext**|`ServiceContext`|Holds all information on the state of the service at the start of the execution.|

#### Returns

`void`

---

### [`void OnSessionEnter(SessionContext sessionContext)`](ServiceContainerBase.md#void-onsessionentersessioncontext-sessioncontext)

#### Description

This handler is executed once after the callback OnCreateService and before the OnInvoke

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **sessionContext**|`SessionContext`|Holds all information on the state of the session at the start of the execution.|

#### Returns

`void`

---

### [`Byte[] OnInvoke(SessionContext sessionContext, TaskContext taskContext)`](ServiceContainerBase.md#byte-oninvokesessioncontext-sessioncontext-taskcontext-taskcontext)

#### Description

The middleware triggers the invocation of this handler every time a task input is  sent to the service to be processed.  The actual service logic should be implemented in this method. This is the only  method that is mandatory for the application developer to implement.

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **sessionContext**|`SessionContext`|Holds all information on the state of the session at the start of the execution such as session ID.|
| **taskContext**|`TaskContext`|Holds all information on the state of the task such as the task ID and the payload.|

#### Returns

`Byte[]`Not Documented

---

### [`void OnSessionLeave(SessionContext sessionContext)`](ServiceContainerBase.md#void-onsessionleavesessioncontext-sessioncontext)

#### Description

The middleware triggers the invocation of this handler to unbind the Service Instance from its owning Session.  This handler should do any cleanup for any resources that were used in the onSessionEnter() method.

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **sessionContext**|`SessionContext`|Holds all information on the state of the session at the start of the execution such as session ID.|

#### Returns

`void`

---

### [`void OnDestroyService(ServiceContext serviceContext)`](ServiceContainerBase.md#void-ondestroyserviceservicecontext-servicecontext)

#### Description

The middleware triggers the invocation of this handler just before a Service Instance is destroyed.  This handler should do any cleanup for any resources that were used in the onCreateService() method.

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **serviceContext**|`ServiceContext`|Holds all information on the state of the service at the start of the execution.|

#### Returns

`void`

---

### [`IEnumerable<String> SubmitTasks(IEnumerable<Byte[]> payloads)`](ServiceContainerBase.md#ienumerablestring-submittasksienumerablebyte-payloads)

#### Description

User method to submit task from the service

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **payloads**|`IEnumerable<Byte[]>`|The user payload list to execute. Generally used for subTasking.|

#### Returns

`IEnumerable<String>`Not Documented

---

### [`IEnumerable<String> SubmitTasksWithDependencies(IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ServiceContainerBase.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)

#### Description

The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **payloadWithDependencies**|`IEnumerable<Tuple<Byte[], IList<String>>>`|A list of Tuple(taskId, Payload) in dependence of those created tasks|

#### Returns

`IEnumerable<String>` : return a list of taskIds of the created tasks

---

### [`IEnumerable<String> SubmitSubtasksWithDependencies(String parentId, IEnumerable<Tuple<Byte[], IList<String>>> payloadWithDependencies)`](ServiceContainerBase.md#ienumerablestring-submitsubtaskswithdependenciesstring-parentid-ienumerabletuplebyte-iliststring-payloadwithdependencies)

#### Description

The method to submit several tasks with dependencies tasks. This task will wait for  to start until all dependencies are completed successfully

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **parentId**|`String`||
| **payloadWithDependencies**|`IEnumerable<Tuple<Byte[], IList<String>>>`|A list of Tuple(taskId, Payload) in dependence of those created Subtasks|

#### Returns

`IEnumerable<String>` : return a list of taskIds of the created subtasks

---

### [`void WaitForTaskCompletion(String taskId)`](ServiceContainerBase.md#void-waitfortaskcompletionstring-taskid)

#### Description

User method to wait for only the parent task from the client

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task id of the task to wait for|

#### Returns

`void`

---

### [`void WaitForTasksCompletion(IEnumerable<String> taskIds)`](ServiceContainerBase.md#void-waitfortaskscompletionienumerablestring-taskids)

#### Description

No Description
#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskIds**|`IEnumerable<String>`|List of tasks to wait for|

#### Returns

`void`

---

### [`void WaitForSubTasksCompletion(String taskId)`](ServiceContainerBase.md#void-waitforsubtaskscompletionstring-taskid)

#### Description

User method to wait for SubTasks from the client

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task id of the Subtask|

#### Returns

`void`

---

### [`Byte[] GetResult(String taskId)`](ServiceContainerBase.md#byte-getresultstring-taskid)

#### Description

Get Result from compute reply

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskId**|`String`|The task Id to get the result|

#### Returns

`Byte[]` : return the customer payload

---

### [`void Configure(IConfiguration configuration, IDictionary<String, String> clientOptions)`](ServiceContainerBase.md#void-configureiconfiguration-configuration-idictionarystring-string-clientoptions)

#### Description

The configure method is an internal call to prepare the ServiceContainer.  Its holds several configuration coming from the Client call

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **configuration**|`IConfiguration`|The appSettings.json configuration prepared during the deployment|
| **clientOptions**|`IDictionary<String, String>`|All data coming from Client within TaskOptions.Options|

#### Returns

`void`

---

### [`void ConfigureSession(SessionId sessionId, IDictionary<String, String> requestTaskOptions)`](ServiceContainerBase.md#void-configuresessionsessionid-sessionid-idictionarystring-string-requesttaskoptions)

#### Description

Prepare Session and create SessionService with the specific session

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **sessionId**|`SessionId`||
| **requestTaskOptions**|`IDictionary<String, String>`||

#### Returns

`void`

