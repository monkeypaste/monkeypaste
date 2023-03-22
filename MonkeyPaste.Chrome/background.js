// Establish a messaging connection with the native application
var port = chrome.runtime.connectNative('example');

// Send a message to the native application
port.postMessage({ text: "Hello from the extension!" });

// Listen for messages from the native application
port.onMessage.addListener(function (message) {
	console.log("Message received from native application:", message);
});
