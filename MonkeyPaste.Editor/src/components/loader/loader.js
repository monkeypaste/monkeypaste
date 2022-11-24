// #region Globals


const REL_EXTERNAL_ROOT_DIR = 'node_modules';
const REL_INTERNAL_ROOT_DIR = 'src';

var UseQuill2 = true;
var UseBetterTable = true;

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function loadEditorGlobals() {
    if (window.location.search != '') {
        // this is the case for converter which uses 1.3.6
        // because it doesn't have that whitespace exclusion bug between spans
        // it doesn't need the quill tables because it converts plain html which doesn't rely on better table
        UseBetterTable = false;
        UseQuill2 = false;
    }
}

function loadVariableStyles() {
    if (UseQuill2 && UseBetterTable) {
        //document.write('<link href="lib/Quill-Better-Table/quill-better-table.css" rel="stylesheet" type="text/css" />');
        //document.write('<link href="node_modules/quill-better-table/dist/quill-better-table.css" rel="stylesheet" type="text/css" />');
        addStyle(`${REL_EXTERNAL_ROOT_DIR}/quill-better-table/dist/quill-better-table.css`);
    }
}

function loadVariableExternalScripts() {
    if (UseQuill2) {
        //document.write('<script src="https://cdn.quilljs.com/2.0.0-dev.4/quill.js"><\/script>');
        //document.write('<script src="lib/Quill/quill.min.js"><\/script>');
        //document.write('<script src="node_modules/quilldev2/dist/quill.js"><\/script>');
        //document.write('<script src="node_modules/quilldev2/dist/quill.js"><\/script>');
        addScript(`${REL_EXTERNAL_ROOT_DIR}/quilldev2/dist/quill.js`);
        if (UseBetterTable) {
            //document.write('<script src="lib/Quill-Better-Table/quill-better-table.min.js"><\/script>');
            //document.write('<script src="node_modules/quill-better-table/dist/quill-better-table.js"><\/script>');
            addScript(`${REL_EXTERNAL_ROOT_DIR}/quill-better-table/dist/quill-better-table.js`);
        }
    } else {
        //document.write('<script src="https://cdn.quilljs.com/1.3.7/quill.js"><\/script>');
        //document.write('<script src="node_modules/quill/dist/quill.min.js"><\/script>');
        addScript(`${REL_EXTERNAL_ROOT_DIR}/quill/dist/quill.min.js`);
    }
}

function loadLifeCycleHandlers() {
    document.addEventListener('DOMContentLoaded', (e) => {
        //init_test();
        onDomLoaded_ntf();
    });

    window.onbeforeunload = function (e) {
        //if (!isRunningInHost()) {
        //    return;
        //}

        //alert('Are you sure you want to leave?');
        log('unload rejected');
        e.preventDefault();
        e.stopPropagation
        return false;
    };
}

function loadVariableInternalScripts() {
    if (UseQuill2) {
        //document.write('<script src="src/components/template/blots/2.0/templateEmbedBlot.js"><\/script>');
        //document.write('<script src="src/components/template/blots/2.0/templatePadAttribute.js"><\/script>');
        addScript(`${REL_INTERNAL_ROOT_DIR}/components/template/blots/2.0/templateEmbedBlot.js`);
        //addScript(`${REL_INTERNAL_ROOT_DIR}/components/template/blots/2.0/templatePadAttribute.js`);
    } else {
        //document.write('<script src="src/components/template/blots/1.0/templateEmbedBlot.js"><\/script>');
        //document.write('<script src="src/components/template/blots/1.0/templatePadAttribute.js"><\/script>');
        addScript(`${REL_INTERNAL_ROOT_DIR}/components/template/blots/1.0/templateEmbedBlot.js`);
        //addScript(`${REL_INTERNAL_ROOT_DIR}/components/template/blots/1.0/templatePadAttribute.js`);
    }
}

function addStyle(href) {
    let link_elm = document.createElement('link');
    link_elm.setAttribute('href', href);
    link_elm.setAttribute('rel', 'stylesheet');
    link_elm.setAttribute('type', 'text/css');
    document.head.appendChild(link_elm);
}

function addScript(src) {
    let script_elm = document.createElement('script');
    script_elm.setAttribute('src', src);
    script_elm.setAttribute('type', 'text/javascript');
    document.head.appendChild(script_elm);
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers