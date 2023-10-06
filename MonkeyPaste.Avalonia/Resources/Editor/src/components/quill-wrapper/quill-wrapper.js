
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
		//allowReadOnlyEdits: true,
		theme: "snow",
		formula: true,
		history: {
			delay: 1000,
			userOnly: false,
			maxStack: globals.MaxUndoLimit < 0 ? Number.MAX_SAFE_INTEGER : globals.MaxUndoLimit
		},
		formats: 'background',
		modules: {
			toolbar: toolbarId,
			//syntax: {
			//	highlight: highlightCode,
			//	interval: 100
			//},
			syntax: true
		}
	}

	quillOptions = initEditorToolbarQuillOptions(quillOptions, toolbarId);

	let quill_instance = new Quill(editorId, quillOptions);

	quill_instance.root.setAttribute("spellcheck", globals.IsSpellCheckEnabled);

	quill_instance.getModule("toolbar").container.addEventListener("mousedown", (e) => {
		e.preventDefault();
	});

	getEditorContainerElement().firstChild.setAttribute('id', 'quill-editor');
	log('quill version: ' + Quill.version);
	return quill_instance;
}

// #endregion Life Cycle

// #region Getters

function getSelectedText(encodeTemplates = false) {
	var selection = getDocSelection();
	return getText(selection, encodeTemplates);
}

function getText(range, selectionOnly = false) {
	//updateQuill();
	range = range == null ? { index: 0, length: getDocLength() } : range;
	let text = '';
	if (globals.IsLoaded && selectionOnly) {
		let encoded_text = getEncodedContentText(range);
		text = getDecodedContentText(encoded_text);
	} else {
		text = globals.quill.getText(range.index, range.length);
	}

	return text;
}

function getAllText() {
	if (!globals.quill) {
		return '';
	}
	return globals.quill.getText();
}

function getRootHtml() {
	return globals.quill.root.innerHTML;
}

function getHtml(range, encodeHtmlEntities = true) {
	if (globals.ContentItemType != 'Text' ||
		(isTableInDocument() && isNullOrUndefined(range))) {
		let root_html = getRootHtml();
		if (encodeHtmlEntities) {
			return encodeHtmlSpecialEntitiesFromHtmlDoc(root_html);
		}
		return root_html;
	}

	range = isNullOrUndefined(range) ? { index: 0, length: getDocLength() } : range;
	let delta = getDelta(range);
	delta = restoreContentColorsFromDelta(delta);

	if (encodeHtmlEntities) {
		delta = encodeHtmlEntitiesInDeltaInserts(delta);
	}
	let htmlStr = convertDeltaToHtml(delta, false);
	return htmlStr;
}


function getHtmlWithTables(range) {
	if (isNullOrUndefined(range)) {
		// BUG when using full range the below approach skips outer div/table and starts right on tr, dunno why
		return getRootHtml();
	}
	range = isNullOrUndefined(range) ? { index: 0, length: getDocLength() } : range;
	let dom_range = convertDocRangeToDomRange(range);
	if (dom_range) {
		let div = document.createElement('div');
		let actual_contents = dom_range.cloneContents();

		let start_elm = dom_range.startContainer;
		if (start_elm.nodeType == 3) {
			let start_block_parent_elm = getBlockElementAtDocIdx(range.index);
			dom_range.setStart(start_block_parent_elm, 0);
		}
		let end_elm = dom_range.endContainer;
		if (end_elm.nodeType == 3) {
			let end_block_parent_elm = getBlockElementAtDocIdx(range.index + range.length);
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
	return globals.quill.getFormat(docIdx, 0);
}
function getFormatForDocRange(docRange) {
	return globals.quill.getFormat(docRange);
}

// #endregion Getters

// #region Setters

function setTextInRange(range, text, source = 'api', decodeTemplates = false) {
	globals.quill.deleteText(range.index, range.length, source);
	insertText(range.index, text, source, decodeTemplates);
}

function setRootHtml(htmlStr) {
	globals.quill.root.innerHTML = htmlStr;
	updateQuill();
}

function setRootElement(elm) {
	globals.quill.root.replaceChildren(elm);
	updateQuill();
}

function setRootText(text) {
	setRootHtml('');
	globals.quill.root.innerText = text;
}

function setHtmlInRange(range, htmlStr, source = 'api', decodeTemplates = false) {
	globals.quill.deleteText(range.index, range.length, source);
	insertHtml(range.index, htmlStr, source, decodeTemplates);
}

function setContents(delta, source = 'api') {
	globals.quill.setContents(delta,source);
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
	if (globals.quill == null) {
		// load error
		debugger;
	}
	return blot.offset(globals.quill.scroll);
}

function formatDocRange(range, format, source = 'api') {
	globals.quill.formatText(range.index, range.length, format, source);
}

function replaceFormatInDocRange(range, format, source = 'api') {
	// get cur formatting
	let replaced_format = getFormatForDocRange(range);
	// set all orig formatting to false
	let orig_keys = Object.keys(replaced_format);
	for (var i = 0; i < orig_keys.length; i++) {
		replaced_format[orig_keys[i]] = false;
	}
	// add or replace new formats
	let new_keys = Object.keys(format);
	for (var i = 0; i < new_keys.length; i++) {
		replaced_format[new_keys[i]] = format[new_keys[i]];
	}

	formatDocRange(range, replaced_format, source);
	let test = getFormatForDocRange(range);
	return;
}

function formatSelection(format, value, source = 'api') {
	globals.quill.format(format, value, source);
}

function insertText(docIdx, text, source = 'api', decodeTemplates = false) {
	if (decodeTemplates) {
		decodeInsertedTemplates(docIdx, text, source);
		return;
	}
	globals.quill.insertText(docIdx, text, source);
}

function deleteText(range, source = 'api') {
	if (!range || range.length == 0) {
		return;
	}
	globals.quill.deleteText(range.index, range.length, source);
}

function insertHtml(docIdx, htmlStr, source = 'api', decodeTemplates = true) {
	globals.quill.clipboard.dangerouslyPasteHTML(docIdx, htmlStr, source);
	if (decodeTemplates) {
		// default is true unlike text, since blot's need to be bound to events not sure if thats always right
		loadTemplates();
		unparentTemplatesAfterHtmlInsert()
	}
}

function setEditorHtml(htmlStr, source = 'api', decodeTemplates = true) {
	// NOTE this works around when setRootHtml to blank it DOES NOT go blank
	// when insert is called it has that stupid newline as the document so it'll tack on 
	// an extra beyond what everything already accounts for
	setRootHtml('');
	insertHtml(0, htmlStr, source, decodeTemplates);
	deleteText({ index: getDocLength() - 1, length: 1 }, source);
}

function setEditorText(text, source = 'api', decodeTemplates = true) {
	// NOTE this works around when setRootHtml to blank it DOES NOT go blank
	// when insert is called it has that stupid newline as the document so it'll tack on 
	// an extra beyond what everything already accounts for
	setRootHtml('');
	insertText(0, text, source, decodeTemplates);
	deleteText({ index: getDocLength() - 1, length: 1 }, source);
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

function updateQuill() {
	if (!globals.quill) {
		return;
	}
	globals.quill.update();
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers
