
var WindowMouseDownLoc = null;

function initWindow() {
	window.addEventListener("resize", onWindowResize, true);
	window.addEventListener('scroll', onWindowScroll);

	window.addEventListener("mousedown", onWindowMouseDown);
	window.addEventListener("mouseup", onWindowMouseUp);
	window.addEventListener("click", onWindowClick);


	window.addEventListener('keydown', onWindowKeyUp);
}

function initWindowDragDrop() {
	//dragdrop handlers
	window.addEventListener("mousedown", onWindowMouseDown_dragdrop);
	window.addEventListener('mousemove', onWindowMouseMove_dragdrop);

	// from https://stackoverflow.com/a/46986927/105028
	window.addEventListener('dragstart', onWindowDragStart_override, true);
	window.addEventListener('dragend', onWindowDragEnd_ovveride, true);
	window.addEventListener('drop', onWindowDrop_override, true);

	//window.addEventListener('dragenter', function (event) {
    //    var event2 = new CustomEvent('mp_dragenter', { detail: { original: event } });
    //    event.target.dispatchEvent(event2);

    //    event.stopPropagation();
    //}, true);

    //window.addEventListener('dragover', function (/*event*/) {
    //    //var event2 = new CustomEvent('mp_dragover', { detail: { original: event } });
    //    //event.target.dispatchEvent(event2);


    //    event.stopPropagation();
    //}, true);


    //window.addEventListener('dragover', function (event) {
    //    var event2 = new CustomEvent('mp_dragover', { detail: { original: event } });
    //    event.target.dispatchEvent(event2);

    //    event.stopPropagation();
    //}, true);
}

function onWindowClick(e) {
	if (
		e.path.find(
			(x) => x.classList && x.classList.contains("edit-template-toolbar")
		) != null ||
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
			(x) => x.classList && x.classList.contains("ql-template-embed-blot")
		) == null
	) {
		// unfocus templates 
		hideAllTemplateContextMenus();
		hideEditTemplateToolbar();
		hidePasteTemplateToolbar();
		clearTemplateFocus();
	}

}

function onWindowMouseDown(e) {
	WindowMouseDownLoc = { x: e.clientX, y: e.clientY };
}

function onWindowMouseUp(e) {
	if (!WindowMouseDownLoc) {
		// mouse outside editor
		return;
	}
	let window_up_mp = { x: e.clientX, y: e.clientY };
	let wmdl_delta_dist = dist(WindowMouseDownLoc, window_up_mp);
	if (wmdl_delta_dist > MIN_DRAG_DIST) {
		// ignore any drags
		return;
	}

	let sel = getSelection();
	if (!sel) {
		// mouse outside editor
		return;
	}
	if (sel.length > 0) {
		// there's a sel range

		let mp = getEditorMousePos(e);
		let editor_rect = getEditorContainerRect();
		if (!isPointInRect(editor_rect, mp)) {
			// ignore when mouse up outside editor
			return;
		}
		let was_sel_click = isPointInRange(mp, sel);
		if (!was_sel_click) {
			// this a workaround for weird click bug to clear selection

			// when click not on selection clear selection and move caret to mp doc idx
			let mp_doc_idx = getDocIdxFromPoint(mp);
			if (mp_doc_idx < 0) {
				// fallback to old range start
				mp_doc_idx = sel.index;
			}
			setEditorSelection(mp_doc_idx, 0);
		}
	}

	WindowMouseDownLoc = null;
}

function onWindowScroll(e) {
	if (isReadOnly()) {
		return;
	}

	updateAllSizeAndPositions();
}

function onWindowResize(e) {
	updateAllSizeAndPositions();
	drawOverlay();
}

// DRAG DROP

function onWindowKeyUp(e) {
	if (e.code == 'Escape') {
		if (IsDragging) {
			endDrag(true);
			return;
		}
		if (isTemplateFocused()) {
			clearTemplateFocus();
			return;
		}
		let sel = getSelection();
		if (!sel) {
			return;
		}
		setEditorSelection(sel.index, 0);
	}
	
}

function onWindowMouseDown_dragdrop(e) {
	// used to notify host of drag may need to remove if editor initiaites drag
	if (!IsSubSelectionEnabled) {
		onContentDraggableChanged_ntf(true);
		return;
	}

	let sel = getSelection();
	let is_none_selected = sel == null || sel.length == 0;
	if (is_none_selected) {
		onContentDraggableChanged_ntf(false);
		return;
	}

	let emp = getEditorMousePos(e);
	let is_down_on_range = isPointInRange(emp, sel);
	let is_all_selected = isAllSelected();

	is_draggable = is_down_on_range || is_all_selected;
	onContentDraggableChanged_ntf(is_draggable);
}

function onWindowMouseMove_dragdrop(e) {
	//let offset = getDocIdxFromPoint({ x: parseFloat(e.clientX), y: parseFloat(e.clientY) });
	//log('offset: ' + offset);

	if (!isDropping()) {
		if (parseInt(e.buttons) != 0) {
			// mouse button is down but not dragging


			//showTemplateUserSelection();
		}

		return;
	}
	log('window.mousemove dragover fallback resetting drag drop');
	resetDragDrop();
	return;

	if (parseInt(e.buttons) == 0) {
		resetDragDrop();
		return;
	}

	// dragging must be outside editor (otherwise this event is suppressed), likely in a toolbar but still within window
	if (DropIdx < 0) {
		return;
	}
	LastMousePos = getEditorMousePos(e);
	LastMouseUpdateDateTime = Date.now();

	DropIdx = -1;
	drawOverlay();
}

function onWindowDragStart_override(event) {
	event.dataTransfer.effectAllowed = 'all';

	var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
	event.target.dispatchEvent(event2);

	event.stopPropagation();

	onDragStart(event);
}

function onWindowDragEnd_ovveride(event) {
	//var event2 = new CustomEvent('mp_dragend', { detail: { original: event } });
	//event.target.dispatchEvent(event2);

	event.stopPropagation();

	resetDragDrop(true);
}

function onWindowDrop_override(event) {
	//var event2 = new CustomEvent('mp_drop', { detail: { original: event } });
	//event.target.dispatchEvent(event2);

	event.stopPropagation();

	onDrop(event);
}