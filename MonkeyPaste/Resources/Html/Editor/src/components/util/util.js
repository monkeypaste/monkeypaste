//#region Color

function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function isBright(hex, brightThreshold = 150) {
    var c = hexToRgb(hex.toLowerCase());
    var grayVal = Math.sqrt(
        c.R * c.R * .299 +
        c.G * c.G * .587 +
        c.B * c.B * .114);
    return grayVal > brightThreshold;
}

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        R: parseInt(result[1], 16),
        G: parseInt(result[2], 16),
        B: parseInt(result[3], 16)
    } : null;
}

//#endregion

//#region DOM Traversal

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