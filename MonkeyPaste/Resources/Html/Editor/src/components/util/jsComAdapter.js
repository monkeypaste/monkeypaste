var IsBoundToHost = false;

async function bindJsComAdapter() {
	if (EnvName == 'web' || typeof CefSharp == 'undefined') {
		IsBoundToHost = false;
		return;
	}
	await CefSharp.BindObjectAsync("jsComAdapter");
	IsBoundToHost = true;
	log('Bound to host');

	//The default is to camel case method names (the first letter of the method name is changed to lowercase)
	jsComAdapter.test1().then(function (actualResult) {
		log('test1:');
		log(actualResult);
	});
	jsComAdapter.test2('string from js').then(function (actualResult) {
		log('test2:');
		log(actualResult);
	});
}

function deleteJsComAdapter() {
	if (EnvName == 'web' || typeof CefSharp == 'undefined') {
		IsBoundToHost = false;
		return;
	}
	IsBoundToHost = !CefSharp.DeleteBoundObject("jsComAdapter");

	if (IsBoundToHost) {
		log('error, could not unbind to host');
	} else {
		log('successfully delete host binding');
    }
}

function setComOutput(output) {
	document.getElementById('comOutputTextArea').innerHtml = output;
}

function clearComOutput() {
	document.getElementById('comOutputTextArea').innerText = '';
}