// #region Globals

// #endregion Globals

// #region Life Cycle

function initExceptionHandler() {
    window.onerror = function (msg, url, line, column, error) {
        if (typeof onException_ntf === 'function') {
            onException_ntf(msg, url, parseInt(line), parseInt(column), error);
        }
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