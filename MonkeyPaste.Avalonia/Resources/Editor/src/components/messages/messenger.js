// #region Life Cycle

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isRunningOnWebKit() {
	if (window &&
		window.webkit) {
		return true;
	}
	return false;
}
function isRunningOnWebView2() {
	if (window &&
		window.chrome &&
		window.chrome.webview &&
		typeof window.chrome.webview.postMessage === 'function') {
		return true;
	}
	return false;
}

function isRunningOnCef() {
	return typeof window['notifyLoadComplete'] === 'function';
}

function isRunningOnXam() {
	return typeof window['CSharp'] === 'object';
}
function isRunningOnOutSys() {
	return typeof window['SendMessage'] === 'object';
}

function isRunningInIframe() {
	return window.parent != window;
}

function isRunningOnHost() {
	if (isRunningOnWebKit() ||
		isRunningOnWebView2() ||
		isRunningOnCef() ||
		isRunningOnOutSys() ||
		isRunningOnXam()) {
		return true;
	}
	return false;
}
function isDesktop() {
	return
		globals.EnvName == globals.WindowsEnv ||
		globals.EnvName == globals.LinuxEnv ||
		globals.EnvName == globals.MacEnv;
}

// #endregion State

// #region Actions

function sendMessage(fn, msg) {
	if (isRunningOnWebView2()) {
		// output 'MpQuillPostMessageResponse'
		let resp = {
			msgType: fn,
			msgData: msg,
			handle: globals.ContentHandle
		}; 
		window.chrome.webview.postMessage(JSON.stringify(resp));
		return;
	}
	if (isRunningOnWebKit()) {
		let resp = {
			msgType: fn,
			msgData: msg,
			handle: globals.ContentHandle
		}; 
		window.webkit.messageHandlers.webview.postMessage(JSON.stringify(resp));
		return;
	}
	if (isRunningOnCef()) {
		window[fn](msg, globals.ContentHandle);
		return;
	}
	if (isRunningOnXam()) {
		CSharp.InvokeMethod(fn, msg, globals.ContentHandle);
		return;
	}
	if (isRunningOnOutSys()) {
		let resp = {
			msgType: fn,
			msgData: msg,
			handle: globals.ContentHandle
		}; 
		window['SendMessage'].invoke(fn, msg, globals.ContentHandle);
		return;
	}
	if (isRunningInIframe()) {
		const msg_packet = {
			guid: 'tbd',
			type: fn,
			data: msg
		};
		window.parent.postMessage(msg_packet);
		return;
	}

	if (isRunningOnHost()) {
		log("can't send message. type '" + fn + "' data '" + msg + "'");	
	}
	
}
// #endregion Actions

// #region Event Handlers

window.addEventListener("message", receivedWindowMessage, false);

function receivedWindowMessage(e) {
	if (!isNullOrUndefined(e)) {
		return;
	}
	log('editor recvd window msg: ' + e);

	const wmsg_parts = e.split('(');
	if (wmsg_parts.length < 2) {
		log('bad msg format, not function call');
		return;
	}

	const fn = wmsg_parts[0];
	const msg = wmsg_parts[1].split(')')[0];
	window[fn](msg);
}


// #endregion Event Handlers