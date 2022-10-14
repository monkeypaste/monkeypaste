//var LastWindowMouseDownLoc = null;
var WindowMouseDownLoc = null;
var WindowMouseLoc = null;

var DecreaseFocusLevelKey = 'Escape'
var IncreaseFocusLevelKey = ' ';

var PermittedNoSelectKeys = [
	IncreaseFocusLevelKey
];

var PermittedSubSelectionKeys = [
	"ArrowLeft",
	"ArrowUp",
	"ArrowRight",
	"ArrowDown",
	"Shift",
	"Alt",
	"Control",
	"Home",
	"End",
	"PageUp",
	"PageDown",
	DecreaseFocusLevelKey,
	IncreaseFocusLevelKey
];

function initWindow() {
	window.addEventListener("resize", onWindowResize, true);
	window.addEventListener('scroll', onWindowScroll);

	window.addEventListener("mousedown", onWindowMouseDown);
	window.addEventListener("mousemove", onWindowMouseMove);
	window.addEventListener("mouseup", onWindowMouseUp);

	window.addEventListener('dblclick', onWindowDoubleClick);
	window.addEventListener("click", onWindowClick);

	window.addEventListener('keydown', onWindowKeyDown);
	window.addEventListener('keyup', onWindowKeyUp);
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

function onWindowScroll(e) {
	updateAllSizeAndPositions();
}

function onWindowResize(e) {
	updateAllSizeAndPositions();
	drawOverlay();
}

function onWindowKeyDown(e) {
	if (IsReadOnly) {
		if (!IsSubSelectionEnabled) {
			// no edit mode
			if (PermittedNoSelectKeys.includes(e.key)) {
				if (e.key == IncreaseFocusLevelKey) {
					enableSubSelection();
				}
			}
		} else {
			//sub-select/droppable mode
			if (PermittedSubSelectionKeys.includes(e.key)) {
				if (e.key == IncreaseFocusLevelKey) {
					disableReadOnly();
				} else if (e.key == DecreaseFocusLevelKey) {
					disableSubSelection();
				}
			}
		}

		e.stopPropagation();
		e.preventDefault();
	} 
}

function onWindowKeyUp(e) {
	if (e.code == DecreaseFocusLevelKey) {
		if (IsDragging || IsDropping || WasDragCanceled) {			
			return;
		}
		if (isTemplateFocused()) {
			clearTemplateFocus();
			if (!IsPastingTemplate) {
				hideEditTemplateToolbar();
			}
			return;
		}


		if (IsSubSelectionEnabled) {
			let sel = getEditorSelection();
			if (!sel || sel.length == 0) {
				if (!IsReadOnly) {
					enableReadOnly();
					return;
				}
				disableSubSelection();
				return;
			}
			setEditorSelection(sel.index, 0);
			return;
		}
	}
}

function getWindowRect() {
	let wrect = cleanRect();
	wrect.right = window.innerWidth;
	wrect.bottom = window.innerHeight;
	wrect = cleanRect(wrect);
	return wrect;
}