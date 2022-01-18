# References

## [ArmoniK.DevelopmentKit.SymphonyApi](ArmoniK.DevelopmentKit.SymphonyApi.md)

### [Class `ServiceContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#servicecontext)

---

### [Class `SessionContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#sessioncontext)

---

### [Class `TaskContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#taskcontext)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.api](ArmoniK.DevelopmentKit.SymphonyApi.api.md)

### [Class `ServiceContainerBase`](ArmoniK.DevelopmentKit.SymphonyApi.api.md#servicecontainerbase)

### Methods:

- [OnCreateService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-oncreateserviceservicecontext-servicecontext)
- [OnSessionEnter](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionentersessioncontext-sessioncontext)
- [OnInvoke](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-oninvokesessioncontext-sessioncontext-taskcontext-taskcontext)
- [OnSessionLeave](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionleavesessioncontext-sessioncontext)
- [OnDestroyService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-ondestroyserviceservicecontext-servicecontext)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)
- [WaitForCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforcompletionstring-taskid)
- [WaitForSubTasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforsubtaskscompletionstring-taskid)
- [GetResult](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-getresultstring-taskid)
- [Configure](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-configureiconfiguration-configuration-idictionarystring-string-clientoptions)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.Client](ArmoniK.DevelopmentKit.SymphonyApi.Client.md)

### [Class `ArmonikSymphonyClient`](ArmoniK.DevelopmentKit.SymphonyApi.Client.md#armoniksymphonyclient)

### Methods:

