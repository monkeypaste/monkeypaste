// these functions wrap window bindings that return data from host

function getAllTemplatesFromDb_get() {
    let all_templates = [];
    if (typeof getAllTemplatesFromDb === 'function') {
        let allTemplatesJsonStr = getAllTemplatesFromDb();
        log('templates from db:');
        log(allTemplatesJsonStr);
        all_templates = JSON.parse(allTemplatesJsonStr);
    }
    return all_templates;
}

function getContacts_get(filterField) {
	// TODO should return contacts with available field from binding property
    if (IsTesting) {
        if (AvailableContacts.length == 0) {
            initContactTestData();
        }
        return AvailableContacts();
    }
    return [];
}

function getContactFieldList_get() {
    // TODO pull these in w/ actual contact stuff
    if (IsTesting) {
        let contact_fields = [
            "First Name",
            "Last Name",
            "Full Name",
            "Phone Number",
            "Address",
            // TODO add rest from wherever i put it
        ];
        return contact_fields;
    }
    return [];
}

function getContactFieldValue_get(contact_guid, field) {
    if (IsTesting) {
        if (!AvailableContacts ||
            AvailableContacts.length == 0 ||
            isNullOrWhiteSpace(contact_guid) ||
            isNullOrWhiteSpace(field)) {
            return null;
        }
        for (var i = 0; i < AvailableContacts.length; i++) {
            let contact = AvailableContacts[i];
            if (contact.guid == contact_guid) {
                return contact[field];
            }
        }
        return null;
    } 
    // AvailableContacts shouldn't have any data besides label and guid
    // so host should get this data
    return null;
}