const KEY_COMBO_SEPARATOR = '+';
const KEY_SEQUENCE_SEPARATOR = ',';

var DecreaseFocusLevelKey = 'Escape'
var IncreaseFocusLevelKey = ' ';

var PermittedNoSelectKeys = [
	IncreaseFocusLevelKey
];

var NavigationKeys = [
	"ArrowLeft",
	"ArrowUp",
	"ArrowRight",
	"ArrowDown",
	//"Shift",
	//"Alt",
	//"Control",
	//"Meta",
	"Home",
	"End",
	"PageUp",
	"PageDown",
	//DecreaseFocusLevelKey,
	//IncreaseFocusLevelKey
];

var IsMetaDown = false; //duplicate (mac)
var IsCtrlDown = false; //duplicate
var IsShiftDown = false; //split 
var IsAltDown = false; // w/ formatting (as html)? ONLY formating? dunno

function initKeyboard() {
	window.addEventListener('keydown', handleWindowKeyDown, true);
	window.addEventListener('keyup', handleWindowKeyUp, true);
}

function handleWindowKeyDown(e) {
	if (!isWindowFocused()) {
		return;
	}
	let suppresKeyDown = true;
	updateModKeys(e);

	if (isSubSelectionEnabled()) {
		if (isReadOnly()) {
			//sub-select/droppable mode
			if (e.key == IncreaseFocusLevelKey) {
				disableReadOnly();
			} else if (e.key == DecreaseFocusLevelKey) {
				disableSubSelection();
			} else if (NavigationKeys.includes(e.key)) {
				// allow for navigation input
				suppresKeyDown = false;
			} else {
				// check for shortcut or special navigation input
				let is_non_input_key_down = IsMetaDown || IsCtrlDown || IsAltDown;
				if (is_non_input_key_down) {
					suppresKeyDown = false;
				}
			}
		} else {
			// edit mode all input allowed
			suppresKeyDown = false;
		}
	} else {
		// no edit mode
		if (e.key == IncreaseFocusLevelKey) {
			enableSubSelection();
		}
	}

	if (suppresKeyDown) {
		e.stopPropagation();
		e.preventDefault();
	}
}

function handleWindowKeyUp(e) {
	if (!isWindowFocused()) {
		return;
	}
	updateModKeys(e);

	if (e.code == DecreaseFocusLevelKey) {
		if (IsDragging || IsDropping || WasDragCanceled) {
			return;
		}
		if (isTemplateFocused()) {
			clearTemplateFocus();
			if (!IsPastingTemplate) {
				hideEditTemplateToolbar(true);
			}
			return;
		}


		if (isSubSelectionEnabled()) {
			let sel = getEditorSelection();
			if (!sel || sel.length == 0) {
				if (!isReadOnly()) {
					enableReadOnly();
					return;
				}
				disableSubSelection();

				e.stopPropagation();
				e.preventDefault();
				return;
			}
			if (quill.hasFocus()) {
				setEditorSelection(sel.index, 0);
			}
			
			return;
		}
	}
}

function updateModKeys(e) {
	//if (e.fromHost === undefined && isRunningInHost()) {
	//    // ignore internal mod key updates when running from host
	//    return;
	//}

	let isModChanged =
		IsMetaDown != e.metaKey ||
		IsCtrlDown != e.ctrlKey ||
		IsAltDown != e.altKey ||
		IsShiftDown != e.shiftKey;

	IsMetaDown = e.metaKey;
	IsCtrlDown = e.ctrlKey;
	IsAltDown = e.altKey;
	IsShiftDown = e.shiftKey;

	if (isModChanged) {
		log('mod changed: Ctrl: ' + (IsCtrlDown ? "YES" : "NO"));
		drawOverlay();
	}
}

function isKeyboardButtonClick(e) {
	if (e.key == ' ' || e.key == 'Enter') {
		return true;
	}
	return false;
}
