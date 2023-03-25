// from https://stackoverflow.com/a/32928812/105028
Object.defineProperty(this, "log", {
    get: function (...args) {
        const d = new Date();
        let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;

        let log_str = '';

        for (var i = 0; i < args.length; i++) {
            if (i == 0 && args[i] == 'table') {
                const table_data = args.length == 1 ? null : args[i + 1]; 
                return IsDebug ? console.table.bind(window.console, table_data)
                    : function () { };
            }
            if (typeof args[i] === 'string' || args[i] instanceof String) {
                log_str += args[i];
            } else {
                log_str += JSON.stringify(args[i]);
		    }
        }
        log_str = '[' + dateTimeStr.trim() + '] [DEBUG]  ' + log_str;

        return IsDebug ? console.log.bind(window.console, log_str) 
            : function () { };
    }
});

function initDebug() {
}

function logtable(table_obj) {
    if (!IsDebug) {
        return;
    }
    console.table(table_obj);
}
