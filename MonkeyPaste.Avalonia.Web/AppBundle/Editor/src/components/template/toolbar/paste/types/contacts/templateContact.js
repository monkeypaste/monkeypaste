// #region Globals

var AvailableContacts = [];

const ContactFieldTypes = [
    'FirstName',
    'LastName',
    'FullName',
    'PhoneNumber',
    'Address',
    'Email',
]

const CONTACT_TEMPLATE_DATA_SEP_TOKEN = '!&^';


// #endregion Globals

// #region Life Cycle

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
    for (var i = 0; i < AvailableContacts.length; i++) {
        let cur_fields = getContactFields(AvailableContacts[i]);
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
    for (var i = 0; i < ContactFieldTypes.length; i++) {
        let field_val = c[ContactFieldTypes[i]];
        if (isNullOrUndefined(field_val)) {
            continue;
        }
        fields.push([ContactFieldTypes[i],field_val]);
    }
    return fields;
}

function getContactTemplateSelField(ct) {
    if (!ct) {
        return '';
    }
    let field_val = ct.templateData; 
    if (isNullOrEmpty(field_val)) {
        field_val = '';
    }
    return field_val.split(CONTACT_TEMPLATE_DATA_SEP_TOKEN)[0];
}
function getContactTemplateLastSelGuid(ct) {
    if (!ct) {
        return '';
    }
    let field_val = ct.templateData; 
    if (isNullOrEmpty(field_val)) {
        field_val = '';
    }
    let data_parts = field_val.split(CONTACT_TEMPLATE_DATA_SEP_TOKEN);
    if (data_parts.length < 2) {
        return '';
    }
    return data_parts[1];
}

function getContactEncodedTemplateData(field, contact_guid) {
    return `${field}${CONTACT_TEMPLATE_DATA_SEP_TOKEN}${contact_guid}`;
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

// #endregion State

// #region Actions

function createContactFieldSelector(ft) {

    let contacts_outer_container_elm = getContactOuterContainerElement();
    contacts_outer_container_elm.innerHTML = '';

    const fields = getAllContactFields();
    let contact_field_sel_elm = document.createElement('select');
    contact_field_sel_elm.id = "contactFieldSelector";
    for (var i = 0; i < fields.length; i++) {
        let field_opt_elm = document.createElement('option');
        field_opt_elm.value = fields[i];
        field_opt_elm.innerText = toLabelCase(fields[i]);
        contact_field_sel_elm.appendChild(field_opt_elm);
    }

    contacts_outer_container_elm.appendChild(contact_field_sel_elm);
    contact_field_sel_elm.addEventListener('change', onContactFieldChanged);

    let contact_sel_elm = createContactSelector();
    contacts_outer_container_elm.appendChild(contact_sel_elm);
    contact_sel_elm.addEventListener('change', onSelectedContactFieldChanged);

    contact_field_sel_elm.value = getContactTemplateSelField(ft);
    contact_sel_elm.value = getContactTemplateLastSelGuid(ft);
}

function createContactSelector() {
    let contact_sel_elm = document.createElement('select');
    contact_sel_elm.id = "contactSelector";
    for (var i = 0; i < AvailableContacts.length + 1; i++) {
        let contact_opt_elm = document.createElement('option');
        if (i == 0) {
            // append empty contact
            contact_sel_elm.appendChild(contact_opt_elm);
            continue;
        }
        let contact = AvailableContacts[i-1];
        contact_opt_elm.setAttribute('contactGuid', contact.guid);
        contact_opt_elm.value = contact.guid;
        contact_opt_elm.innerText = getContactLabel(contact);    
        contact_sel_elm.appendChild(contact_opt_elm);
    }
    return contact_sel_elm;
}
function updateContactFieldSelectorToFocus(ft) {
    if (!ft || ft.templateType.toLowerCase() != 'contact') {
        getContactOuterContainerElement().classList.add('hidden');
        return;
    }
    getContactOuterContainerElement().classList.remove('hidden');

    if (AvailableContacts.length == 0) {
        // NOTE/TODO contacts only fetched once...
        getContactsFromFetcherAsync_get()
            .then((result) => {
                if (Array.isArray(result)) {
                    AvailableContacts = result;
                } else {
                    AvailableContacts = [];
                }

                createContactFieldSelector(ft);
                updateContactTemplateToOptionChange();
            });
        return;
    }
    createContactFieldSelector(ft);
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
    let sel_contact_opt_elm = Array.from(contact_sel_elm.children).find(x => x.value == contact_sel_elm.value);

    let contact_guid = '';
    if (sel_contact_opt_elm) {
        let sel_contact = AvailableContacts.find(x => x.guid == sel_contact_opt_elm.getAttribute('contactGuid'));
        if (sel_contact) {
            contact_guid = sel_contact.guid;
            new_ft_pv = sel_contact[field_val];
            if (isNullOrUndefined(new_ft_pv)) {
                new_ft_pv = '';
            }
        }
    }

    let new_ft_data = getContactEncodedTemplateData(field_val, contact_guid);
    setTemplateData(ft.templateGuid, new_ft_data);
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