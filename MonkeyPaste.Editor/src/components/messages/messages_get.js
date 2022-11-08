// #region Globals
var PendingGetResponses = [];
// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters
async function getAllNonInputTemplatesFromDb_get() {
    // output 'MpQuillNonInputTemplateRequestMessage'
    let all_non_input_templates = [];
    if (typeof getAllNonInputTemplatesFromDb === 'function') {
        let req = {
            templateTypes: [
                'Static',
                'Contact',
                'DateTime'
            ]
        };
        let resp = await processGetRequestAsync(getAllNonInputTemplatesFromDb, toBase64FromJsonObj(req));
        all_non_input_templates = toJsonObjFromBase64Str(resp);
    }
    return all_non_input_templates;
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
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function receiveGetResponse(reqGuid) {
    // check if any response has matching request guid
    let resp = PendingGetResponses.find(x => x.requestGuid == reqGuid);
    if (!resp || resp.length == 0) {
        return null;
    }
    if (Array.isArray(resp)) {
        // this really shouldn't happen...
        log('Error, multiple get responses detected for request guid (both will be removed): ' + reqGuid);
        debugger;
        resp = resp[0];
    }
    // remove request from pending
    PendingGetResponses = PendingGetResponses.filter(x => x.requestGuid != reqGuid);

    // return resp and unblock processGet
    return resp;
}
// #endregion State

// #region Actions

async function processGetRequestAsync(func, reqMsgBase64Str) {
    // output 'MpQuillGetRequestNotification'

    let reqObj = {
        requestGuid: generateGuid(),
        reqMsgFragmentBase64JsonStrreqArg: reqMsgBase64Str
    };

    let req = toBase64FromJsonObj(reqObj);
    func(req);

    while (true) {
        let esp = receiveGetResponse(reqObj.requestGuid);
        if (resp) {
            // input see calling func
            let resp_data_obj = toJsonObjFromBase64Str(resp.responseFragmentBase64JsonStr);
            return resp_data_obj;
        }
        await delay(100);
	}
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers