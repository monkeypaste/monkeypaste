//#endregion

//#region DOM Traversal

function toBase64FromJsonObj(obj) {
    let objStr = null
    if (typeof obj === 'string' || obj instanceof String) {
        objStr = obj;
    } else {
        objStr = JSON.stringify(obj);
	}
    let base64 = btoa(objStr);
    return base64;
}

function toJsonObjFromBase64Str(base64Str) {
    if (typeof base64Str === 'string' || base64Str instanceof String) {
        let jsonStr = atob(base64Str);
        let jsonObj = JSON.parse(jsonStr);
        return jsonObj;
    }
    return null;
}

function changeInnerText(elm, text, newText) {
    if (elm == null) {
        return;
    }

    function changeInnerTextHelper(elm, text, newText) {
        if (elm == null) {
            return;
        }
        if (elm.nodeType == 3 && elm.data == text) {
            elm.data = newText;
            return;
        }
        changeInnerTextHelper(elm.firstChild, text, newText);
        changeInnerTextHelper(elm.nextSibling, text, newText);
    }

    changeInnerTextHelper(elm.firstChild, text, newText);
}

function substringByLength(str, sIdx, length) {
    // js subsring is by sidx,eidx
    // cs substring is by sidx,length 
    // this mimics cs for ported code, etc.
    let eIdx = sIdx + length;
    return str.substring(sIdx, eIdx);
}

function getAllElements(elm) {
    let allElm = [];
    if (!elm) {
        return allElm;
    }

    allElm.push(elm);
    function getAllElementsHelper(elm, elml) {

    }

    letgetAllElements(elm.nextSibling, allElm);
}

function getElementsFromAndUntil(fromElm, untilElm) {
    let elms = [];
    getElementsFromAndUntilHelper(fromElm, untilElm, elms);
    let filteredArr = [];
    elms.forEach((item) => {
        if (!filteredArr.includes(item)) {
            filteredArr.push(item);
        }
    })

    return filteredArr;
}
function getElementsFromAndUntilHelper(curElm, untilElm, elms) {
    if (curElm == null) {
        return;
    }
    elms.push(curElm);
    if (curElm == untilElm) {
        return;
    }

    let celms = [];
    getElementsFromAndUntilHelper(curElm.firstChild, untilElm, celms);

    let nelms = [];
    nelms = getElementsFromAndUntilHelper(curElm.nextSibling, untilElm, nelms);

    if (celms) {
        elms.push(...celms);
    }
    if (nelms) {
        elms.push(...nelms);
    }
}

//#endregion

//function log(msg) {
//    if (!isLoggingEnabled) {
//        return;
//    }
//    console.log(msg);
//}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function clone(obj) {
    if (obj == null) {
        return obj
    }

    let cloneObj = JSON.parse(JSON.stringify(obj));
    return cloneObj;
}

function getRandomInt(max = Number.MAX_SAFE_INTEGER) {
    return Math.floor(Math.random() * max);
}

function isNullOrEmpty(str) {
    return str == null || str == '';
}

function isNullOrWhiteSpace(str) {
    return str == null || str.every(x => x == ' ');
}

function overrideEvent(elm, eventName, handler) {
    let overrideEventName = "mp_" + eventName;

    window.addEventListener(eventName, function (event) {
        // (note: not cross-browser)
        var event2 = new CustomEvent(overrideEventName, { detail: { original: event } });
        event.target.dispatchEvent(event2);
        event.stopPropagation();
    }, true);

    elm.addEventListener(overrideEventName, handler);
}

function getAllElementsBetween(fromElm, toElm, inclusive = { from: true, to: true }) {
    let allElms = [];
    if (fromElm == null || toElm == null) {
        return allElms;
    }


    while (true) {

    }
}

function getAllElementsBetweenHelper(elm, elms) {
    if (elm == null) {
        return elms;
    }
    elms.push(elm);
    getAllElementsBetweenHelper(elm.firstChild, elms);
    getAllElementsBetweenHelper(elm.nextSibling, elms);
}




function isEmptyOrSpaces(str) {
    return str === null || str.match(/^ *$/) !== null;
}

function generateGuid() {
    var d = new Date().getTime();//Timestamp
    var d2 = (performance && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16;//random number between 0 and 16
        if (d > 0) {//Use timestamp until depleted
            r = (d + r) % 16 | 0;
            d = Math.floor(d / 16);
        } else {//Use microseconds since page-load if supported
            r = (d2 + r) % 16 | 0;
            d2 = Math.floor(d2 / 16);
        }
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}

function parseBool(text) {
    return text == 'true';
}

function hasJsonStructure(str) {
    if (typeof str !== 'string') return false;
    try {
        const result = JSON.parse(str);
        const type = Object.prototype.toString.call(result);
        return type === '[object Object]'
            || type === '[object Array]';
    } catch (err) {
        return false;
    }
}

function isTextElement(elm) {
    return elm.nodeType === 3;
}


const delay = time => new Promise(res => setTimeout(res, time));

