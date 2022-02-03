## Class ArmonikSymphonyClient

---

### [`SessionService CreateSession(TaskOptions taskOptions = null)`](ArmonikSymphonyClient.md#sessionservice-createsessiontaskoptions-taskoptions--null)

#### Description

Create the session to submit task

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **taskOptions**|`TaskOptions`|Optional parameter to set TaskOptions during the Session creation|

#### Returns

`SessionService` : Returns the SessionService to submit, wait or get result

---

### [`SessionService OpenSession(SessionId sessionId, IDictionary<String, String> clientOptions)`](ArmonikSymphonyClient.md#sessionservice-opensessionsessionid-sessionid-idictionarystring-string-clientoptions)

#### Description

Open the session already created to submit task

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **sessionId**|`SessionId`|The sessionId string which will opened|
| **clientOptions**|`IDictionary<String, String>`|the customer taskOptions.Options send to the server by the client|

#### Returns

`SessionService` : Returns the SessionService to submit, wait or get result

