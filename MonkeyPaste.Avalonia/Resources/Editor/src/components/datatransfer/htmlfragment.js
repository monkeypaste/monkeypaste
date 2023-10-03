// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getDataTransferDelta(dt) {
    if (globals.ContentItemType != 'Text') {
        log('Warning! Only text items should use deltas for data transfer, this is not text type');
        debugger;
    }
    let dt_html_str = getDataTransferHtmlFragment(dt).html;

    // ignore html for append when dest formating pref is enabled
    const can_transfer_formatted_content =
        !isNullOrEmpty(dt_html_str) &&
        !globals.isDataTransferDestFormattingEnabled;
    
    let result_delta = null;
    if (can_transfer_formatted_content) {
        result_delta = convertHtmlToDelta(dt_html_str);
    } else {
        let dt_pt_str = getDataTransferPlainText(dt);
        if (!isNullOrEmpty(dt_pt_str)) {
            result_delta = convertHtmlToDelta(dt_pt_str);
        } else {
            // no text format on data object
            // when html found fallback and 
            // convert that to delta and strip attributes
            result_delta = {
                ops: convertHtmlToDelta(dt_html_str)
                    .filter(x => x.insert !== undefined)
                    .map(x => ({ insert: x.insert }))
            };
        }
    }
    result_delta = decodeHtmlEntitiesInDeltaInserts(result_delta);
    return result_delta;
}
function getDataTransferHtmlFragment(dt) {
    let dt_html_str = null;
    let dt_html_source = null;
    if (dt.types.includes('html format')) {
        // prefer system html format to get sourceurl (on windows)
        dt_html_str = b64_to_utf8(dt.getData('html format'));
    } else if (dt.types.includes('text/html')) {
        dt_html_str = dt.getData('text/html');
    }
    let result = parseHtmlFromHtmlClipboardFragment(dt_html_str);
    return result;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function containsHtmlClipboardFragment(dataStr) {
    if (!isString(dataStr) ||
        !dataStr.includes("StartFragment") ||
        !dataStr.includes("EndFragment")) {
        return false;
    }
    return true;
}
function isHtmlClipboardFormat(dataStr) {
    // TODO need to check common browser html clipboard formats this is only for Chrome on Windows
    if (!isString(dataStr) ||
        !dataStr.startsWith("Version:") ||
        !dataStr.includes("StartHTML:") ||
        !dataStr.includes("EndHTML:")) {
        return false;
    }
    return true;
}

// #endregion State

// #region Actions


function parseHtmlFromHtmlClipboardFragment(cbDataStr) {
    // PARSE URL

    let cbData = {
        sourceUrl: '',
        html: cbDataStr
    };
    if (!isHtmlClipboardFormat(cbDataStr)) {
        if (containsHtmlClipboardFragment(cbDataStr)) {
            // NOTE this shouldn't really happen but in case fragment wasn't parsed
            cbData.html = cleanHtmlForFragmentMarkers(cbDataStr);
        }
        return cbData;
    }

    let sourceUrlToken = 'SourceURL:';
    let source_url_start_idx = cbDataStr.indexOf(sourceUrlToken) + sourceUrlToken.length;
    if (cbDataStr.includes(sourceUrlToken) &&
        source_url_start_idx >= 0) {
        let source_url_length = substringByLength(cbDataStr, source_url_start_idx).indexOf(envNewLine());
        if (source_url_length >= 0) {
            let parsed_url = substringByLength(cbDataStr, source_url_start_idx, source_url_length);
            if (isValidHttpUrl(parsed_url)) {
                cbData.sourceUrl = parsed_url;
            }
        }
    }

    // PARSE HTML
    if (containsHtmlClipboardFragment(cbDataStr)) {
        cbData.html = cleanHtmlForFragmentMarkers(cbDataStr);
    }

    return cbData;
}

function parseHtmlFragmentVersion(cbDataStr) {
    let version_parts = cbDataStr.split('Version:');
    if (version_parts.length < 2) {
        return null;
    }
    let version_num_parts = version_parts[1].split('\n');
    if (version_num_parts.length < 2) {
        return null;
    }
    return version_num_parts[0];
}

function cleanHtmlForFragmentMarkers(htmlStr) {

    let version = parseHtmlFragmentVersion(htmlStr);
    if (version == null) {
        return htmlStr;
    }
    if (version.startsWith('0.9')) {
    // NOTE reading raw clipboard format html (at least from chrome on windows)
    // has line breaks at opening/closing of <body> tag which get picked up
    // so this pulls inner fragment out which will not have those
        let htmlStartToken = '<!--StartFragment-->';
        let htmlEndToken = '<!--EndFragment-->';
        const frag_start_idx = htmlStr.indexOf(htmlStartToken);
        if (frag_start_idx >= 0) {
            let html_start_idx = frag_start_idx + htmlStartToken.length;
            let html_end_idx = htmlStr.indexOf(htmlEndToken);
            let html_length = html_end_idx - html_start_idx;
            return substringByLength(htmlStr, html_start_idx, html_length);
        }
    } else if (version.startsWith('1.0')) {
        // just returning everything after EndFragment line
        let end_html_line_parts = htmlStr.split('EndFragment:');
        if (end_html_line_parts.length < 2) {
            return null;
        }
        let next_line_parts = end_html_line_parts[1].split('\n');
        if (next_line_parts.length < 2) {
            return null;
        }
        next_line_parts.splice(0, 1);
        let result = next_line_parts.join('\n');
        return result;
    } else {
        log(`unknown html fragment type: ${version}`);
        debugger;
        return htmlStr;
    }
    

    return htmlStr;
}

function createHtmlClipboardFragment(htmlStr) {
    // NOTE not sure if this varies by OS, assuming no
    /*
    Version:0.9
    StartHTML:0000000165
    EndHTML:0000001132
    StartFragment:0000000201
    EndFragment:0000001096
    SourceURL:https://github.com/loagit/Quill-Examples-and-FAQ
    <html>
    <body>
    <!--StartFragment--><ol dir="auto" style="box-sizing: border-box; padding-left: 2em; margin-top: 0px; margin-bottom: 16px; color: rgb(36, 41, 47); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;"><li style="box-sizing: border-box;">ntry point. Open it to see the editor.</li><li style="box-sizing: border-box; margin-top: 0.25em;">app.js - The JavaScript source co</li></ol><!--EndFragment-->
    </body>
    </html>
    */

    let num_str = '0000000000';
    let pre_fragment_str = '<!--StartFragment-->';
    let post_fragment_str = '<!--EndFragment-->';
    let sourceUrl = `${globals.LOCAL_HOST_DOMAIN}?type=CopyItem&handle=${globals.ContentHandle}`;
    let join_str = envNewLine();

    let fragment_parts = [
        'Version:0.9',
        'StartHTML:' + num_str, //[1]
        'EndHTML:' + num_str,     //[2]
        'StartFragment:' + num_str,
        'EndFragment:' + num_str,
        'SourceURL:' + sourceUrl,
        '<html>',                   //[6]
        '<body>',
        pre_fragment_str,     //[8]
        '</body>',
        '</html>'
    ];

    fragment_parts[8] += htmlStr + post_fragment_str;

    let start_html_idx = fragment_parts.slice(0, 6).join('').length + (6 * join_str.length);
    let end_html_idx = fragment_parts.join(join_str).length;// + (fragment_parts.length * join_str.length);

    let start_fragment_idx = fragment_parts.slice(0, 8).join('').length + (8 * join_str.length);
    start_fragment_idx += pre_fragment_str.length;
    let end_fragment_idx = fragment_parts.slice(0, 9).join('').length + (9 * join_str.length);

    fragment_parts[1] = fragment_parts[1].replace(num_str, numToPaddedStr(start_html_idx, '0', 10));
    fragment_parts[2] = fragment_parts[2].replace(num_str, numToPaddedStr(end_html_idx, '0', 10));

    fragment_parts[3] = fragment_parts[3].replace(num_str, numToPaddedStr(start_fragment_idx, '0', 10));
    fragment_parts[4] = fragment_parts[4].replace(num_str, numToPaddedStr(end_fragment_idx, '0', 10));

    return fragment_parts.join(join_str);
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers