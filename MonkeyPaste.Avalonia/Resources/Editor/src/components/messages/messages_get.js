// #region Life Cycle

// #endregion Life Cycle

// #region Getters

async function getAllSharedTemplatesFromDbAsync_get() {
    // output 'MpQuillTemplateDbQueryRequestMessage'
    let all_non_input_templates = [];
    if (isRunningOnHost()) {
        let req = {
            templateTypes: [
                'Static',
                'Contact',
                'DateTime'
            ]
        };
        all_non_input_templates = await processGetRequestAsync('getAllSharedTemplatesFromDb', JSON.stringify(req));
    } else {

        await delay(1000);
    }
    return all_non_input_templates;
}

async function getAppPasteInfoFromDbAsync_get() {
    // output 'MpQuillPasteInfoRequestMessage'
    let info_items = [];
    // NOTE this flag is used so when this editor receives new paste info msg
    // (like when active app changes) it will stay locked to the request and
    // coordinate using the infoId

    startPasteInfoQueryRequest();
    if (isRunningOnHost()) {
        // NOTE passing cur infoId since this is async to help
        // keep editor w/ actual active process info
        let req = {
            infoId: globals.CurPasteInfoId
        };
        info_items = await processGetRequestAsync('getAppPasteInfoFromDb', JSON.stringify(req));
    } else {

        await delay(1000);
    }
    return info_items;
}

async function getMessageBoxResultAsync_get(_title,_msg,_dialogType,_iconResourceObj) {
    // output 'MpQuillShowDialogRequestMessage'
    // input 'MpQuillShowDialogResponseMessage'
    let req = {
        title: _title,
        msg: _msg,
        dialogType: _dialogType,
        iconResourceObj: _iconResourceObj
    };
    let diag_resp_obj = await processGetRequestAsync('getMessageBoxResult', JSON.stringify(req));
    return diag_resp_obj.dialogResponse;
}

async function getClipboardDataTransferObjectAsync_get() {
    // output 'MpQuillEditorClipboardDataObjectRequestNotification'
    let clipboard_dt = new DataTransfer();
    let req = {
        //empty
    };
    let dt_json_obj = await processGetRequestAsync('getClipboardDataTransferObject', JSON.stringify(req));
    clipboard_dt = convertHostDataItemsToDataTransfer(dt_json_obj); 
    return clipboard_dt;
}

async function getDragDataTransferObjectAsync_get(unprocessed_dt) {
    // output 'MpQuillEditorDragDataObjectRequestNotification'
    let processed_drag_dt = null;
    let unprocessed_hdo = convertDataTransferToHostDataItems(unprocessed_dt);
    let req = {
        unprocessedDataItemsJsonStr: toBase64FromJsonObj(unprocessed_hdo)
    };
    let processed_hdo = await processGetRequestAsync('getDragDataTransferObject', JSON.stringify(req));
    processed_drag_dt = convertHostDataItemsToDataTransfer(processed_hdo);
    return processed_drag_dt;
}
async function getContactsFromFetcherAsync_get() {
    // output 'MpIContact[]'
    let all_contacts = [];
    if (isRunningOnHost()) {
        all_contacts = await processGetRequestAsync('getContactsFromFetcher', '');
    } else {
        await delay(1000);
        all_contacts = getContactTestData();
    }
    return all_contacts;
}

function getContactFieldList_get() {
    // TODO pull these in w/ actual contact stuff
    if (globals.IsTesting) {
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
    if (globals.IsTesting) {
        if (!globals.AvailableContacts ||
            globals.AvailableContacts.length == 0 ||
            isNullOrWhiteSpace(contact_guid) ||
            isNullOrWhiteSpace(field)) {
            return null;
        }
        for (var i = 0; i < globals.AvailableContacts.length; i++) {
            let contact = globals.AvailableContacts[i];
            if (contact.guid == contact_guid) {
                return contact[field];
            }
        }
        return null;
    }
    // globals.AvailableContacts shouldn't have any data besides label and guid
    // so host should get this data
    return null;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function receiveGetResponse(reqGuid) {
    // check if any response has matching request guid
    let respMsg = globals.PendingGetResponses.find(x => x.requestGuid == reqGuid);
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
    globals.PendingGetResponses = globals.PendingGetResponses.filter(x => x.requestGuid != reqGuid);

    // return respMsg and unblock processGet
    let resp = JSON.parse(respMsg.responseFragmentJsonStr);
    if (resp == null) {
        resp = '';
    }
    return resp;
}
// #endregion State

// #region Actions

async function processGetRequestAsync(fn, reqMsgStr) {
    // output 'MpQuillGetRequestNotification'

    let reqObj = {
        requestGuid: generateGuid(),
        reqMsgFragmentJsonStr: reqMsgStr
    };

    let req = toBase64FromJsonObj(reqObj);
    sendMessage(fn, req);

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