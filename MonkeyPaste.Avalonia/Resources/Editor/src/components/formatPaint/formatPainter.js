// #region Globals

// #endregion Globals

// #region Life Cycle

function initFormatPaint() {
	addClickOrKeyClickEventListener(getFormatPaintToolbarElement(), onFormatPaintToolbarButtonClick);
}

// #endregion Life Cycle

// #region Getters
function getFormatPaintToolbarElement() {
	return document.getElementById('formatPaintToolbarButton');
}

function getCurPaintFormat() {
	return globals.CurPaintFormat;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isFormatPaintToggled() {
	const is_toggled = Array.from(getFormatPaintToolbarElement().querySelectorAll('path'))[0].classList.contains('toggled');
	return is_toggled;
}

function isFormatPaintLocked() {
	const is_toggled = Array.from(getFormatPaintToolbarElement().querySelectorAll('path'))[0].classList.contains('locked');
	return is_toggled;
}

// #endregion State

// #region Actions
function setFormatPaintToolbarButtonToggleState(isToggled,isLocked = false) {
	let fp_svg_path_elms = Array.from(getFormatPaintToolbarElement().querySelectorAll('path'));
	fp_svg_path_elms.forEach(x => isToggled ? x.classList.add('toggled') : x.classList.remove('toggled'));
	fp_svg_path_elms.forEach(x => isLocked ? x.classList.add('locked') : x.classList.remove('locked'));
}

function paintFormatOnSelection() {
	let sel = getDocSelection();
	if (!sel || sel.length == 0) {
		return;
	}

	formatDocRange(sel, globals.CurPaintFormat, 'user');
	if (!isFormatPaintLocked()) {
		disableFormatPaint();
	}
}


function enableFormatPaint(isLocked) {
	let sel = getDocSelection() || { index: 0, length: 0 };
	setFormatPaintToolbarButtonToggleState(true,isLocked);
	globals.CurPaintFormat = getFormatForDocRange(sel);

	getEditorElement().addEventListener('mouseup', onEditorSelChangedForFormatPaint);
}

function disableFormatPaint() {
	setFormatPaintToolbarButtonToggleState(false);
	globals.CurPaintFormat = null;

	getEditorElement().removeEventListener('mouseup', onEditorSelChangedForFormatPaint);
}

// #endregion Actions

// #region Event Handlers

function onFormatPaintToolbarButtonClick(e) {
	if (e.detail == 1) {
		if (isFormatPaintToggled()) {
			disableFormatPaint();
		} else {
			enableFormatPaint();
		}
		return;
	}
	if (!isFormatPaintToggled()) {
		return;
	}
	if (e.detail == 2) {
		enableFormatPaint(true);
		return;
	}
	disableFormatPaint();	
}

function onEditorSelChangedForFormatPaint(e) {
	paintFormatOnSelection();
}
// #endregion Event Handlers