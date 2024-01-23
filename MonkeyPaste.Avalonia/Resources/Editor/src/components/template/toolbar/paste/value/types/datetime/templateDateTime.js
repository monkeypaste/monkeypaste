
// #region Life Cycle

function initDateTimeTemplate() {
    getDateTimeTemplateFormatSelectorElement().addEventListener('change', onDateTimeSelFormatChanged);

    getDateTimeTemplateCustomInputElement().addEventListener('keydown', onTemplateDateTimeCustomInputKeyDown, true);
    getDateTimeTemplateCustomInputElement().addEventListener('blur', onTemplateDateTimeCustomInputBlur, true);
}

// #endregion Life Cycle

// #region Getters

function getFormattedDateTime(dt, format) {
    // from 'https://github.com/phstc/jquery-dateFormat'
    dt = isNullOrUndefined(dt) ? new Date() : dt;
    return jQuery.format.date(dt, format);
}

function getDateTimeTemplateOuterContainerElement() {
    return document.getElementById('pasteTemplateToolbarDateTimeFormatSelectorContainer');
}

function getDateTimeTemplateCustomInputElement() {
    return document.getElementById('datetimeCustomInput');
}

function getDateTimeTemplateFormatSelectorElement() {
    return document.getElementById('datetimeSelector');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isTemplateDateTimeCustom(t) {
    if (!isTemplateDateTime(t)) {
        return false;
    }
    return globals.TemplateDateTimeFormatOptionLabels.includes(t.templateData) == false;
}
// #endregion State

// #region Actions

function createDateTimeFormatSelectorOpts(ft) {
    let datetime_sel_elm = getDateTimeTemplateFormatSelectorElement();
    datetime_sel_elm.innerHTML = '';
    const cur_dt = new Date();
    for (var i = 0; i < globals.TemplateDateTimeFormatOptionLabels.length; i++) {
        let datetime_format = globals.TemplateDateTimeFormatOptionLabels[i];
        let datetime_opt_elm = document.createElement('option');
        datetime_opt_elm.value = datetime_format;
        if (datetime_format == globals.CUSTOM_TEMPLATE_LABEL_VAL) {
            datetime_opt_elm.innerText = globals.CUSTOM_TEMPLATE_LABEL_VAL;
        } else {
            datetime_opt_elm.innerText = getFormattedDateTime(cur_dt, datetime_format);
        }        
        datetime_sel_elm.appendChild(datetime_opt_elm);
    }


    let datetime_input_elm = getDateTimeTemplateCustomInputElement();
    datetime_input_elm.classList.add('hidden');
    if (ft && ft.templateType.toLowerCase() == 'datetime') {
        // set dt format selection
        if (isTemplateDateTimeCustom(ft)) {
            // template has custom format
            datetime_sel_elm.value = globals.CUSTOM_TEMPLATE_LABEL_VAL;

            // show custom input
            datetime_input_elm.value = getFormattedDateTime(null, ft.templateData);
            datetime_input_elm.classList.remove('hidden');
        } else {
            // template has preset dt
            datetime_sel_elm.value = ft.templateData;
        } 
    }
}


function updateDateTimeFieldSelectorToFocus(ft) {
    if (!ft || ft.templateType.toLowerCase() != 'datetime') {
        getDateTimeTemplateOuterContainerElement().classList.add('hidden');
        return;
    }
    getDateTimeTemplateOuterContainerElement().classList.remove('hidden');
    createDateTimeFormatSelectorOpts(ft);
    updateDateTimeTemplateToOptionChange();
}

function updateDateTimeTemplateToOptionChange() {
    // NOTE always refresh all data regardless if its field or contact that changes

    let ft = getFocusTemplate();
    if (!ft || ft.templateType.toLowerCase() != 'datetime') {
        return;
    }

    // SET TEMPLATE DATA
    let field_sel_elm = getDateTimeTemplateFormatSelectorElement();
    if (!field_sel_elm) {
        return;
    }
    let custom_format_elm = getDateTimeTemplateCustomInputElement();
    if (field_sel_elm.value == globals.CUSTOM_TEMPLATE_LABEL_VAL) {
        custom_format_elm.value = getFormattedDateTime(null, ft.templateData);
        custom_format_elm.classList.remove('hidden');

        ft.templateData = custom_format_elm.value;
    } else {
        custom_format_elm.classList.add('hidden');
        ft.templateData = field_sel_elm.value;
    }
    setTemplateData(ft.templateGuid, ft.templateData);
    let new_ft_pv = getFormattedDateTime(null, ft.templateData);
    let sup_guid = suppressTextChanged();
    setTemplatePasteValue(ft.templateGuid, new_ft_pv);
    unsupressTextChanged(sup_guid);
}
// #endregion Actions

// #region Event Handlers

function onDateTimeSelFormatChanged(e) {
    if (getDateTimeTemplateFormatSelectorElement().value == globals.CUSTOM_TEMPLATE_LABEL_VAL) {
        setTemplateData(getFocusTemplateGuid(), '');
    }
    updateDateTimeTemplateToOptionChange();
}
function onTemplateDateTimeCustomInputBlur(e) {
    updateDateTimeTemplateToOptionChange();
}
function onTemplateDateTimeCustomInputKeyDown(e) {
    if (e.key == 'Enter') {
        e.preventDefault();
    }
}
// #endregion Event Handlers