// #region Globals

var LastTextChangedDelta = null;

// #endregion Globals

// #region Life Cycle

function initHistory() {
}

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function clearLastDelta() {
	log('Delta log cleared. It was: ');
	log(LastTextChangedDelta);
	LastTextChangedDelta = null;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers