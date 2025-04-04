﻿// #region Life Cycle

function initMouse() {
	let capture = true;
	window.addEventListener("contextmenu", onWindowContextMenu, capture);

	window.addEventListener("mousedown", onWindowMouseDown, capture);
	window.addEventListener("mousemove", onWindowMouseMove, capture);
	window.addEventListener("mouseup", onWindowMouseUp, capture);

	window.addEventListener('dblclick', onWindowDoubleClick, capture);
	window.addEventListener("click", onWindowClick, capture);

	document.documentElement.addEventListener("pointerenter", onPointerEnter, false);
	document.documentElement.addEventListener("pointerleave", onPointerLeave, false);
}

// #endregion Life Cycle

// #region Getters

function getMouseDragDist() {
	if (isPoint(globals.WindowMouseLoc) &&
		isPoint(globals.WindowMouseDownLoc)) {
		return dist(globals.WindowMouseLoc, globals.WindowMouseDownLoc)
	}
	return 0;
}
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
function isLeftDownEvent(e) {
	if (isNullOrUndefined(e)) {
		return false;
	}
	if (e.which === 1 || e.button === 0) {
		return true;
	}
	return false;
}
// #endregion State

// #region Actions

function updateWindowMouseState(e, eventType) {
	// NOTE this is called from both mouse and dnd events so state is 'always' accurate
	if (!isNullOrUndefined(e)) {
		globals.WindowMouseLoc = getClientMousePos(e);
		if (globals.WindowMouseDownLoc == null &&
			(eventType == 'down' || eventType == 'dragStart')) {
			globals.WindowMouseDownLoc = globals.WindowMouseLoc;
		}
		sendHostMouseState(eventType, isLeftDownEvent(e));
	}
	
	if (globals.WindowMouseDownLoc &&
		(eventType == 'up' || eventType == 'dragEnd' || eventType == 'dragLeave' || eventType == 'drop')) {
		globals.WindowMouseDownLoc = null;
	}
}

function sendHostMouseState(eventType, isLeft) {
	if (isRunningOnCef()) {
		return;
	}
	if (eventType != 'enter' && eventType != 'leave' && eventType != 'up' && eventType != 'down' && eventType != 'move') {
		return;
	}
	onPointerEvent_ntf(eventType, globals.WindowMouseLoc, isLeft);
}

// #endregion Actions

// #region Event Handlers

function onWindowClick(e) {
	if (rejectTableContextMenu(e,'click')) {
		return false;
	} 

	let ignore_classes = [
		"common-toolbar",
		"context-menu-option",
		"ql-toolbar"
	];
	if (isClassInElementPath(e.target, ignore_classes)) {
		return;
	}
	if (!isClassInElementPath(e.target, globals.TemplateEmbedClass)) {
		// unfocus templates 
		if (globals.TemplateBeforeEdit) {
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
	// NOTE using this for right mouse button clicks not normal events since
	// mouse can have N buttons 

	sendHostMouseState('up', true);
	if (!isRunningOnHost()) {
		e.handled = false;
		return;
	}
	if (rejectTableContextMenu(e)) {
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

	if (globals.WasSupressRightMouseDownSentToHost) {
		// sanity check to cleanup any uncaptured mouse ups during a down supress
		// (like if right click down on cell then up outside of editor window)
		globals.WasSupressRightMouseDownSentToHost = false;
		onInternalContextMenuIsVisibleChanged_ntf(false);
	}

	if (rejectTableContextMenu(e)) {
		e.preventDefault();
		e.stopPropagation();
		log('mouse down rejected by table logic')
		return false;
	}
	if (isContextMenuEventGoingToShowTableMenu(e)) {
		// notify host to not show 
		//onInternalContextMenuIsVisibleChanged_ntf(true);
		globals.WasSupressRightMouseDownSentToHost = true;
	}
	
	updateWindowMouseState(e,'down');
	globals.SelectionOnMouseDown = getDocSelection();
	if (e.buttons !== 2 && hasEditableTable()) {
		// deal w/ table drag selection to supppress table select if on already selected cell
		return updateTableDragState(e, 'down');
	}
}

function onWindowMouseMove(e) {
	updateWindowMouseState(e,'move');
	if (hasAnnotations()) {
		onAnnotationWindowPointerMove(e);
	}
	if (globals.IsTableDragSelecting) {
		//updateTableDragState(e, 'move');
	}
	updateSelCursor();
}

function onWindowMouseUp(e) {
	if (globals.WasSupressRightMouseDownSentToHost) {
		delay(300)
			.then(() => {
				onInternalContextMenuIsVisibleChanged_ntf(false);
			});		
	}
	if (rejectTableContextMenu(e)) {
		e.preventDefault();
		e.stopPropagation();
		log('mouse up rejected by table logic')
		return false;
	}

	if (canContextMenuEventShowTableOpsMenu()) {
		// ntf host to suppress context menu while in cell
		onInternalContextMenuIsVisibleChanged_ntf(true);
		globals.WasInternalContextMenuAbleToShow = true;
	} else if (globals.WasInternalContextMenuAbleToShow) {
		onInternalContextMenuIsVisibleChanged_ntf(false);
		globals.WasInternalContextMenuAbleToShow = false;
	}

	updateWindowMouseState(e,'up');
	globals.SelectionOnMouseDown = null;
	updateTableDragState(null,'up');
}

function onPointerEnter(e) {
	updateWindowMouseState(e, 'enter');
}
function onPointerLeave(e) {
	updateWindowMouseState(e, 'leave');
}
// #endregion Event Handlers