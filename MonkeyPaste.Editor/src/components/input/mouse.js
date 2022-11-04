// #region Globals

var WindowMouseDownLoc = null;
var WindowMouseLoc = null;

// #endregion Globals

// #region Life Cycle

function initMouse() {
	let capture = true;
	window.addEventListener("mousedown", onWindowMouseDown, capture);
	window.addEventListener("mousemove", onWindowMouseMove, capture);
	window.addEventListener("mouseup", onWindowMouseUp, capture);

	window.addEventListener('dblclick', onWindowDoubleClick, capture);
	window.addEventListener("click", onWindowClick, capture);
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

function onWindowClick(e) {
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

		if (IsPastingTemplate) {
			//hidePasteTemplateToolbar();
			//clearTemplateFocus();
			hideAllTemplateContextMenus();
		}
	}
	return;
}

function onWindowDoubleClick(e) {
	enableSubSelection();
}

function onWindowMouseDown(e) {
	if (!isChildOfElement(e.target, getEditorContainerElement()) && !isChildOfElement(e.target, getDragOverlayElement())) {
		// NOTE this is to ignore tracking of mouse events in toolbars for the sake of drag and drop
		log('window mouse down rejected ', e.target, ' is not a child of editor container or drag overlay');
		return;
	}
	WindowMouseDownLoc = { x: e.clientX, y: e.clientY };
	SelectionOnMouseDown = getDocSelection();
}

function onWindowMouseMove(e) {
	WindowMouseLoc = { x: e.clientX, y: e.clientY };
}

function onWindowMouseUp(e) {
	WindowMouseDownLoc = null;
	SelectionOnMouseDown = null;
}
// #endregion Event Handlers