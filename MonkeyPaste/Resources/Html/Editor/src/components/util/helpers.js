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

async function getBase64ScreenshotOfElementAsync(element,crop_rects) {
    // from https://stackoverflow.com/a/41585230/105028
    let base64Str = '';

    let bgColor = 'white';

    let oldWidth = element.style.width;
    let oldHeight = element.style.height;
    let oldOverflow = element.style.overflow;

    element.style.width = 'auto';
    element.style.height = 'auto';
    element.style.overflow = 'visible';

    var h2c_options = {
        backgroundColor: bgColor,
        allowTaint: false,
        logging: false,
        scrollX: -window.scrollX,
        scrollY: -window.scrollY
    };

    if (crop_rects && crop_rects.length > 0) {
        let crop_block_rect = rectsUnion(crop_rects);

        h2c_options.x = crop_block_rect.left;
        h2c_options.y = crop_block_rect.top;
        h2c_options.width = crop_block_rect.width;
        h2c_options.height = crop_block_rect.height;
	}

    html2canvas(element, h2c_options).then(imgCanvas => {
        let imgSrcVal = null;
        //if (crop_rects && crop_rects.length > 1) {
        //    imgSrcVal = cropCanvas2Canvas(imgCanvas, crop_rects, h2c_options);
        //}
        if (!imgSrcVal) {
            // if no selection cropping or cropping failed?
            imgSrcVal = imgCanvas.toDataURL("image/png");
        }

        base64Str = imgSrcVal.replace("data:image/png;base64,", "");
        element.style.width = oldWidth;
        element.style.height = oldHeight;
        element.style.overflow = oldOverflow;
    });

    while (base64Str == '') {
        await delay(100);
    }
    return base64Str;
}

function cropCanvas2Canvas(imgCanvas,crop_rects, h2c_options) {
    if (crop_rects && crop_rects.length > 1) {
        // crop selection by making empty canvas same size as screen shot
        // then only copy the rects of selection so overflow is excluded

        let block_ctx = imgCanvas.getContext('2d');
        if (block_ctx) {
            let crop_canvas = document.createElement('canvas');

            document.body.appendChild(crop_canvas);

            //crop_canvas.style.left = imgCanvas.style.left;
            //crop_canvas.style.top = imgCanvas.style.top;

            //crop_canvas.width = h2c_options.width;
            //crop_canvas.height = h2c_options.height;

            crop_canvas.width = imgCanvas.width;
            crop_canvas.height = imgCanvas.height;

            let crop_ctx = crop_canvas.getContext('2d');
            if (crop_ctx) {
                block_ctx.globalCompositeOperation = 'source-out';

                crop_ctx.clearRect(0, 0, crop_canvas.width, crop_canvas.height);
                //let block_img = new Image();
                //block_img.src = imgCanvas.toDataURL();
                //document.body.appendChild(block_img);
                //let scale_x = h2c_options.width / crop_canvas.width;
                //let scale_y = h2c_options.height / crop_canvas.height;

                for (var i = 0; i < crop_rects.length; i++) {
                    let cr = crop_rects[i];

                    /*
                     This variant of the function with 9 arguments allows you to select a portion of the image and use that.
                     The selected area is then resized to fit the width and height that you specify. 
                     This allows you to select a section of your image to display. You should note the order of the arguments. 
                     The area to select X/Y/W/H comes before the displayed X/Y/W/H.

                        The image object (as returned by document.getElementById() )
                        The X coordinate of the area to select
                        The Y coordinate of the area to select
                        The width of the area to select
                        The height of the area to select
                        The X coordinate to draw the image at
                        The Y coordinate to draw the image at
                        The width to draw the image
                        The height to draw the image
                    */
                    //crop_ctx.drawImage(
                    //    block_img,
                    //    cr.left, cr.top, cr.width, cr.height,
                    //    cr.left, cr.top, cr.width, cr.height);
                    //cr.left * scale_x, cr.top * scale_y, cr.width * scale_x, cr.height * scale_y);


                    //crop_ctx.putImageData(block_ctx.getImageData(cr.left, cr.top, cr.width, cr.height), cr.left, cr.top);
                }

                //crop_ctx.scale(scale_x, scale_y);

                imgSrcVal = crop_canvas.toDataURL("image/png");
            }

            document.body.removeChild(crop_canvas);
        }
    }
}


