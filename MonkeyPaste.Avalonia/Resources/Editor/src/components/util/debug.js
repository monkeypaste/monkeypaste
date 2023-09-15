// from https://stackoverflow.com/a/32928812/105028
Object.defineProperty(this, "log", {
    get: function (...args) {
        //if (!window.globals.IsDebug) {
        //    return function () { };
        //}

      //  const d = new Date();
      //  let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;

      //  let table_data = null;
      //  let log_str = '';
      //  let this_log_level = 0;

      //  for (var i = 0; i < args.length; i++) {

      //      if (i == 0 && args[i] == 'table') {
      //          table_data = args.length == 1 ? null : args[i + 1];
      //          continue;
      //      }
      //      if (typeof args[i] === 'string' || args[i] instanceof String) {
      //          if (args.length > 1) {
      //              debugger;
      //          }
      //          // parse literal log level from args
      //          const log_lvl_val = getArgLogLevel(args[i]);
      //          if (log_lvl_val >= 0) {
      //              this_log_level = log_lvl_val;
      //          } else {
      //              log_str += args[i];
      //          }
      //      } else {
      //          log_str += JSON.stringify(args[i]);
		    //}
      //  }
      //  let log_data = null;
      //  if (this_log_level <= globals.MinLogLevel) {
      //      // can't log
      //      return function () { };
      //  }
      //  const mode = globals.IsDebug ? 'DEBUG' : 'RELEASE';
      //  if (table_data) {
      //      return console.table.bind(window.console, table_data);
      //  }
      //  log_str = `[${dateTimeStr.trim()}] [${mode}] ${log_str}`;

      //  return console.log.bind(window.console, log_str);


        const d = new Date();
        let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;

        let log_str = '';

        for (var i = 0; i < args.length; i++) {
            if (i == 0 && args[i] == 'table') {
                const table_data = args.length == 1 ? null : args[i + 1];
                return globals.IsDebug ? console.table.bind(window.console, table_data)
                    : function () { };
            }
            if (typeof args[i] === 'string' || args[i] instanceof String) {
                log_str += args[i];
            } else {
                log_str += JSON.stringify(args[i]);
            }
        }
        log_str = '[' + dateTimeStr.trim() + '] [DEBUG]  ' + log_str;

        return globals.IsDebug ? console.log.bind(window.console, log_str)
            : function () { };
    }
});

function isLogLevelArg(arg) {
    if (arg.startsWith('LogLevel_')) {
        return true;
    }
    return false;
}
function getArgLogLevel(arg) {
    if (isLogLevelArg(arg)) {
        const level = globals[arg];
        return level;
    }
    return -1;
}
function initDebug() {
}

function logtable(table_obj) {
    if (!globals.IsDebug) {
        return;
    }
    console.table(table_obj);
}
