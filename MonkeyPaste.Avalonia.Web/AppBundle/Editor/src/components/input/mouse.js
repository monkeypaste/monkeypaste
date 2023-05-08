// #region Globals

var WindowMouseDownLoc = null;
var WindowMouseLoc = null;

var WasSupressRightMouseDownSentToHost = false;
var WasInternalContextMenuAbleToShow = false;
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
		if (e && e.screenX && e.screenY) {
			//let client_rect = getEditorContainerElement().getBoundingClientRect();
			//let client_ox = client_rect.left;
			//let client_oy = client_rect.top;

			//let window_x = window.screenX + e.screenX;
			//let window_y = window.screenY + e.screenY;

			//e.clientX = window_x + client_ox;
			//e.clientY = window_y + client_oy;
			e.clientX = e.screenX;
			e.clientY = e.screenY;
		} else {
			return { x: -1, y: -1 };
		}		
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

function updateWindowMouseState(e) {
	// NOTE this is called from both mouse and dnd events so state is 'always' accurate
	if (!e || e.buttons === undefined) {
		return;
	}
	WindowMouseLoc = getClientMousePos(e);
	if (e.buttons === 1) {
		if (WindowMouseDownLoc == null) {
			WindowMouseDownLoc = WindowMouseLoc;
			if (isDragging()) {
				// drag end was not triggered, so reset here
				// i think this only happen when resuming from breakpoint in dnd

				//log('lingering drag elm caught in mouse down, manually calling dragEnd...')
				//onDragEnd('forced from window mousedown');
			}
		}
	} else if (e.dataTransfer === undefined) {
		// 
		WindowMouseDownLoc = null;
	}
}

// #endregion Actions

// #region Event Handlers

function onWindowClick(e) {
	if (rejectTableMouseEvent(e,'click')) {
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

	onAnnotationWindowPointerClick(e);
	return;
}

function onWindowDoubleClick(e) {
	if (isSubSelectionEnabled()) {
		return;
	}
	enableSubSelection();
}

function onWindowContextMenu(e) {
	if (rejectTableMouseEvent(e)) {
		e.preventDefault();
		e.stopPropagation();
		return false;
	} else {
		// only relevant for table ops menu display
		// to clamp height to window height to show its scrollbars
		updateTableOpsMenuSizeAndPosition();
	}
}

function onWindowMouseDown(e) {
	if (isSubSelectionEnabled()) {
		ensureWindowFocus();
	}

	if (WasSupressRightMouseDownSentToHost) {
		// sanity check to cleanup any uncaptured mouse ups during a down supress
		// (like if right click down on cell then up outside of editor window)
		WasSupressRightMouseDownSentToHost = false;
		onInternalContextMenuIsVisibleChanged_ntf(false);
	}

	if (rejectTableMouseEvent(e)) {
		e.preventDefault();
		e.stopPropagation();
		log('mouse down rejected by table logic')
		return false;
	}
	if (isContextMenuEventGoingToShowTableMenu(e)) {
		// notify host to not show 
		//onInternalContextMenuIsVisibleChanged_ntf(true);
		WasSupressRightMouseDownSentToHost = true;
	}

	//WindowMouseDownLoc = { x: e.clientX, y: e.clientY };
	updateWindowMouseState(e);
	SelectionOnMouseDown = getDocSelection();
}

function onWindowMouseMove(e) {
	updateWindowMouseState(e);
	if (hasAnnotations()) {
		onAnnotationWindowPointerMove(e);
	}	
}

function onWindowMouseUp(e) {
	if (WasSupressRightMouseDownSentToHost) {
		delay(300)
			.then(() => {
				onInternalContextMenuIsVisibleChanged_ntf(false);
			});		
	}
	if (rejectTableMouseEvent(e)) {
		e.preventDefault();
		e.stopPropagation();
		log('mouse up rejected by table logic')
		return false;
	}

	if (canContextMenuEventShowTableOpsMenu()) {
		onInternalContextMenuIsVisibleChanged_ntf(true);
		WasInternalContextMenuAbleToShow = true;
	} else if (WasInternalContextMenuAbleToShow) {
		onInternalContextMenuIsVisibleChanged_ntf(false);
		WasInternalContextMenuAbleToShow = false;
	}

	updateWindowMouseState(e);
	SelectionOnMouseDown = null;
	DragDomRange = null;
}
// #endregion Event Handlers