- [CreateSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-createsessiontaskoptions-taskoptions--null)
- [OpenSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-opensessionsessionid-session)
- [WaitSubtasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitsubtaskscompletionstring-parenttaskid)
- [WaitCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitcompletionstring-taskid)
- [GetResults](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitSubTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtasksstring-session-string-parenttaskid-ienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [SubmitSubtaskWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-submitsubtaskwithdependenciesstring-session-string-parentid-byte-payload-iliststring-dependencies)
- [SubmitSubtasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtaskswithdependenciesstring-session-string-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [TryGetResult](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#byte-trygetresultstring-taskid)

---

## [ArmoniK.DevelopmentKit.WorkerApi.Common](ArmoniK.DevelopmentKit.WorkerApi.Common.md)

### [Class `AppsOptions`](ArmoniK.DevelopmentKit.WorkerApi.Common.md#appsoptions)

---


## [ArmoniK.DevelopmentKit.SymphonyApi](ArmoniK.DevelopmentKit.SymphonyApi.md)

### [Class `ServiceContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#servicecontext)

---

### [Class `SessionContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#sessioncontext)

---

### [Class `TaskContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#taskcontext)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.api](ArmoniK.DevelopmentKit.SymphonyApi.api.md)

### [Class `ServiceContainerBase`](ArmoniK.DevelopmentKit.SymphonyApi.api.md#servicecontainerbase)

### Methods:

- [OnCreateService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-oncreateserviceservicecontext-servicecontext)
- [OnSessionEnter](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionentersessioncontext-sessioncontext)
- [OnInvoke](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-oninvokesessioncontext-sessioncontext-taskcontext-taskcontext)
- [OnSessionLeave](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionleavesessioncontext-sessioncontext)
- [OnDestroyService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-ondestroyserviceservicecontext-servicecontext)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)
- [WaitForCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforcompletionstring-taskid)
- [WaitForSubTasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforsubtaskscompletionstring-taskid)
- [GetResult](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-getresultstring-taskid)
- [Configure](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-configureiconfiguration-configuration-idictionarystring-string-clientoptions)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.Client](ArmoniK.DevelopmentKit.SymphonyApi.Client.md)

### [Class `ArmonikSymphonyClient`](ArmoniK.DevelopmentKit.SymphonyApi.Client.md#armoniksymphonyclient)

### Methods:

- [CreateSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-createsessiontaskoptions-taskoptions--null)
- [OpenSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-opensessionsessionid-session)
- [WaitSubtasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitsubtaskscompletionstring-parenttaskid)
- [WaitCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitcompletionstring-taskid)
- [GetResults](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitSubTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtasksstring-session-string-parenttaskid-ienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [SubmitSubtaskWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-submitsubtaskwithdependenciesstring-session-string-parentid-byte-payload-iliststring-dependencies)
- [SubmitSubtasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtaskswithdependenciesstring-session-string-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [TryGetResult](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#byte-trygetresultstring-taskid)

---

## [ArmoniK.DevelopmentKit.WorkerApi.Common](ArmoniK.DevelopmentKit.WorkerApi.Common.md)

### [Class `AppsOptions`](ArmoniK.DevelopmentKit.WorkerApi.Common.md#appsoptions)

---


## [ArmoniK.DevelopmentKit.SymphonyApi](ArmoniK.DevelopmentKit.SymphonyApi.md)

### [Class `ServiceContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#servicecontext)

---

### [Class `SessionContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#sessioncontext)

---

### [Class `TaskContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#taskcontext)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.api](ArmoniK.DevelopmentKit.SymphonyApi.api.md)

### [Class `ServiceContainerBase`](ArmoniK.DevelopmentKit.SymphonyApi.api.md#servicecontainerbase)

### Methods:

- [OnCreateService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-oncreateserviceservicecontext-servicecontext)
- [OnSessionEnter](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionentersessioncontext-sessioncontext)
- [OnInvoke](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-oninvokesessioncontext-sessioncontext-taskcontext-taskcontext)
- [OnSessionLeave](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionleavesessioncontext-sessioncontext)
- [OnDestroyService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-ondestroyserviceservicecontext-servicecontext)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)
- [WaitForCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforcompletionstring-taskid)
- [WaitForSubTasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforsubtaskscompletionstring-taskid)
- [GetResult](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-getresultstring-taskid)
- [Configure](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-configureiconfiguration-configuration-idictionarystring-string-clientoptions)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.Client](ArmoniK.DevelopmentKit.SymphonyApi.Client.md)

### [Class `ArmonikSymphonyClient`](ArmoniK.DevelopmentKit.SymphonyApi.Client.md#armoniksymphonyclient)

### Methods:

- [CreateSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-createsessiontaskoptions-taskoptions--null)
- [OpenSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-opensessionsessionid-session)
- [WaitSubtasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitsubtaskscompletionstring-parenttaskid)
- [WaitCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitcompletionstring-taskid)
- [GetResults](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitSubTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtasksstring-session-string-parenttaskid-ienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [SubmitSubtaskWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-submitsubtaskwithdependenciesstring-session-string-parentid-byte-payload-iliststring-dependencies)
- [SubmitSubtasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtaskswithdependenciesstring-session-string-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [TryGetResult](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#byte-trygetresultstring-taskid)

---

## [ArmoniK.DevelopmentKit.WorkerApi.Common](ArmoniK.DevelopmentKit.WorkerApi.Common.md)

### [Class `AppsOptions`](ArmoniK.DevelopmentKit.WorkerApi.Common.md#appsoptions)

---


## [ArmoniK.DevelopmentKit.SymphonyApi](ArmoniK.DevelopmentKit.SymphonyApi.md)

### [Class `ServiceContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#servicecontext)

---

### [Class `SessionContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#sessioncontext)

---

### [Class `TaskContext`](ArmoniK.DevelopmentKit.SymphonyApi.md#taskcontext)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.api](ArmoniK.DevelopmentKit.SymphonyApi.api.md)

### [Class `ServiceContainerBase`](ArmoniK.DevelopmentKit.SymphonyApi.api.md#servicecontainerbase)

### Methods:

- [OnCreateService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-oncreateserviceservicecontext-servicecontext)
- [OnSessionEnter](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionentersessioncontext-sessioncontext)
- [OnInvoke](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-oninvokesessioncontext-sessioncontext-taskcontext-taskcontext)
- [OnSessionLeave](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-onsessionleavesessioncontext-sessioncontext)
- [OnDestroyService](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-ondestroyserviceservicecontext-servicecontext)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#ienumerablestring-submittaskswithdependenciesienumerabletuplebyte-iliststring-payloadwithdependencies)
- [WaitForCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforcompletionstring-taskid)
- [WaitForSubTasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-waitforsubtaskscompletionstring-taskid)
- [GetResult](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#byte-getresultstring-taskid)
- [Configure](ArmoniK.DevelopmentKit.SymphonyApi.api_methods.md#void-configureiconfiguration-configuration-idictionarystring-string-clientoptions)

---

## [ArmoniK.DevelopmentKit.SymphonyApi.Client](ArmoniK.DevelopmentKit.SymphonyApi.Client.md)

### [Class `ArmonikSymphonyClient`](ArmoniK.DevelopmentKit.SymphonyApi.Client.md#armoniksymphonyclient)

### Methods:

- [CreateSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-createsessiontaskoptions-taskoptions--null)
- [OpenSession](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-opensessionsessionid-session)
- [WaitSubtasksCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitsubtaskscompletionstring-parenttaskid)
- [WaitCompletion](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#void-waitcompletionstring-taskid)
- [GetResults](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#taskienumerabletuplestring-byte-getresultsienumerablestring-taskids)
- [SubmitTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittasksienumerablebyte-payloads)
- [SubmitSubTasks](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtasksstring-session-string-parenttaskid-ienumerablebyte-payloads)
- [SubmitTasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submittaskswithdependenciesstring-session-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [SubmitSubtaskWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#string-submitsubtaskwithdependenciesstring-session-string-parentid-byte-payload-iliststring-dependencies)
- [SubmitSubtasksWithDependencies](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#ienumerablestring-submitsubtaskswithdependenciesstring-session-string-parenttaskid-ienumerabletuplebyte-iliststring-payloadwithdependencies)
- [TryGetResult](ArmoniK.DevelopmentKit.SymphonyApi.Client_methods.md#byte-trygetresultstring-taskid)

---

## [ArmoniK.DevelopmentKit.WorkerApi.Common](ArmoniK.DevelopmentKit.WorkerApi.Common.md)

### [Class `AppsOptions`](ArmoniK.DevelopmentKit.WorkerApi.Common.md#appsoptions)

---

