// from https://stackoverflow.com/a/32928812/105028
Object.defineProperty(this, "log", {
    get: function () {
        const d = new Date();
        let dateTimeStr = d.toLocaleTimeString().replace('AM', '').replace('PM', '').trim() + `.${d.getMilliseconds()}`;
        return IsDebug ? console.log.bind(window.console, '[' + dateTimeStr.trim() + ']', '[DEBUG]')
            : function () { };
    }
});