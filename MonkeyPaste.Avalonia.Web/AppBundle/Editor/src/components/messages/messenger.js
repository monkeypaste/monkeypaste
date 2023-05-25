// #region Globals

const DOT_NET_MSG_METHOD_NAME = "ReceiveMessage";

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isRunningOnHost() {
	return isRunningOnCef() || isRunningOnXam();
}

function isRunningOnCef() {
	return typeof window['notifyLoadComplete'] === 'function';
}

function isRunningOnXam() {
	return typeof window['CSharp'] === 'object';
}

function isRunningInIframe() {
	return window.parent != window;
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
	if (isRunningOnCef()) {
		window[fn](msg);
		return;
	}
	if (isRunningOnXam()) {
		CSharp.InvokeMethod(fn, msg);
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

	log("can't send message. type '" + fn + "' data '" + msg + "'");	
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