function getOS() {
    var userAgent = window.navigator.userAgent,
        platform = window.navigator?.userAgentData?.platform || window.navigator.platform,
        macosPlatforms = ['Macintosh', 'MacIntel', 'MacPPC', 'Mac68K'],
        windowsPlatforms = ['Win32', 'Win64', 'Windows', 'WinCE'],
        iosPlatforms = ['iPhone', 'iPad', 'iPod'],
        os = null;

    if (macosPlatforms.indexOf(platform) !== -1) {
        os = 'Mac OS';
    } else if (iosPlatforms.indexOf(platform) !== -1) {
        os = 'iOS';
    } else if (windowsPlatforms.indexOf(platform) !== -1) {
        os = 'Windows';
    } else if (/Android/.test(userAgent)) {
        os = 'Android';
    } else if (/Linux/.test(platform)) {
        os = 'Linux';
    }

    return os;
}

function distinct(arr) {
    if (isNullOrUndefined(arr)) {
        return [];
    }
    arr = arr.filter((value, index) => {
        return index === arr.findIndex((obj) => {
            return JSON.stringify(obj) === JSON.stringify(value);
        })
    });
    return arr;
}

function removeAllChildren(elm) {
    if (!elm) {
        return;
    }
    while (elm.hasChildNodes()) {
        elm.removeChild(elm.lastChild)
    }
}

function isString(obj) {
    if (isNullOrUndefined(obj)) {
        return false;
    }
    if (typeof obj === 'string' || obj instanceof String) {
        return true;
    }
    return false;
}

function isNullOrUndefined(obj) {
    return obj === undefined || obj == null;
}

function splitByNewLine(str) {
    if (isNullOrEmpty(str)) {
        return [];
    }
    return str.split(/\r?\n/);
}

function indexOfAll(pt, search_text, case_sensitive = true) {
    let indexes = [];

    pt = case_sensitive ? pt : pt.toLowerCase();
    search_text = case_sensitive ? search_text : search_text.toLowerCase();

    let cur_idx = pt.indexOf(search_text);
    while (cur_idx >= 0) {
        let cur_pt = substringByLength(pt, cur_idx + search_text.length);
        let rel_idx = cur_pt.indexOf(search_text);
        if (rel_idx < 0) {
            break;
        }
        cur_idx = pt.length - cur_pt.length + rel_idx;
        indexes.push(cur_idx);
    }
    return indexes;
}

function findRanges(pt, search_text, case_sensitive) {
    let indexes = indexOfAll(pt, search_text, case_sensitive);
    let ranges = [];
    for (var i = 0; i < indexes.length; i++) {
        ranges.push({ index: indexes[i], length: search_text.length });
    }
    return ranges;
}

function queryText(pt, search_text, case_sensitive, whole_word, use_regex) {
    let regex = null;
    let flags = 'g';
    if (!case_sensitive) {
        flags += 'i';
	}

    if (use_regex) {
        regex = new RegExp(search_text, flags);
    } else {
        let word_str = whole_word ? '\\b' : '';
        regex = new RegExp(`${word_str}${search_text}${word_str}`, flags);
    }
    let match_ranges = [];
    var result;
    while (result = regex.exec(pt)) {
        let cur_range = {
            index: result.index,
            length: result[0].length,
            text: result[0]
        };
        match_ranges.push(cur_range);
    }
    return match_ranges;
}

function parseInt_safe(obj) {
    if (!obj) {
        return 0;
    }
    let result = parseInt(obj);
    if (isNaN(result)) {
        return 0;
    }
    return result;
}

function utf8_to_b64(str) {
    return window.btoa(unescape(encodeURIComponent(str)));
}

function b64_to_utf8(str) {
    return decodeURIComponent(escape(window.atob(str)));
}
function toBase64FromJsonObj(obj) {
    let objStr = null
    if (typeof obj === 'string' || obj instanceof String) {
        objStr = obj;
    } else {
        objStr = JSON.stringify(obj);
	}
    let base64 = utf8_to_b64(objStr);
    return base64;
}

