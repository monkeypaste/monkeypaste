// #region Globals

var quill;
// #endregion Globals

// #region Life Cycle
function initQuill() {
	let quillOptions = {
		//debug: true,
		placeholder: "",
		//allowReadOnlyEdits: true,
		theme: "snow",
		modules: {
			toolbar: '#editorToolbar',
			//table: !UseBetterTable,
			//htmlEditButton: {
			//	syntax: true
			//}
		}
	}

	quillOptions = initEditorToolbarQuillOptions(quillOptions);

	quill = new Quill(
		"#editor",
		quillOptions);

	initEditorToolbar();

	quill.root.setAttribute("spellcheck", "false");


	initEditTemplateToolbar();
	initPasteTemplateToolbar();
}

// #endregion Life Cycle

// #region Getters

function getSelectedText(encodeTemplates = false) {
	var selection = getDocSelection();
	return getText(selection, encodeTemplates);
}

function getText(range, encodeTemplates = false) {
	range = range == null ? { index: 0, length: quill.getLength() } : range;
	let text = '';
	if (IsLoaded & encodeTemplates) {
		text = getTemplatePlainTextForDocRange(range);
	} else {
		text = quill.getText(range.index, range.length);
	}
	return text;
}

function getHtml(range) {
	range = !range ? { index: 0, length: getDocLength() } : range;
	var range_content = quill.getContents(range.index, range.length);
	var tempContainer = document.createElement("div");
	var tempQuill = new Quill(tempContainer);

	tempQuill.setContents(range_content);
	let result = tempContainer.querySelector(".ql-editor").innerHTML;
	tempContainer.remove();

	let htmlStr = result;
	let htmlStr_unescaped = unescapeHtml(htmlStr);
	return htmlStr_unescaped;
}

function getSelectedHtml() {
	var selection = getDocSelection();
	let sel_html = getHtml(selection);
	return sel_html;
}

function getSelectedHtml2() {
	var selection = window.getSelection();
	if (selection.rangeCount > 0) {
		var range = selection.getRangeAt(0);
		//var docFrag = range.cloneContents();

		//let docFragStr = DomSerializer.serializeToString(docFrag);

		//const xmlnAttribute = ' xmlns="http://www.w3.org/1999/xhtml"';
		//const regEx = new RegExp(xmlnAttribute, "g");
		//docFragStr = docFragStr.replace(regEx, "");
		//return docFragStr;
		var clonedSelection = range.cloneContents();
		var div = document.createElement('div');
		div.appendChild(clonedSelection);
		let htmlStr = div.innerHTML;
		return htmlStr;
	}
	return "";
}

function getSelectedHtml3() {
	var selection = window.getDocSelection();
	if (selection.rangeCount > 0) {
		var range = selection.getRangeAt(0);
		var documentFragment = range.cloneContents();
	}
	console.log(documentFragment || 'nothing selected');
	return documentFragment;
}

function getDelta(rangeObj) {
	rangeObj = rangeObj == null ? { index: 0, length: quill.getLength() } : rangeObj;

	let delta = quill.getContents(rangeObj.index, rangeObj.length);
	return delta;
}

function getDeltaJson(rangeObj, encodeWithContentHandle = false) {
	let delta = getDelta(rangeObj);
	if (encodeWithContentHandle) {
		delta.contentHandle = ContentHandle;
	}
	let deltaJson = JSON.stringify(delta);
	return deltaJson;
}

function getSelectedDelta() {
	let sel = getDocSelection();
	if (!sel) {
		return null;
	}
	let selDelta = quill.getContents(sel.index, sel.length);
	return selDelta;
}

function getSelectedDeltaJson() {
	let selDelta = getSelectedDelta();
	let selDeltaStr = JSON.stringify(selDelta);
	return selDeltaStr;
}
// #endregion Getters

// #region Setters

function setTextInRange(range, text, source = 'api', decodeTemplates = false) {
	quill.deleteText(range.index, range.length, source);
	insertText(range.index, text, source, decodeTemplates);
}

function setRootHtml(html) {
	quill.root.innerHTML = html;
}

function setHtmlInRange(range, htmlStr, source = 'api', decodeTemplates = false) {
	quill.deleteText(range.index, range.length, source);
	insertHtml(range.index, htmlStr, source, decodeTemplates);
}

function setContents(jsonStr) {
	quill.setContents(JSON.parse(jsonStr));
}

function formatDocRange(range, format, source = 'api') {
	quill.formatText(range.index, range.length, format, source);
}
// #endregion Setters

// #region State

// #endregion State

// #region Actions

function insertText(docIdx, text, source = 'api', decodeTemplates = false) {
	if (decodeTemplates) {
		decodeInsertedTemplates(docIdx, text, source);
		return;
	}
	quill.insertText(docIdx, text, source);
}


function insertHtml(docIdx, htmlStr, source = 'api', decodeTemplates = true) {
	quill.clipboard.dangerouslyPasteHTML(docIdx, htmlStr, source);
	if (decodeTemplates) {
		// default is true unlike text, since blot's need to be bound to events not sure if thats always right
		loadTemplates();
	}
}

function insertContent(docIdx, data, forcePlainText = false) {
	// TODO need to determine data type here
	if (forcePlainText) {
		insertText(docIdx, data);
		return;
	}
	debugger;
	insertHtml(docIdx, data);
}

function insertDelta(range, deltaOrDeltaJsonStr) {
	let deltaObj = deltaOrDeltaJsonStr;
	if (typeof deltaObj === 'string' || deltaObj instanceof String) {
		deltaObj = JSON.parse(deltaOrDeltaJsonStr);
		//deltaObj = Object.assign(new Delta, plainObj);
	}

	setTextInRange(range, '');
	deltaObj.ops = [{ retain: range.index }, ...deltaObj.ops];
	quill.updateContents(deltaObj);
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers
