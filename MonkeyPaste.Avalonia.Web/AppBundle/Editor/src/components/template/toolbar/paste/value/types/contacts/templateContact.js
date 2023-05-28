// #region Globals
// #endregion Globals

// #region Life Cycle
function initTemplateContact() {
    getContactFieldSelectorElement().addEventListener('change', onContactFieldChanged);
    getContactSelectorElement().addEventListener('change', onSelectedContactFieldChanged);
}

// #endregion Life Cycle

// #region Getters

function getContactTestData() {
    let dummy_contact1 = {
        label: 'Mikey Mikerson',
        guid: '25ee8f84-c504-4a4a-82e6-b7be17535541'
    };
    let dummy_contact2 = {
        label: 'Bill Lambeer',
        guid: '91efa767-4d67-49d2-b3ea-3aac7dc5e5e4'
    };
    let dummy_contact3 = {
        label: 'Feta Daniels',
        guid: '54e6c039-2f45-4de0-909a-a0eba1fa0be8'
    };
    let dummy_contacts = [
        dummy_contact1,
        dummy_contact2,
        dummy_contact3
    ];
    for (var i = 0; i < dummy_contacts.length; i++) {
        dummy_contacts[i]['FullName'] = dummy_contacts[i].label;
        dummy_contacts[i]['FirstName'] = dummy_contacts[i].label.split(' ')[0];
        if (i == 0) {
            continue;
        }
        dummy_contacts[i]['LastName'] = dummy_contacts[i].label.split(' ')[1];
    }
    return dummy_contacts;
}


function getContactFieldValue(contact_guid, fieldName) {
    let contact_field_value = getContactFieldValue_get(contact_guid, fieldName);
    if (!contact_field_value) {
        return '';
    }
    return contact_field_value;
}

function getAllContactFields() {
    let all_fields = [];
    for (var i = 0; i < globals.AvailableContacts.length; i++) {
        let cur_fields = getContactFields(globals.AvailableContacts[i]);
        for (var j = 0; j < cur_fields.length; j++) {
            if (all_fields.includes(cur_fields[j][0])) {
                continue;
            }
            all_fields.push(cur_fields[j][0]);
		}
    }
    return all_fields;
}

function getContactLabel(c) {
    if (!c) {
        return '';
    }
    if (!isNullOrUndefined(c.label)) {
        return c.label;
    }

    if (!isNullOrUndefined(c['FullName'])) {
        return c['FullName'];
    }
    return '????';
}

function getContactFields(c) {
    // fields is kvp array of field label/value
    let fields = [];
    if (!c) {
        return fields;
    }
    for (var i = 0; i < globals.ContactFieldTypes.length; i++) {
        let field_val = c[globals.ContactFieldTypes[i]];
        if (isNullOrUndefined(field_val)) {
            continue;
        }
        fields.push([globals.ContactFieldTypes[i],field_val]);
    }
    return fields;
}

function getContactTemplateSelField(ct) {
    if (!ct) {
        return '';
    }
    return ct.templateData;
}
function getContactTemplateLastSelGuid(ct) {
    if (!ct) {
        return '';
    }
    return ct.templateState;
}

function getContactTemplateSelContact(ct) {
    const contact_guid = getContactTemplateLastSelGuid(ct);
    return globals.AvailableContacts.find(x => x.guid == contact_guid);
}

function getContactOuterContainerElement() {
    return document.getElementById('pasteTemplateToolbarContactFieldSelectorContainer');
}

function getContactSelectorElement() {
    return document.getElementById('contactSelector');
}

