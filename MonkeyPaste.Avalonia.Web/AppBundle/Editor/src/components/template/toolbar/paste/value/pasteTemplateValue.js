// #region Globals

// #endregion Globals

// #region Life Cycle

function initTemplateValueTypes() {
	initTemplateContact();
	initPasteTemplateValue();
	initDateTimeTemplate();

	initPasteTemplateDataChangedHandlers();
}

function initPasteTemplateDataChangedHandlers() {
	Array.from(document.getElementsByClassName('template-data-element'))
		.forEach(tdata_elm => {
			tdata_elm.addEventListener('focus', onTemplateDataElementFocus, true);
			tdata_elm.addEventListener('blur', onTemplateDataElementBlur, true);
		});
}

// #endregion Life Cycle

// #region Getters
function getTemplateDataElements() {
	return Array.from(document.getElementsByClassName('template-data-element'));
}

// #endregion Getters

// #region Setters
// #endregion Setters

// #region State
function isAnyTemplateToolbarElementFocused() {
	if (!hasTemplates()) {
		return false;
	}
	const focus_elm = document.activeElement;
	if (!focus_elm) {
		return false;
	}
	if (isChildOfElement(focus_elm, getEditTemplateToolbarContainerElement())) {
		return true;
	}
	if (isChildOfElement(focus_elm, getPasteToolbarContainerElement())) {
		return true;
	}
	return false;
}

// #endregion State

// #region Actions

function focusTemplatePasteValueElement() {
	// focus template text area, paste button then editor when all nothing enabled

	let pvta_elm = getPasteValueTextAreaElement();
	let pb_elm = getPasteButtonElement();

	if (isElementDisabled(pvta_elm)) {
		if (isElementDisabled(pb_elm)) {
			getEditorElement().focus();
		} else {
			pb_elm.focus({ focusVisible: true });
		}
	} else {
		const ft = getFocusTemplate(true);
		if (isTemplateDynamic(ft) || isTemplateStatic(ft)) {
			getPasteValueTextAreaElement().focus({ focusVisible: true });
			return;
		}
	}
}

function finalizeTemplateBeforeEdit() {
	if (!globals.TemplateBeforeEdit) {
		return;
	}

	let cur_et = getTemplateDefByGuid(globals.TemplateBeforeEdit.templateGuid);
	if (cur_et &&
		isTemplateSharedValue(cur_et) &&
		isTemplateDefChanged(globals.TemplateBeforeEdit, cur_et)) {
		// t new or updated
		onAddOrUpdateTemplate_ntf(cur_et);
		// trigger content change  make db content current
		onContentChanged_ntf();
	}

	globals.TemplateBeforeEdit = null;
}

function evalTemplateValue(t) {
	if (!t) {
		return '';
	}
	if (isTemplateDynamic(t)) {
		return t.templateText;
	}
	if (isTemplateStatic(t)) {
		return t.templateData;
	}
	if (isTemplateDateTime(t)) {
		return getFormattedDateTime(null, t.templateData);
	}
	if (isTemplateContact(t)) {
		return getContactFieldValue(t.templateState, t.templateData);
	}
	return '';
}
// #endregion Actions

// #region Event Handlers

function onTemplateDataElementFocus(e) {
	log('template data element FOCUSED: ' + e.currentTarget.id);
	let focus_check = isAnyTemplateToolbarElementFocused();
	log('focus checker says: ' + (focus_check ? "TRUE" : "FALSE"));

	const ft = getFocusTemplate(true);
	if (ft && globals.TemplateBeforeEdit && ft.templateGuid == globals.TemplateBeforeEdit.templateGuid) {
		// no change in cur edit template, ignore
		return;
	}

	if (ft && globals.TemplateBeforeEdit && ft.templateGuid != globals.TemplateBeforeEdit.templateGuid) {
		// new edit template from previous
		finalizeTemplateBeforeEdit();
		globals.TemplateBeforeEdit = ft;
		return;
	}
	if (!ft) {
		// finalize edit (if there was any)
		finalizeTemplateBeforeEdit();
	}
	globals.TemplateBeforeEdit = ft;

}
function onTemplateDataElementBlur(e) {
	log('template data element BLURRED: ' + e.currentTarget.id);

	function onNextFocus(e2) {
		document.body.removeEventListener('focus', onNextFocus);
		if (isAnyTemplateToolbarElementFocused()) {
			return;
		}
		delay(500)
			.then(() => {
				finalizeTemplateBeforeEdit();
			});
	}
	// need to know when blur goes from template to not template
	// discrete events make harder so when template data elm blurred
	// check NEXT focus, when NOT tde wait for other handlers to process changes
	// so finalize has current values in template def
	document.body.addEventListener('focus', onNextFocus, true);
}

// #endregion Event Handlers