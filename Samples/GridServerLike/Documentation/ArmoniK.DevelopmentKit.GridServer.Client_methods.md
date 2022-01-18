## Class IServiceInvocationHandler

---

### [`void HandleError(ServiceInvocationException e, String taskId)`](IServiceInvocationHandler.md#void-handleerrorserviceinvocationexception-e-string-taskid)

#### Description

The callBack method which has to be implemented to retrieve error or exception

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **e**|`ServiceInvocationException`|The exception sent to the client from the control plane|
| **taskId**|`String`|The task identifier which has invoke the error callBack|

#### Returns

`void`

---

### [`void HandleResponse(Object response, String taskId)`](IServiceInvocationHandler.md#void-handleresponseobject-response-string-taskid)

#### Description

The callBack method which has to be implemented to retrieve response from the server

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **response**|`Object`|The object receive from the server as result the method called by the client|
| **taskId**|`String`|The task identifier which has invoke the response callBack|

#### Returns

`void`

## Class Properties

## Class Service

---

### [`Object LocalExecute(Object service, String methodName, Object[] arguments)`](Service.md#object-localexecuteobject-service-string-methodname-object-arguments)

#### Description

This function execute code locally with the same configuration as Armonik Grid execution  The method needs the Service to execute, the method name to call and arguments of method to pass

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **service**|`Object`|The instance of object containing the method to call|
| **methodName**|`String`|The string name of the method|
| **arguments**|`Object[]`|the array of object to pass as arguments for the method|

#### Returns

`Object` : Returns an object as result of the method call

---

### [`Object Execute(String methodName, Object[] arguments)`](Service.md#object-executestring-methodname-object-arguments)

#### Description

This method is used to execute task and waiting after the result.  the method will return the result of the execution until the grid returns the task result

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **methodName**|`String`|The string name of the method|
| **arguments**|`Object[]`|the array of object to pass as arguments for the method|

#### Returns

`Object` : Returns an object as result of the method call

---

### [`void Submit(String methodName, Object[] arguments, IServiceInvocationHandler handler)`](Service.md#void-submitstring-methodname-object-arguments-iserviceinvocationhandler-handler)

#### Description

The method submit will execute task asynchronously on the server

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **methodName**|`String`|The name of the method inside the service|
| **arguments**|`Object[]`|A list of object that can be passed in parameters of the function|
| **handler**|`IServiceInvocationHandler`|The handler callBack implemented as IServiceInvocationHandler to get response or result or error|

#### Returns

`void`

---

### [`void Dispose()`](Service.md#void-dispose)

#### Description

Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.

#### Parameters


#### Returns

`void`

---

### [`void Destroy()`](Service.md#void-destroy)

#### Description

The method to destroy the service and close the session

#### Parameters


#### Returns

`void`

---

### [`Boolean IsDestroyed()`](Service.md#boolean-isdestroyed)

#### Description

Check if this service has been destroyed before that call

#### Parameters


#### Returns

`Boolean` : Returns true if the service was destroyed previously

## Class ServiceFactory

---

### [`Service CreateService(String serviceType, Properties props)`](ServiceFactory.md#service-createservicestring-servicetype-properties-props)

#### Description

The methode to create new Service

#### Parameters

| Name | Type | Description |
| --- | --- | --- |
| **serviceType**|`String`|Future value no usage for now.
            This is the Service type reflection for method|
| **props**|`Properties`|Properties for the service containing IConfiguration and TaskOptions|

#### Returns

`Service` : returns the new instantiated service

## Class ServiceInvocationException

