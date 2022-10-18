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
	if (
		e.path.find(
			(x) => x.classList && x.classList.contains("edit-template-toolbar")) != null ||
		e.path.find(
			(x) => x.classList && x.classList.contains("paste-template-toolbar")
		) != null ||
		e.path.find(
			(x) => x.classList && x.classList.contains("context-menu-option")
		) != null ||
		e.path.find((x) => x.classList && x.classList.contains("ql-toolbar")) !=
		null
	) {
		//ignore clicks within template toolbars
		return;
	}
	if (
		e.path.find(
			(x) => x.classList && x.classList.contains(TemplateEmbedClass)
		) == null
	) {
		// unfocus templates 
		//
		if (TemplateBeforeEdit) {
			hideEditTemplateToolbar();
			clearTemplateFocus();
			hideAllTemplateContextMenus();
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
	if (!isChildOfElement(e.target, getEditorContainerElement())) {
		log('window mouse down rejected ', e.target, ' is not a child of editor container');
		return;
	}
	WindowMouseDownLoc = { x: e.clientX, y: e.clientY };
	SelectionOnMouseDown = getEditorSelection();
}

function onWindowMouseMove(e) {
	WindowMouseLoc = { x: e.clientX, y: e.clientY };
}

function onWindowMouseUp(e) {
	WindowMouseDownLoc = null;
	SelectionOnMouseDown = null;
}
// #endregion Event Handlers