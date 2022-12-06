// #region Globals

var WindowMouseDownLoc = null;
var WindowMouseLoc = null;

// #endregion Globals

// #region Life Cycle

function initMouse() {
	let capture = true;
	window.addEventListener("contextmenu", onWindowContextMenu, capture);

	window.addEventListener("mousedown", onWindowMouseDown, capture);
	window.addEventListener("mousemove", onWindowMouseMove, capture);
	window.addEventListener("mouseup", onWindowMouseUp, capture);

	window.addEventListener('dblclick', onWindowDoubleClick, capture);
	window.addEventListener("click", onWindowClick, capture);
}

// #endregion Life Cycle

// #region Getters

function getClientMousePos(e) {
	if (!e || !e.clientX || !e.clientY) {
		return { x: -1, y: -1 };
	}

	let mp = { x: parseFloat(e.clientX), y: parseFloat(e.clientY) };
	return mp;
}

function getScreenMousePos(e) {
	if (!e || !e.screenX || !e.screenY) {
		return { x: -1, y: -1 };
	}

	let mp = { x: parseFloat(e.screenX), y: parseFloat(e.screenY) };
	return mp;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

function onWindowClick(e) {
	if (rejectTableMouseEvent(e)) {
		return false;
	} 

	let ignore_classes = [
		"edit-template-toolbar",
		"paste-toolbar",
		"context-menu-option",
		"ql-toolbar"
	];
	if (isClassInElementPath(e.target, ignore_classes)) {
		return;
	}
	if (!isClassInElementPath(e.target, TemplateEmbedClass)) {
		// unfocus templates 
		if (TemplateBeforeEdit) {
			if (isShowingColorPaletteMenu()) {
				hideColorPaletteMenu();
			} else if (isShowingCreateTemplateToolbarMenu()) {
				hideCreateTemplateToolbarContextMenu();
			} else {
				hideEditTemplateToolbar();
				clearTemplateFocus();
			}
		}

		if (isShowingPasteToolbar()) {
			//hidePasteToolbar();
			//clearTemplateFocus();
			hideAllTemplateContextMenus();
		}
	}
	return;
}

function onWindowDoubleClick(e) {
	enableSubSelection();
}

function onWindowContextMenu(e) {
	if (rejectTableMouseEvent(e)) {
		e.preventDefault();
		e.stopPropagation();
		return false;
	}
}

function onWindowMouseDown(e) {
	if (rejectTableMouseEvent(e)) {
		e.preventDefault();
		e.stopPropagation();
		return false;
	} 
	WindowMouseDownLoc = { x: e.clientX, y: e.clientY };
	SelectionOnMouseDown = getDocSelection();
}

function onWindowMouseMove(e) {
	WindowMouseLoc = { x: e.clientX, y: e.clientY };
}

function onWindowMouseUp(e) {
	if (rejectTableMouseEvent(e)) {
		return false;
	} 
	WindowMouseDownLoc = null;
	SelectionOnMouseDown = null;
}
// #endregion Event Handlers