function getContactFieldSelectorElement() {
    return document.getElementById('contactFieldSelector');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isContactTemplateHaveContact(ct) {
    if (!ct || isNullOrEmpty(ct.templateState) || ct.templateState == 'undefined') {
        return false;
    }
    return true;
}
function isContactTemplateHaveField(ct) {
    if (!ct || isNullOrEmpty(ct.templateData) || ct.templateData == 'undefined') {
        return false;
    }
    return true;
}
function isContactTemplateContactHaveField(ct, field) {
    const contact = getContactTemplateSelContact(ct);
    if (!contact) {
        return false;
    }
    return isNullOrUndefined(contact[field]) == false;
}

function isContactHaveField(contact_guid, field) {
    const contact = globals.AvailableContacts.find(x => x.guid == contact_guid);
    if (!contact) {
        return false;
    }
    return isNullOrUndefined(contact[field]) == false;
}
// #endregion State

// #region Actions

function createAllContactSelectOpts(ft) {
    createContactFieldSelectorOpts(ft);
    createContactSelectorOpts(ft);
}

function createContactFieldSelectorOpts(ft) {
    let contact_field_sel_elm = getContactFieldSelectorElement();
    contact_field_sel_elm.innerHTML = '';

    const fields = getAllContactFields();
    contact_field_sel_elm.id = "contactFieldSelector";
    for (var i = 0; i < fields.length; i++) {
        let field_opt_elm = document.createElement('option');
        if (isContactTemplateHaveContact(ft) && !isContactTemplateContactHaveField(ft, fields[i])) {
            field_opt_elm.classList.add('strike-through');
        } else {
            field_opt_elm.classList.remove('strike-through');
        }
        field_opt_elm.value = fields[i];
        field_opt_elm.innerText = toLabelCase(fields[i]);
        contact_field_sel_elm.appendChild(field_opt_elm);
    }

    contact_field_sel_elm.value = getContactTemplateSelField(ft);
}

function createContactSelectorOpts(ft) {
    const sel_contact_field_val = getContactFieldSelectorElement().value;
    let contact_sel_elm = getContactSelectorElement();
    contact_sel_elm.innerHTML = '';
    for (var i = 0; i < globals.AvailableContacts.length + 1; i++) {
        let contact_opt_elm = document.createElement('option');
        if (i == 0) {
            // append empty contact
            contact_sel_elm.appendChild(contact_opt_elm);
            continue;
        }
        let contact = globals.AvailableContacts[i - 1];
        if (isContactTemplateHaveField(ft) && !isContactHaveField(contact.guid, sel_contact_field_val)) {
            contact_opt_elm.classList.add('strike-through');
        } else {
            contact_opt_elm.classList.remove('strike-through');
        }
        contact_opt_elm.setAttribute('contactGuid', contact.guid);
        contact_opt_elm.value = contact.guid;
        contact_opt_elm.innerText = getContactLabel(contact);    
        contact_sel_elm.appendChild(contact_opt_elm);
    }
    contact_sel_elm.value = getContactTemplateLastSelGuid(ft);
}
function updateContactFieldSelectorToFocus(ft) {
    if (!ft || ft.templateType.toLowerCase() != 'contact') {
        getContactOuterContainerElement().classList.add('hidden');
        return;
    }
    getContactOuterContainerElement().classList.remove('hidden');

    if (globals.AvailableContacts.length == 0) {
        // NOTE/TODO contacts only fetched once...
        getContactsFromFetcherAsync_get()
            .then((result) => {
                if (Array.isArray(result)) {
                    globals.AvailableContacts = result;
                } else {
                    globals.AvailableContacts = [];
                }
                createAllContactSelectOpts(ft);
                updateContactTemplateToOptionChange();
            });
        return;
    }
    createAllContactSelectOpts(ft)
    updateContactTemplateToOptionChange();
}

function updateContactTemplateToOptionChange() {
    // NOTE always refresh all data regardless if its field or contact that changes

    let ft = getFocusTemplate();
    if (!ft || ft.templateType.toLowerCase() != 'contact') {
        return;
    }

    // SET TEMPLATE DATA
    let field_sel_elm = getContactFieldSelectorElement();
    if (!field_sel_elm) {
        return;
    }
    let field_val = field_sel_elm.value == null ? '' : field_sel_elm.value;
    field_val = field_val.replaceAll(' ', '');

    // SET TEMPLATE PASTE VALUE
    let contact_sel_elm = getContactSelectorElement();
    if (!contact_sel_elm) {
        return;
    }
    let new_ft_pv = '';
    let sel_contact_opt_elm =
        Array.from(contact_sel_elm.children)
            .find(x => x.value == contact_sel_elm.value);

    let contact_guid = '';
    if (sel_contact_opt_elm) {
        let sel_contact =
            globals.AvailableContacts
                .find(x => x.guid == sel_contact_opt_elm.getAttribute('contactGuid'));
        if (sel_contact) {
            contact_guid = sel_contact.guid;
            new_ft_pv = sel_contact[field_val];
            if (isNullOrUndefined(new_ft_pv)) {
                new_ft_pv = '';
            }
        }
    }

    setTemplateData(ft.templateGuid, field_val);
    setTemplateState(ft.templateGuid, contact_guid);
    setTemplatePasteValue(ft.templateGuid, new_ft_pv);
}
// #endregion Actions

// #region Event Handlers

function onContactFieldChanged(e) {
    updateContactTemplateToOptionChange();
}
function onSelectedContactFieldChanged(e) {
    updateContactTemplateToOptionChange();
}
// #endregion Event Handlers