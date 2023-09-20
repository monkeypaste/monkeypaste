// #region Life Cycle
function initKeyboard() {
	window.addEventListener('keydown', handleWindowKeyDown, true);
	window.addEventListener('keyup', handleWindowKeyUp, true);
}
// #endregion Life Cycle

// #region Getters

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
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isKeyboardButtonClick(e) {
	if (e.key == ' ' || e.key == 'Enter') {
		return true;
	}
	return false;
}

function isModKeysDifferent(mods1,mods2) {
	mods1 = cleanModKeys(mods1);
	mods2 = cleanModKeys(mods2);

	let isModChanged =
		mods1.IsMetaDown != mods2.IsMetaDown ||
		mods1.IsCtrlDown != mods2.IsCtrlDown ||
		mods1.IsAltDown != mods2.IsAltDown ||
		mods1.IsShiftDown != mods2.IsShiftDown;
	return isModChanged;
}
// #endregion State

// #region Actions

function cleanModKeys(e) {
	if (!e) {
		e = {
			IsMetaDown: false,
			IsCtrlDown: false,
			IsAltDown: false,
			IsShiftDown: false
		};
	}
	if (e.metaKey !== undefined) {
		e.IsMetaDown = e.metaKey;
	}
	if (e.ctrlKey !== undefined) {
		e.IsCtrlDown = e.ctrlKey;
	}
	if (e.altKey !== undefined) {
		e.IsAltDown = e.altKey;
	}
	if (e.shiftKey !== undefined) {
		e.IsShiftDown = e.shiftKey;
	}

	if (e.IsMetaDown === undefined) {
		e.IsMetaDown = false;
	}
	if (e.IsCtrlDown === undefined) {
		e.IsCtrlDown = false;
	}
	if (e.IsAltDown === undefined) {
		e.IsAltDown = false;
	}
	if (e.IsShiftDown === undefined) {
		e.IsShiftDown = false;
	}
	return e;
}

function updateGlobalModKeys(e) {
	e = cleanModKeys(e);

	let isModChanged = isModKeysDifferent(globals.ModKeys,e);

	globals.ModKeys = e;

	if (isModChanged) {
		log('mod changed: Ctrl: ' + (globals.ModKeys.IsCtrlDown ? "YES" : "NO"));
		drawOverlay();
	}
}
// #endregion Actions

// #region Event Handlers

function handleWindowKeyDown(e) {
	if (!isWindowFocused()) {
		return;
	}
	let suppresKeyDown = true;
	updateGlobalModKeys(e);

	if (isSubSelectionEnabled()) {
		if (!globals.quill.hasFocus()) {
			suppresKeyDown = false;
		}
		if (isReadOnly()) {
			//sub-select/droppable mode
			if (e.key == globals.IncreaseFocusLevelKey) {
				disableReadOnly();
			} else if (e.key == globals.DecreaseFocusLevelKey) {
				disableSubSelection();
			} else if (globals.NavigationKeys.includes(e.key)) {
				// allow for navigation input
				suppresKeyDown = false;
			}
		} else {
			// edit mode all input allowed
			suppresKeyDown = false;
		}
	} else {
		// no edit mode
		if (e.key == globals.IncreaseFocusLevelKey) {
			enableSubSelection();
		}
	}

	if (suppresKeyDown) {
		// allow shortcuts during any state
		// check for shortcut or special navigation input
		let is_non_input_key_down = globals.ModKeys.IsMetaDown || globals.ModKeys.IsCtrlDown || globals.ModKeys.IsAltDown;
		if (is_non_input_key_down) {
			suppresKeyDown = false;
		}
		if (suppresKeyDown) {
			e.stopPropagation();
			e.preventDefault();
		}
	}
}

function handleWindowKeyUp(e) {
	if (!isWindowFocused()) {
		return;
	}
	updateGlobalModKeys(e);

	if (e.code == globals.DecreaseFocusLevelKey) {
		if (isDragging() || isDropping()) {// || WasDragCanceled) {
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
			if (globals.quill.hasFocus()) {
				setDocSelection(sel.index, 0);
			}

			return;
		}
	}
}
// #endregion Event Handlers




