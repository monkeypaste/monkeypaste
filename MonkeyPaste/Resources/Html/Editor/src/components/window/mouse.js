function initMouse() {
	window.addEventListener("mousedown", onWindowMouseDown);
	window.addEventListener("mousemove", onWindowMouseMove);
	window.addEventListener("mouseup", onWindowMouseUp);

	window.addEventListener('dblclick', onWindowDoubleClick);
	window.addEventListener("click", onWindowClick);
}

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
		hideAllTemplateContextMenus();
		hideEditTemplateToolbar();
		if (IsPastingTemplate) {
			return;
		}
		hidePasteTemplateToolbar();
		clearTemplateFocus();
	}
}

function onWindowDoubleClick(e) {
	//if (IsSubSelectionEnabled) {
	//	disableReadOnly();
	//	return;
	//}
	enableSubSelection();
}

function onWindowMouseDown(e) {
	if (!isChildOfElement(e.target, getEditorContainerElement())) {
		log('window mouse down rejected ', e.target, ' is not a child of editor container');
		return;
	}
	WindowMouseDownLoc = { x: e.clientX, y: e.clientY };
	//LastWindowMouseDownLoc = WindowMouseDownLoc;
}

function onWindowMouseMove(e) {
	// NOTE! not called during drag over

	//if (!isChildOfElement(e.target, getEditorContainerElement())) {
	//	log('window mouse move rejected ', e.target, ' is not a child of editor container');
	//	return;
	//}
	WindowMouseLoc = { x: e.clientX, y: e.clientY };
	//if (WindowMouseDownLoc == null) {
	//	return;
	//}
	//if (!IsReadOnly || IsSubSelectionEnabled) {
	//	return;
	//}
	//let cur_dist = dist(WindowMouseLoc, WindowMouseDownLoc);
	//log('drag dist: ' + cur_dist);
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

	//if (!isChildOfElement(e.target, getEditorContainerElement())) {
	//	log('window mouse up rejected ', e.target, ' is not a child of editor container');
	//	return;
	//}
	//LastWindowMouseDownLoc = WindowMouseDownLoc;
	let last_dmp = WindowMouseDownLoc;
	WindowMouseDownLoc = null;
	return;

	//if (!WindowMouseDownLoc) {
	//	// mouse outside editor
	//	return;
	//}
	//WindowMouseDownLoc = null;
	//return;
	if (last_dmp == null) {
		return;
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