function toJsonObjFromBase64Str(base64Str) {
    if (typeof base64Str === 'string' || base64Str instanceof String) {
        let jsonStr = b64_to_utf8(base64Str);
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


function isValidEmalAddress(text) {
    return /^\S+@\S+\.\S+$/.test(text) && text.indexOf('mailto:') !== 0;
}

function isValidFileSystemPath(text) {
    // 
    if (!text.startsWith('file:')) {
        return false;
    }
    return true;
}

function isValidUri(text) {
    return isValidHttpUrl(text) || isValidEmalAddress(text) || isValidFileSystemPath(text);
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
    if (length == 0) {
        return '';
    }
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

function getAncestorElements(elm, include_self = true) {
    let elms = [];
    if (!elm) {
        return elms;
    }
    let cur_elm = include_self ? elm : elm.parentNode;
    while (cur_elm != null) {
        elms.push(cur_elm);
        cur_elm = cur_elm.parentNode;
    }
    return elms;
}
//#endregion

//function log(msg) {
//    if (!isLoggingEnabled) {
//        return;
//    }
//    console.log(msg);
//}
function clamp(num, min, max) {
    return Math.min(Math.max(num, min), max);
}

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
    return str == null || str == '' || str === undefined;
}

function isNullOrWhiteSpace(str) {
    return isNullOrEmpty(str) || str.trim().length == 0;
}

function isChildOfElement(elm, parent, include_self = true) {
    if (!elm || !parent) {
        return false;
    }
    if (include_self && elm == parent) {
        return true;
    }
    return parent.contains(elm);
}
function isChildOfTagName(elm, tagName, include_self = true) {
    if (!elm || isNullOrWhiteSpace(tagName)) {
        return false;
    }
    let search_elm = include_self ? elm : elm.parentNode;
    while (true) {
        if (search_elm == null) {
            return false;
        }
        if (search_elm.tagName === undefined) {
            if (search_elm.parentNode == null) {
                return false;
            }
            search_elm = search_elm.parentNode;
        }
        if (search_elm == null || search_elm.tagName === undefined) {
            debugger;
        }
        if (search_elm.tagName.toLowerCase() == tagName.toLowerCase()) {
            return true;
        }
        search_elm = search_elm.parentNode;
    }
    return false;
}

function isClassInElementPath(elm, classOrClasses, compareOp = 'OR') {
    if (!elm || !classOrClasses) {
        return false
	}
    let classes = [];
    if (typeof classOrClasses === 'string' || classOrClasses instanceof String) {
        classes = classOrClasses.split(' ');
    } else if (Array.isArray(classOrClasses)) {
        classes = classOrClasses;
    } else {
        // what type is it?
        debugger;
        return false;
    }

    let search_elm = elm;
    while (search_elm != null) {
        let was_and_valid = compareOp == 'AND';
        for (var i = 0; i < classes.length; i++) {
            if (search_elm.classList === undefined) {
                continue;
			}
            if (compareOp == 'OR') {
                if (search_elm.classList.contains(classes[i])) {
                    return true;
				}
            } else if (compareOp == 'AND') {
                if (!search_elm.classList.contains(classes[i])) {
                    was_and_valid = false;
                }
            } else {
                // whats the op?
                debugger;
                return false;
			}
        }
        if (was_and_valid) {
            return true;
		}
        search_elm = search_elm.parentNode;
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
const HtmlEntitiesLookup = [
    [`&`, `&amp;`],
    [` `, `&nbsp;`],
    [`\"`, `&quot;`],
    [`\'`, `&apos;`],
    [`>`, `&gt;`],
    [`¢`, `&cent;`],
    [`£`, `&pound;`],
    [`¥`, `&yen;`],
    [`€`, `&euro;`],
    [`©`, `&copy;`],
    [`®`, `&reg;`],
    [`™`, `&trade;`],
    [`<`, `&lt;`]
];

function isStringContainSpecialHtmlEntities(str) {
    if (isNullOrEmpty(str)) {
        return false;
    }
    //return Object.entries(HtmlEntitiesLookup).find(([k, v]) => str.includes(k)) != null;
    return HtmlEntitiesLookup.find(x => str.includes(x[0])) != null;
}
function encodeHtmlSpecialEntities(str) {
    if (!isString(str) || isNullOrEmpty(str)) {
        return '';
    }
    for (var i = 0; i < HtmlEntitiesLookup.length; i++) {
        if (i == 0) {
            // special case for & to avoid double encoding
            str = str.replaceAll(/&(?!(#[0-9]{2,4}|[A-z]{2,6});)/g, HtmlEntitiesLookup[i][1]);
            continue;
        }
        str = str.replaceAll(HtmlEntitiesLookup[i][0], HtmlEntitiesLookup[i][1]);
    }
    return str;
}

function decodeHtmlSpecialEntities(str) {
    if (!isString(str) || isNullOrEmpty(str)) {
        return '';
    }
    for (var i = 0; i < HtmlEntitiesLookup.length; i++) {
        str = str.replaceAll(HtmlEntitiesLookup[i][1], HtmlEntitiesLookup[i][0]);
    }
    return str;
}

function getAllTextElementsInElement(elm) {
    var text_elms = [];
    if (!elm) {
        return text_elms;
    }
    for (elm = elm.firstChild; elm; elm = elm.nextSibling) {
        if (elm.nodeType == 3) {
            text_elms.push(elm);
        }
        else text_elms = text_elms.concat(getAllTextElementsInElement(elm));
    }
    return text_elms;
}

const delay = time => new Promise(res => setTimeout(res, time));

function getElementComputedStyleProp(elm, propName) {
    if (isNullOrUndefined(elm) ||
        isNullOrWhiteSpace(propName)) {
        return null;
    }
    let elmStyles = window.getComputedStyle(elm);
    return elmStyles.getPropertyValue(propName).trim();
}



function setElementComputedStyleProp(elm, propName, value) {
    if (!elm) {
        debugger;
    }
    elm.style.setProperty(propName, value);
}

function clearElementClasses(elm) {
    while(elm.classList.length > 0) {
        elm.classList.remove(elm.classList[0]);
	}
} 
async function readFileAsDataURL(file) {
    let result_base64 = await new Promise((resolve) => {
        let fileReader = new FileReader();
        fileReader.onload = (e) => resolve(fileReader.result);
        fileReader.readAsDataURL(file);
    });

    log('loaded file: ' + file);
    log('data: '+result_base64); 

    return result_base64;
}

function numToPaddedStr(num, padStr, padCount) {
    let numStr = '';
    if (typeof num === 'string' || num instanceof String) {
        numStr = num;
    } else {
        numStr = num.toString();
    }
    let pad_needed = padCount - numStr.length;
    for (var i = 0; i < pad_needed; i++) {
        numStr = padStr + numStr;
    }
    return numStr;
}

function get2dIdx(idx, cols) {
    return [
        parseInt(idx / cols), // row
        parseInt(idx % cols) // col
    ];
}

function insertTextAtIdx(str, idx, text) {
    return str.substr(0, idx) + text + str.substr(idx);
}

function replaceTextInRange(str, text, idx, length) {
    str = str.substr(0, idx) + str.substr(idx+length)
    return insertTextAtIdx(str, idx, text);
}

function convertIntToRomanNumeral(num) {
    if (!num) {
        return '';
    }
    num = parseInt(num);
    let roman = ["M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I"];
    let arabic = [1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1];
    let index = 0;
    let result = "";
    while (num > 0) {
        if (num >= arabic[index]) {
            result += roman[index];
            num -= arabic[index];
        } else index++;
    }

    return result;
}

function convertRomanNumeralToInt(str1) {
    if (str1 == null) return -1;

    function char_to_int(c) {
        switch (c) {
            case 'I': return 1;
            case 'V': return 5;
            case 'X': return 10;
            case 'L': return 50;
            case 'C': return 100;
            case 'D': return 500;
            case 'M': return 1000;
            default: return -1;
        }
    }

    var num = char_to_int(str1.charAt(0));
    var pre, curr;

    for (var i = 1; i < str1.length; i++) {
        curr = char_to_int(str1.charAt(i));
        pre = char_to_int(str1.charAt(i - 1));
        if (curr <= pre) {
            num += curr;
        } else {
            num = num - pre * 2 + curr;
        }
    }

    return num;
}


