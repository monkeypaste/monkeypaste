// #region Globals
var PendingGetResponses = [];
// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

async function getAllNonInputTemplatesFromDbAsync_get() {
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
        all_non_input_templates = await processGetRequestAsync(getAllNonInputTemplatesFromDb, JSON.stringify(req));
    } else {
        await delay(1000);
	}
    return all_non_input_templates;
}

async function getClipboardDataTransferObjectAsync_get() {
    // output 'MpQuillEditorClipboardDataObjectRequestNotification'
    let clipboard_dt = new DataTransfer();
    if (typeof getClipboardDataTransferObject === 'function') {
        let req = {
            //empty
        };
        let dt_json_obj = await processGetRequestAsync(getClipboardDataTransferObject, JSON.stringify(req));
        clipboard_dt = convertHostDataItemsToDataTransfer(dt_json_obj);
    } 
    return clipboard_dt;
}

async function getDragDataTransferObjectAsync_get(unprocessed_dt) {
    // output 'MpQuillEditorDragDataObjectRequestNotification'
    let processed_drag_dt = null;
    if (typeof getDragDataTransferObject === 'function') {
        let unprocessed_hdo = convertDataTransferToHostDataItems(unprocessed_dt);
        let req = {
            unprocessedDataItemsJsonStr: toBase64FromJsonObj(unprocessed_hdo)
        };
        let processed_hdo = await processGetRequestAsync(getDragDataTransferObject, JSON.stringify(req));
        processed_drag_dt = convertHostDataItemsToDataTransfer(processed_hdo);
    } 
    return processed_drag_dt;
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
    let respMsg = PendingGetResponses.find(x => x.requestGuid == reqGuid);
    if (!respMsg || respMsg.length == 0) {
        return null;
    }
    if (Array.isArray(respMsg)) {
        // this really shouldn't happen...
        log('Error, multiple get responses detected for request guid (both will be removed): ' + reqGuid);
        debugger;
        respMsg = respMsg[0];
    }
    // remove request from pending
    PendingGetResponses = PendingGetResponses.filter(x => x.requestGuid != reqGuid);

    // return respMsg and unblock processGet
    let resp = JSON.parse(respMsg.responseFragmentJsonStr);
    if (resp == null) {
        resp = '';
    }
    return resp;
}
// #endregion State

// #region Actions

async function processGetRequestAsync(func, reqMsgStr) {
    // output 'MpQuillGetRequestNotification'

    let reqObj = {
        requestGuid: generateGuid(),
        reqMsgFragmentJsonStr: reqMsgStr
    };

    let req = toBase64FromJsonObj(reqObj);
    func(req);

    while (true) {
        let resp = receiveGetResponse(reqObj.requestGuid);
        if (resp) {
            // input see calling func
            return resp;
        }
        await delay(100);
	}
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers