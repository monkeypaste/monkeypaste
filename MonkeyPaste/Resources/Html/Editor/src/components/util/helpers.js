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

function envNewLine() {
    // Windows = CR LF
    // Linux = LF
    // MAC < 0SX = CR
    // MAC >= OSX = LF
    if (EnvName == WindowsEnv) {
        return '\r\n';
    }
    return '\n';
}

function isValidHttpUrl(text) {
    let url;

    try {
        url = new URL(text);
    } catch (_) {
        return false;
    }

    return url.protocol === "http:" || url.protocol === "https:";
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
    if (!length) {
        length = str.length - sIdx;
	}
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

function isChildOfElement(elm, parent) {
    if (!elm || !parent) {
        return false;
    }
    let elm_parent = elm.parentNode;
    while (elm_parent != null) {
        if (elm_parent == parent) {
            return true;
        }
        elm_parent = elm_parent.parentNode;
    }
    return false;
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
function isInt(n) {
    return n % 1 === 0;
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
//function htmlCollectionMove(td, fromIndex, toIndex) {
//    var before = td.children[curr_index + direction];
//    var child = td.children[curr_index];

//    td.removeChild(child);
//    td.insertBefore(child, before); //attempt to insert it   
//}
function unescapeHtml(htmlStr) {
    //return htmlStr.replace(/&lt;/g, "<")
    //    .replace(/&gt;/g, ">")
    //    .replace(/&quot;/g, '"')
    //    .replace(/&amp;/g, "&");

    //const doc = DomParser.parseFromString(htmlStr, "text/html");
    //return doc.documentElement.textContent;

    const e = document.createElement('textarea');
    e.innerHTML = htmlStr;
    return e.childNodes.length === 0 ? "" : e.childNodes[0].nodeValue;
}

function escapeHtml(htmlStr) {
    const htmlEntities = {
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&apos;"
    };
    return htmlStr.replace(/([&<>\"'])/g, match => htmlEntities[match]);
}
const delay = time => new Promise(res => setTimeout(res, time));

async function getBase64ScreenshotOfElementAsync(element) {
    // from https://stackoverflow.com/a/41585230/105028
    let base64Str = '';
    html2canvas(element).then(function (canvas9) {
        var theimage9 = canvas9.toDataURL("image/png");
        base64Str = theimage9.replace("data:image/png;base64,", "");
        //document.querySelector("#theimage9").src = theimage9;
    });

    while (base64Str == '') {
        await delay(100);
    }
    return base64Str;
}


function getBase64ScreenshotOfElement(element) {
    // from https://stackoverflow.com/a/41585230/105028
    let base64Str = getBase64ScreenshotOfElementAsync(element);
    //html2canvas(element).then(function (canvas9) {
    //    var theimage9 = canvas9.toDataURL("image/png");
    //    base64Str = theimage9.replace("data:image/png;base64,", "");
    //    //document.querySelector("#theimage9").src = theimage9;
    //});

    return base64Str;
}

