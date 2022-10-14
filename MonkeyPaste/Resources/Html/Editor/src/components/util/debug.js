// from https://stackoverflow.com/a/32928812/105028
//Object.defineProperty(this, "log", {
//    get: function () {
//        const d = new Date();
//        let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;
//        return IsDebug ? console.log.bind(window.console, '[' + dateTimeStr.trim() + ']', '[DEBUG]')
//            : function () { };
//    }
//});

function log(...args) {
    let log_str = '';
    if (!args) {
        console.log
	}

    for (var i = 0; i < args.length; i++) {
        if (typeof args[i] === 'string' || args[i] instanceof String) {
            log_str += args[i];
        } else {
            log_str += JSON.stringify(args[i]);
		}
    }
    log_internal(log_str);
}

function log_internal(logStr) {
    if (!IsDebug) {
        return;
    }
    const d = new Date();
    let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;
    logStr = '[' + dateTimeStr.trim() + '] [DEBUG]  ' + logStr;
    console.log(logStr);
}