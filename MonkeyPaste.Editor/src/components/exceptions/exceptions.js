// #region Globals

// #endregion Globals

// #region Life Cycle

function initExceptionHandler() {
    window.onerror = function (msg, url, line) {
        alert("Message : " + msg);
        alert("url : " + url);
        alert("Line number : " + line);

        onException_ntf(msg, url, parseInt(line));
    }
}

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers