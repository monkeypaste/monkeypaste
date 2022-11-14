// #region Globals

var quill;
// #endregion Globals

// #region Life Cycle
function initQuill(editorId = '#editor', toolbarId = '#editorToolbar') {
	hljs.configure({   // optionally configure hljs
		languages: ['javascript', 'ruby', 'python', 'xml', 'html', 'xhtml']
	});

	//class PreserveWhiteSpace {
	//	constructor(quill,options) {
	//		quill.container.style.whiteSpace = "pre-line";
	//	}
	//}
	//Quill.register('modules/preserveWhiteSpace', PreserveWhiteSpace);

	let quillOptions = {
		//debug: true,
		placeholder: "",
		//allowReadOnlyEdits: true,
		theme: "snow",
		formula: true,
		preserveWhiteSpace: true,
		syntax: true,
		//modules: {
		//	toolbar: '#editorToolbar',
		//	//table: !UseBetterTable,
		//	//htmlEditButton: {
		//	//	syntax: true
		//	//}
		//}
	}

	quillOptions = initEditorToolbarQuillOptions(quillOptions, toolbarId);

	let quill_instance = new Quill(editorId, quillOptions);

	quill_instance.root.setAttribute("spellcheck", "false");
	return quill_instance;
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

function getRootHtml() {
	return quill.root.innerHTML;
}

function getHtml(range) {
	if (ContentItemType != 'Text') {
		return getRootHtml();
	}
	range = !range ? { index: 0, length: getDocLength() } : range;	
	return getHtml2(range);
	//let result = '';

	//let range_content = getDelta(range);
	//let tempContainer = document.createElement('div');
	//tempContainer.setAttribute('id', 'tempContainer');
	//try {
	//	//let tempToolbar = document.createElement('div');
	//	//tempToolbar.setAttribute('id', 'tempToolbar');
	//	let tempQuill = new Quill(tempContainer); //initQuill(tempContainer, tempToolbar);

	//	tempQuill.setContents(range_content);
	//	let result = tempContainer.querySelector(".ql-editor").innerHTML;
	//	tempContainer.remove();
	//	//tempToolbar.remove();

	//	let htmlStr = result;
	//	let htmlStr_unescaped = unescapeHtml(htmlStr);

	//	result = htmlStr_unescaped;
	//} catch (ex) {
	//	debugger;
	//}
	//return result;
}

function getSelectedHtml() {
	let selection = getDocSelection();
	let sel_html = getHtml(selection);
	return sel_html;
}

function getHtml2(sel) {
	let dom_range = convertDocRangeToDomRange(sel);
	if (dom_range) {
		let clonedSelection = dom_range.cloneContents();
		let div = document.createElement('div');
		div.appendChild(clonedSelection);
		let htmlStr = div.innerHTML;
		div.remove();
		return htmlStr;
	}
	return "";
}

function getSelectedHtml3() {
	let selection = window.getDocSelection();
	if (selection.rangeCount > 0) {
		let range = selection.getRangeAt(0);
		let documentFragment = range.cloneContents();
	}
	console.log(documentFragment || 'nothing selected');
	return documentFragment;
}

function getDelta(rangeObj) {
	// NOTE if quill is not enabled it return empty contents
	let wasEnabled = quill.isEnabled();
	quill.enable(true);
	rangeObj = rangeObj == null ? { index: 0, length: getDocLength() } : rangeObj;

	let delta = quill.getContents(rangeObj.index, rangeObj.length);
	quill.enable(wasEnabled);

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
	let selDelta = getDelta(sel);
	return selDelta;
}

function getSelectedDeltaJson() {
	let selDelta = getSelectedDelta();
	let selDeltaStr = JSON.stringify(selDelta);
	return selDeltaStr;
}
function getFormatAtDocIdx(docIdx) {
	return quill.getFormat(docIdx, 0);
}
function getFormatForDocRange(docRange) {
	return quill.getFormat(docRange);
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

function setRootText(text) {
	setRootHtml('');
	quill.root.innerText = text;
}

function setHtmlInRange(range, htmlStr, source = 'api', decodeTemplates = false) {
	quill.deleteText(range.index, range.length, source);
	insertHtml(range.index, htmlStr, source, decodeTemplates);
}

function setContents(jsonStr) {
	quill.setContents(JSON.parse(jsonStr));
}

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function formatDocRange(range, format, source = 'api') {
	quill.formatText(range.index, range.length, format, source);
}

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

function trimQuillTrailingLineEndFromText(textStr) {
	if (textStr == null) {
		return null;
	}
	if (textStr.endsWith('\n')) {
		// remove trailing line ending
		return substringByLength(textStr, 0, textStr.length - 2);
	}
	return textStr;
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers
