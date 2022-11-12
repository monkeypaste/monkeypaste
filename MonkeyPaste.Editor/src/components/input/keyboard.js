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
		if (!quill.hasFocus()) {
			suppresKeyDown = false;
		}
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
			if (!isShowingPasteToolbar()) {
				hideEditTemplateToolbar(true);
			}
			return;
		}


		if (isSubSelectionEnabled()) {
			let sel = getDocSelection();
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
				setDocSelection(sel.index, 0);
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

function getDownModKeys(e) {
	let down_mod_keys = [];
	if (e.getModifierState("Shift")) {
		down_mod_keys.push('Shift');
	}
	if (e.getModifierState("Control")) {
		down_mod_keys.push('Control');
	}
	if (e.getModifierState("Alt")) {
		down_mod_keys.push('Alt');
	}
	return down_mod_keys;
}

function isKeyboardButtonClick(e) {
	if (e.key == ' ' || e.key == 'Enter') {
		return true;
	}
	return false;
}

function getArrowVal(key) {
	if (key == 'ArrowLeft') {
		return { x: -1, y: 0 };
	}
	if (key == 'ArrowRight') {
		return { x: 1, y: 0 };
	}
	if (key == 'ArrowUp') {
		return { x: 0, y: -1 };
	}
	if (key == 'ArrowDown') {
		return { x: 0, y: 1 };
	}
	return null;
}
