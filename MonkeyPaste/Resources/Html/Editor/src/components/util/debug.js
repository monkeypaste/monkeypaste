// from https://stackoverflow.com/a/32928812/105028
Object.defineProperty(this, "log", {
    get: function () {
        return isDebug ? console.log.bind(window.console, '[' + Date.now() + ']', '[DEBUG]')
            : function () { };
    }
});