// from https://stackoverflow.com/a/32928812/105028
Object.defineProperty(this, "log", {
    get: function (...args) {
        const d = new Date();
        let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;

        let log_str = '';

        for (var i = 0; i < args.length; i++) {
            if (typeof args[i] === 'string' || args[i] instanceof String) {
                log_str += args[i];
            } else {
                log_str += JSON.stringify(args[i]);
		    }
        }
        log_str = '[' + dateTimeStr.trim() + '] [DEBUG]  ' + log_str;

        return IsDebug ? console.log.bind(window.console, log_str) //'[' + dateTimeStr.trim() + ']', '[DEBUG]')
            : function () { };
    }
});

function initDebug() {
    //console.log.bind(log);

    
}

//function log(...args) {
//    let log_str = '';
//    if (!args) {
//        console.log
//	}

//    for (var i = 0; i < args.length; i++) {
//        if (typeof args[i] === 'string' || args[i] instanceof String) {
//            log_str += args[i];
//        } else {
//            log_str += JSON.stringify(args[i]);
//		}
//    }
//    //log_internal(log_str);
//    const d = new Date();
//    let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;
//    log_str = '[' + dateTimeStr.trim() + '] [DEBUG]  ' + log_str;
//    console.log(log_str);
//}

//function log_internal(logStr) {
//    if (!IsDebug) {
//        return;
//    }
//    const d = new Date();
//    let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;
//    logStr = '[' + dateTimeStr.trim() + '] [DEBUG]  ' + logStr;
//    console.log(logStr);
//}