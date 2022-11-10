var AvailableContacts = [];
var SelectedContactGuid = null;

function initContactTestData() {
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
        dummy_contacts[i]['Full Name'] = dummy_contacts[i].label;
    }
    AvailableContacts = dummy_contacts;
}

function loadContacts(filterField) {
	// TODO should happen on contact template focus during paste

    SelectedContactIdx = null;
    
    let contacts = getContacts_get(filterField);
    return contacts;
}

function getContactFieldList() {
	// TODO this should return json data for
	// drop down in edit template for contact

	let contact_fields = getContactFieldList_get();
	return contact_fields;
}

function getContactFieldValue(contact_guid, fieldName) {
	let contact_field_value = getContactFieldValue_get(contact_guid, fieldName)
	return contact_field_value;
}


