//var LastWindowMouseDownLoc = null;
var WindowMouseDownLoc = null;
var WindowMouseLoc = null;

var PermittedReadOnlyKeys = [
	"ArrowLeft",
	"ArrowUp",
	"ArrowRight",
	"ArrowDown",
	"Escape",
	"Shift",
	"Alt",
	"Control",
	"Home",
	"End",
	"PageUp",
	"PageDown"
];

function initWindow() {
	window.addEventListener("resize", onWindowResize, true);
	window.addEventListener('scroll', onWindowScroll);

	window.addEventListener("mousedown", onWindowMouseDown);
	window.addEventListener("mousemove", onWindowMouseMove);
	window.addEventListener("mouseup", onWindowMouseUp);

	window.addEventListener("click", onWindowClick);

	window.addEventListener('keydown', onWindowKeyDown);
	window.addEventListener('keyup', onWindowKeyUp);

	window.addEventListener('dblclick', onWindowDoubleClick);
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
			(x) => x.classList && x.classList.contains(TemplateEmbedClass)
		) == null
	) {
		// unfocus templates 
		hideAllTemplateContextMenus();
		hideEditTemplateToolbar();
		hidePasteTemplateToolbar();
		clearTemplateFocus();
	}
}

function onWindowDoubleClick(e) {
	if (IsSubSelectionEnabled) {
		return;
	}
	enableSubSelection();
}

function onWindowMouseDown(e) {
	if (!isChildOfElement(e.target, getEditorContainerElement())) {
		return;
	}
	WindowMouseDownLoc = { x: e.clientX, y: e.clientY };
	//LastWindowMouseDownLoc = WindowMouseDownLoc;
}
function onWindowMouseMove(e) {
	// NOTE! not called during drag over

	if (!isChildOfElement(e.target, getEditorContainerElement())) {
		return;
	}
	WindowMouseLoc = { x: e.clientX, y: e.clientY };
	//if (WindowMouseDownLoc == null) {
	//	return;
	//}
	//const selection = document.getEditorSelection();
	//const start_range = document.caretRangeFromPoint(WindowMouseDownLoc.x, WindowMouseDownLoc.y);
	//const cur_range = document.caretRangeFromPoint(e.clientX, e.clientY);

	//let actual_range = {};
	//if (cur_range.startOffset < start_range.startOffset) {
	//	actual_range.index = cur_range.startOffset;
	//	actual_range.length = start_range.startOffset - cur_range.startOffset;
	//} else {
	//	actual_range.index = start_range.startOffset;
	//	actual_range.length = cur_range.startOffset - start_range.startOffset;
	//}
	//updateTemplatesAfterSelectionChange(actual_range);
	//log(' ');
	//log('start sidx: ' + start_range.startOffset + ' eidx: ' + start_range.endOffset);
	//log('cur sidx: ' + cur_range.startOffset + ' eidx: ' + cur_range.endOffset);
}

function onWindowMouseUp(e) {

	if (!isChildOfElement(e.target, getEditorContainerElement())) {
		return;
	}
	//LastWindowMouseDownLoc = WindowMouseDownLoc;
	let last_dmp = WindowMouseDownLoc;
	WindowMouseDownLoc = null;
	//if (!WindowMouseDownLoc) {
	//	// mouse outside editor
	//	return;
	//}
	//WindowMouseDownLoc = null;
	//return;
	if (last_dmp == null) {
		debugger;
	}
	let window_up_mp = { x: e.clientX, y: e.clientY };
	let wmdl_delta_dist = dist(last_dmp, window_up_mp);
	if (wmdl_delta_dist > MIN_DRAG_DIST) {
		// ignore any drags
		return;
	}

	let sel = getEditorSelection();
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

}

function onWindowScroll(e) {
	if (isContentEditable()) {
		return;
	}

	updateAllSizeAndPositions();
}

function onWindowResize(e) {
	updateAllSizeAndPositions();
	drawOverlay();
}

function onWindowKeyDown(e) {
	if (IsReadOnly) {
		if (PermittedReadOnlyKeys.contains(e.key)) {
			return;
		}
		e.stopPropagation();
		e.preventDefault();
	}
}

// DRAG DROP

function onWindowKeyUp(e) {
	if (e.code == 'Escape') {
		if (IsDragging || IsDropping) {			
			return;
		}
		if (isTemplateFocused()) {
			clearTemplateFocus();
			return;
		}

		return;

		let sel = getEditorSelection();
		if (!sel) {
			return;
		}
		if (!isReadOnly()) {
			if (!sel) {
				return;
			}
			setEditorSelection(sel.index, 0);
			return;
		}

		if (IsSubSelectionEnabled) {
			if (sel.length == 0) {
				disableSubSelection();
				return;
			}
			setEditorSelection(sel.index, 0);
			return;
		}
		setEditorSelection(sel.index, 0);
		return;
	}	
}

function onWindowMouseDown_dragdrop(e) {
	// used to notify host of drag may need to remove if editor initiaites drag
	let mp = getEditorMousePos(e);
	let can_drag = checkCanDrag(mp);
	onContentDraggableChanged_ntf(can_drag);
}

function onWindowMouseMove_dragdrop(e) {

	//let offset = getDocIdxFromPoint({ x: parseFloat(e.clientX), y: parseFloat(e.clientY) });
	//log('offset: ' + offset);
	if (IsDragging) {
		return;
	}
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
	WindowMouseLoc = getEditorMousePos(e);
	LastMouseUpdateDateTime = Date.now();

	DropIdx = -1;
	drawOverlay();
}

function onWindowDragStart_override(e) {
	//if (isRunningInHost()) {
	//	e.dataTransfer.effectAllowed = 'none';
	//	e.preventDefault();
	//} else {
	//	e.dataTransfer.effectAllowed = 'all';

	//}	
	e.dataTransfer.effectAllowed = 'copyMove';

	//var event2 = new CustomEvent('mp_dragstart', { detail: { original: e } });
	//e.target.dispatchEvent(event2);

	//e.preventDefault();
	e.stopPropagation();

	//if (isRunningInHost()) {
	//	onDragStart_ntf();
	//} else {
	//	onDragStart(e);
	//}
	onDragStart(e);
}

function onWindowDragEnd_ovveride(event) {
	//var event2 = new CustomEvent('mp_dragend', { detail: { original: event } });
	//event.target.dispatchEvent(event2);

	event.stopPropagation();

	if (!isRunningInHost()) {

		resetDragDrop(true);
	}
}

function onWindowDrop_override(event) {
	//var event2 = new CustomEvent('mp_drop', { detail: { original: event } });
	//event.target.dispatchEvent(event2);

	event.stopPropagation();
	if (!isRunningInHost()) {

	}
	onDrop(event);
}