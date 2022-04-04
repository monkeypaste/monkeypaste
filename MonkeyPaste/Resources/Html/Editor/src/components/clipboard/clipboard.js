var LastCutDocRange = { index: 0, length: 0 };
var LastCutOrCopyUpdatedHtml = '';

var PasteNode;

function initClipboard() {
    const Delta = Quill.imports.delta;
    const clipboardItems = [];

    quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        PasteNode = retargetContentItemDomNode(node);

        return delta;
    });


    //document.addEventListener("cut", onCut);
    //document.addEventListener("copy", onCopy);

    //document.addEventListener('paste', async (e) => {
        // Cases
        // 1. Cb data was cut or copied from THIS document 
        //    - this is known if the clipboard html data equals Last var and can be pasted as is
        // 2. Cb data was copied externally and only has text (NOTE 1)
        // 3. Cb data was copied externally and has html (NOTE 1)
        // 4. Cb data was copied from another tile within app

        // Notes
        // 1. When data is brought in externally then clipboard watcher will add it to db
        //    so in enableReadOnly back in c# new guids should be cross-checked by data
        //    if found in db use its source info when creating it (may be problematic)

    //    let cb_html_str = await getClipboardHtml();
    //    if (cb_html_str && !cb_html_str.includes('copyItemGuid')) {
    //        // case 3
    //        e.preventDefault();

    //        updateHtmlClipboardData(e, cb_html_str);
    //    }
    //});
}

async function onCut(e) {
    // Cases
    // 1. A selection is cut
    //      -If remaining document does NOT contain a cut contentGuid then 
    //       full item was removed so do not alter its contentBlot
    //          

    // NOTE Cut event is triggered before quill.selectionChanged

    LastCutDocRange = quill.getSelection(); //used to check if paste is in same place

    //let cb_html_str = await getClipboardHtml();
    let cb_html_str = SelectedHtml;
    if (cb_html_str) {
        e.preventDefault();

        LastCutOrCopyUpdatedHtml = updateHtmlClipboardData(e, cb_html_str);
    }
}

async function onCopy(e) {
    // Cases
    // 1. A selection is copied
    //      -Since content will be duplicated if pasted always give new guid
    LastCutDocRange = null; // will let paste handler know was not a cut op

    //let cb_html_str = await getClipboardHtml();
    let cb_html_str = SelectedHtml;
    if (cb_html_str) {
        e.preventDefault();

        LastCutOrCopyUpdatedHtml = updateHtmlClipboardData(e, cb_html_str);
    }
}

async function getClipboardHtml() {
    let data = await getClipboardDataByType("text/html");
    return data;
}

async function getClipboardText() {
    let data = await getClipboardDataByType("text/plain");
    return data;
}

async function getClipboardDataByType(dataType) {
    try {
        const clipboardItems = await navigator.clipboard.read();
        for (const clipboardItem of clipboardItems) {
            for (const type of clipboardItem.types) {
                if (type != dataType) {
                    continue;
                }
                const blob = await clipboardItem.getType(type);
                let data = await blob.text();
                return data;
            }
        }
    } catch (err) {
        log(err.name, err.message);
    }
    return null;
}

function updateHtmlClipboardData(e, htmlDataStr) {
    log('Paste Input: ');
    log(htmlDataStr);

    let newContentGuid = generateGuid();
    try {
        let cb_html = domParser.parseFromString(htmlDataStr, 'text/html');
        let cb_elms = cb_html.querySelectorAll('*');

        Array.from(cb_elms).forEach(elm => {
            retargetContentItemDomNode(elm, newContentGuid);
        });
        let cb_elms_str = cb_elms.toString();

        log('Paste Output:');
        log(cb_elms_str);
        e.clipboardData.setData('text/html', cb_elms_str);

        return cb_elms_str;
    } catch (err) {
        log(err.name, err.message);
    }
    return null;
}