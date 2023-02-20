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
function test() {
	DotNet.invokeMethodAsync('{ASSEMBLY NAME}', '{.NET METHOD ID}', { ARGUMENTS });
}

function sendMessage(fn, msg) {
	if (isWindowFunction(fn)) {
		window[fn](msg);
		return;
	}
	if (isWindowFunction('DotNet.invokeMethod')) {
		DotNet.invokeMethod(getDotNetModuleName(), DOT_NET_MSG_METHOD_NAME, fn, msg);
		return;
	}
	onShowDebugger_ntf("can't send message");
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers