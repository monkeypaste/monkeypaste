


// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isPoint(p) {
	if (isNullOrUndefined(p)) {
		return false;
	}
	return p.x !== undefined && p.y !== undefined;
}
// #endregion State

// #region Actions

function editorToScreenPoint(ep) {
	let editor_rect = getEditorContainerRect();
	let sp = { x: ep.x + editor_rect.left, y: ep.y + editor_rect.top };

	return sp;
}

function screenToEditorPoint(sp) {
	let editor_rect = getEditorContainerRect();
	return { x: sp.x - editor_rect.left, y: sp.y - editor_rect.top };
}

function addPoints(p1, p2) {
	return { x: p1.x + p2.x, y: p1.y + p2.y };
}
function subtractPoints(p1, p2) {
	return { x: p1.x - p2.x, y: p1.y - p2.y };
}
function multiplyPoints(p1, p2) {
	return { x: p1.x * p2.x, y: p1.y * p2.y };
}
function dividePoints(p1, p2) {
	return { x: p1.x / p2.x, y: p1.y / p2.y };
}

function addPointScalar(p1, val) {
	return { x: p1.x + val, y: p1.y + val };
}
function subtractPointScalar(p1, val) {
	return { x: p1.x - val, y: p1.y - val };
}
function multiplyPointScalar(p1, val) {
	return { x: p1.x * val, y: p1.y * val };
}
function dividePointScalar(p1, val) {
	return { x: p1.x / val, y: p1.y / val };
}

function dist(p1, p2) {
	return Math.sqrt(Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2));
}

function distSqr(p1, p2) {
	return Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2);
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers

