function notifyHost(binding_func, msgObj) {
	let msgObjBase64Str = toBase64FromJsonObj(msgObj);
	binding_func(msgObjBase64Str);
}

function reqFromHost(binding_func, msgObj) {
	let msgObjBase64Str = toBase64FromJsonObj(msgObj);
	return binding_func(msgObjBase64Str);
}

function receiveHostMsg(msgObjBase64Str) {
	let msgObj = toJsonObjFromBase64Str(msgObjBase64Str);
	return msgObj;
}