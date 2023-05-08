// #region Globals

const EDITOR_WINDOW_NAME = 'MpEditorWindow';

var IsWindowResizeUpdateEnabled = true;

// #endregion Globals

// #region Life Cycle
function initWindow() {
	window.addEventListener("resize", onWindowResize, true);
	window.addEventListener('scroll', onWindowScroll);
}
// #endregion Life Cycle

// #region Getters

function getEditorSelection_safe() {
	let cmp = WindowMouseLoc;
	let dmp = WindowMouseDownLoc;
	if (!dmp) {
		dmp = cmp;
	}
	if (!cmp) {
		return { index: 0, length: 0 };
	}
	let down_idx = getDocIdxFromPoint(dmp);
	let cur_idx = getDocIdxFromPoint(cmp);

	let safe_range = {};
	if (cur_idx < down_idx) {
		safe_range.index = cur_idx;
		safe_range.length = down_idx - cur_idx;
	} else {
		safe_range.index = down_idx;
		safe_range.length = cur_idx - down_idx;
	}
	return safe_range;
}

function getWindowRect() {
	let wrect = cleanRect();
	wrect.right = window.innerWidth;
	wrect.bottom = window.innerHeight;
	wrect = cleanRect(wrect);
	return wrect;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

function onWindowScroll(e) {
	updateAllElements();
}

function onWindowResize(e) {
	if (!IsWindowResizeUpdateEnabled) {
		return;
	}
	updateAllElements();
	drawOverlay();
}
// #endregion Event Handlers