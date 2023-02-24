// #region Globals

const DOT_NET_MSG_METHOD_NAME = "ReceiveMessage";

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getDotNetModuleName() {
	if (EnvName == AndroidEnv) {
		return "MonkeyPaste.Avalonia.Android";
	}
	onShowDebugger_ntf("unknown env", true);
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isRunningOnHost() {
	return isRunningOnCef() || isRunningOnXam();
}

function isRunningOnCef() {
	return typeof window['notifyLoadComplete'] === 'function'
}

function isRunningOnXam() {
	return typeof window['CSharp'] === 'object';
}

function isWindowFunction(fn) {
	if (typeof window[fn] === 'function') {
		return true;
	}
	return false;
}

function isDesktop() {
	return
		EnvName == WindowsEnv ||
		EnvName == LinuxEnv ||
		EnvName == MacEnv;
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
	log("can't send message. type '" + fn + "' data '" + msg + "'");	
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers