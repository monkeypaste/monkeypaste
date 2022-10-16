
function initBouncyTextArea(elmId) {
    document.getElementById(elmId).addEventListener('focus', onBouncyTextAreaFocus);
    document.getElementById(elmId).addEventListener('blur', onBouncyTextAreaBlur);
    document.getElementById(elmId).addEventListener('keydown', onBouncyTextAreaKeyUp);
    document.getElementById(elmId).addEventListener('keyup', onBouncyTextAreaKeyDown);
}

function onBouncyTextAreaFocus(e) {
    //jiggleFocusTemplates();
}

function onBouncyTextAreaBlur(e) {
    //jiggleFocusTemplates(true);
}

async function onBouncyTextAreaKeyDown(e) {
    await scaleFocusTemplates('bigger');
}

async function onBouncyTextAreaKeyUp(e) {
    await scaleFocusTemplates('default');
}

async function scaleFocusTemplates(scaleType, tguid) {
    if (!tguid) {
        tguid = getFocusTemplateGuid();
        if (!tguid) {
            return;
        }
    }

    let f_cit_elm_l = getTemplateElements(tguid);
    for (var i = 0; i < f_cit_elm_l.length; i++) {
        let f_cit_elm = f_cit_elm_l[i];
        if (scaleType == 'bigger') {
            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-up');
            f_cit_elm.classList.add('ql-template-embed-blot-display-key-down');
        } else if (scaleType == 'default') {
            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-down');
            f_cit_elm.classList.add('ql-template-embed-blot-display-key-up');
        } else {

            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-down');
            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-up');
        }
    }
}


function clearAllTemplateEditClasses() {
    getTemplateElements().forEach((telm) => {
        telm.classList.remove('ql-template-embed-blot-display-key-up');
        telm.classList.remove('ql-template-embed-blot-display-key-down');
    });
}

// unused
async function jiggleFocusTemplates(resetOnComplete = false) {
    return;
    //let f_cit = getFocusTemplateElement();
    let tguid = getFocusTemplateGuid();
    if (!tguid) {
        return;
    }
    let scale_ms = 100;
    for (var i = 0; i < 2; i++) {
        await scaleFocusTemplates('bigger', tguid);
        await delay(scale_ms);
        await scaleFocusTemplates('default', tguid);
        await delay(scale_ms);
    }
    if (resetOnComplete) {
        scaleFocusTemplates('reset', tguid);
    }
}