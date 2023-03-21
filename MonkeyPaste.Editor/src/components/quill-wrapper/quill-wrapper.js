// #region Globals

const EditorPlaceHolderText = 'Empty Content...';
var quill;
// #endregion Globals

// #region Life Cycle

function initQuill(editorId = '#editor', toolbarId = '#editorToolbar') {
	if (Quill === undefined) {
		/// host load error case
		debugger;
	}
	//hljs.configure({   // optionally configure hljs
	//	languages: ['javascript', 'ruby', 'python', 'xml', 'html', 'xhtml']
	//});

	let quillOptions = {
		//debug: true,
		placeholder: EditorPlaceHolderText,
		//allowReadOnlyEdits: true,
		theme: "snow",
		formula: true,
		preserveWhiteSpace: true,
		history: {
			delay: 1000,
			userOnly: false,
			maxStack: MaxUndoLimit < 0 ? Number.MAX_SAFE_INTEGER : MaxUndoLimit
		},
		syntax: false,
		modules: {
			toolbar: toolbarId,
			//table: !UseBetterTable,
			//htmlEditButton: {
			//	syntax: true
			//}
		}
	}

	quillOptions = initEditorToolbarQuillOptions(quillOptions, toolbarId);

	let quill_instance = new Quill(editorId, quillOptions);

	quill_instance.root.setAttribute("spellcheck", IsSpellCheckEnabled);

	quill_instance.getModule("toolbar").container.addEventListener("mousedown", (e) => {
		e.preventDefault();
	});
	return quill_instance;
}

// #endregion Life Cycle

// #region Getters

function getSelectedText(encodeTemplates = false) {
	var selection = getDocSelection();
	return getText(selection, encodeTemplates);
}

function getText(range, for_ole = false) {
	//quill.update();
	range = range == null ? { index: 0, length: getDocLength() } : range;
	let text = '';
	if (IsLoaded && for_ole) {
		let encoded_text = getEncodedContentText(range);
		text = getDecodedContentText(encoded_text);
	} else {
		text = quill.getText(range.index, range.length);
	}

	return text;
}

function getAllText() {
	if (!quill) {
		return '';
	}
	return quill.getText();
}

function getRootHtml() {
	return quill.root.innerHTML;
}

function getHtml(range) {
	if (ContentItemType != 'Text') {
		return getRootHtml();
	}
	range = !range ? { index: 0, length: getDocLength() } : range;
	let delta = getDelta(range);

	delta = encodeHtmlEntitiesInDeltaInserts(delta);
	let htmlStr = convertDeltaToHtml(delta);
	return htmlStr;
}

function getSelectedHtml() {
	let selection = getDocSelection();
	let sel_html = getHtml(selection);
	return sel_html;
}

function getHtml2(sel) {
	sel = !sel ? { index: 0, length: getDocLength() } : sel;
	let dom_range = convertDocRangeToDomRange(sel);
	if (dom_range) {
		let div = document.createElement('div');
		let actual_contents = dom_range.cloneContents();

		let start_elm = dom_range.startContainer;
		if (start_elm.nodeType == 3) {
			let start_block_parent_elm = getBlockElementAtDocIdx(sel.index);
			dom_range.setStart(start_block_parent_elm, 0);
		}
		let end_elm = dom_range.endContainer;
		if (end_elm.nodeType == 3) {
			let end_block_parent_elm = getBlockElementAtDocIdx(sel.index + sel.length);
			dom_range.setEnd(end_block_parent_elm, 0);
		}

		let blocked_contents = dom_range.cloneContents();

		div.appendChild(blocked_contents);

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

function setContents(delta, source = 'api') {
	quill.setContents(delta,source);
}

// #endregion Setters

// #region State

// #endregion State

// #region Actions


function quillFindBlot(elm, bubble = false) {
	if (Quill === undefined) {
		/// host load error case
		debugger;
		return null;
	}
	return Quill.find(elm, bubble);
}

function quillFindBlotOffset(elm, bubble = false) {
	let blot = quillFindBlot(elm, bubble);
	if (!blot) {
		return 0;
	}
	if (quill == null) {
		// load error
		debugger;
	}
	return blot.offset(quill.scroll);
}

function formatDocRange(range, format, source = 'api') {
	quill.formatText(range.index, range.length, format, source);
}

function formatSelection(format, value, source = 'api') {
	quill.format(format, value, source);
}

function insertText(docIdx, text, source = 'api', decodeTemplates = false) {
	if (decodeTemplates) {
		decodeInsertedTemplates(docIdx, text, source);
		return;
	}
	quill.insertText(docIdx, text, source);
}

function deleteText(range, source = 'api') {
	if (!range || range.length == 0) {
		return;
	}
	quill.deleteText(range.index, range.length, source);
}

function insertHtml(docIdx, htmlStr, source = 'api', decodeTemplates = true) {
	quill.clipboard.dangerouslyPasteHTML(docIdx, htmlStr, source);
	if (decodeTemplates) {
		// default is true unlike text, since blot's need to be bound to events not sure if thats always right
		loadTemplates();
		unparentTemplatesAfterHtmlInsert()
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


function trimQuillTrailingLineEndFromText(textStr) {
	if (textStr == null) {
		return null;
	}
	if (textStr.endsWith('\n')) {
		// remove trailing line ending
		return substringByLength(textStr, 0, textStr.length - 1);
	}
	return textStr;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers
