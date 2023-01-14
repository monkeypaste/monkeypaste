// #region Globals

var IsConverterLoaded = false;

// #endregion Globals

// #region Life Cycle

function initPlainHtmlConverter() {
	quill = initQuill();
	getEditorContainerElement().firstChild.setAttribute('id', 'quill-editor');

	getEditorElement().classList.add('ql-editor-converter');
	//initConverterMatchers();

	IsConverterLoaded = true;
	IsLoaded = true;

	onInitComplete_ntf();
}

function initConverterMatchers() {
	let Delta = Quill.imports.delta;
	quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
		if (node.tagName == 'TABLE') {
			debugger;
		}
		return new Delta().insert(node.data);
	});
}
// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isPlainHtmlConverter() {
	return window.location.search.toLowerCase().endsWith(HTML_CONVERTER_PARAMS.toLowerCase());
}
// #endregion State

// #region Actions
function convertPlainHtml(dataStr, formatType, bgOpacity = 0.0) {
	if (!IsConverterLoaded) {
		log('convertPlainHtml error! converter not initialized, returning null');
		return null;
	}


	log("Converting This Plain Html:");
	log(dataStr);

	setRootHtml('');
	quill.update();

	let qhtml = '';
	let formatted_delta = '';

	if (formatType == 'text') {
		insertText(0, encodeHtmlSpecialEntities(dataStr), 'silent');
	} else if (formatType == 'rtf2html') {
		const raw_delta = convertHtmlToDelta(dataStr);
		setContents(raw_delta);

		quill.update();
		qhtml = getHtml();
		// NOTE this maybe only necessary on windows
		//qhtml = fixHtmlBug1(qhtml);
		//qhtml = removeUnicode(qhtml);
		//qhtml = fixUnicode(qhtml);
		qhtml = forceHtmlBgOpacity(qhtml, bgOpacity);

		formatted_delta = convertHtmlToDelta(qhtml);
		setRootHtml(qhtml);
	}else if (formatType == 'html') {
		insertHtml(0, dataStr, 'user', false);
		formatted_delta = forceDeltaBgOpacity(getDelta(), bgOpacity);
		setContents(formatted_delta);
		qhtml = getHtml();
	}

	if (isTableInDocument()) {
		// delta-to-html doesn't convert tables 
		qhtml = getHtml2();
	}
	

	log('');
	log('RichHtml: ');
	log(qhtml);
	return {
		html: qhtml,
		delta: formatted_delta
	};
}

function forceHtmlBgOpacity(htmlStr, opacity) {
	let html_doc = DomParser.parseFromString(htmlStr, 'text/html');
	let elms = html_doc.querySelectorAll(InlineTags.join(", ") + ',' + BlockTags.join(','));
	for (var i = 0; i < elms.length; i++) {
		if (elms[i].style.backgroundColor === undefined || elms[i].style.backgroundColor == '') {
			continue;
		}
		let rgba = cleanColor(elms[i].style.backgroundColor);
		rgba.a = opacity;
		let newBg = rgbaToCssColor(rgba);
		elms[i].style.backgroundColor = newBg;
		continue;
	}
	return html_doc.body.innerHTML;
}

function forceDeltaBgOpacity(delta, opacity) {
	if (!delta || delta.ops === undefined || delta.ops.length == 0) {
		return delta;
	}
	delta.ops
		.filter(x => x.attributes !== undefined && x.attributes.background !== undefined)
		.forEach(x => x.attributes.background = cleanColor(x.attributes.background, 0, 'rgbaStyle'));

	delta.ops
		.filter(x => x.attributes !== undefined && x.attributes.background !== undefined)
		.forEach(x => log(x.attributes.background));

	return delta;
}

function fixHtmlBug1(htmlStr) {
	// replace <span>Â </span>
	return htmlStr.replaceAll('<span>Â </span>', '');
}

function removeUnicode(str) {
	str = str.replace(/[\uE000-\uF8FF]/ig, '');
	return str;
}
function fixUnicode(text) {
	// replaces any combo of chars in [] with single space
	const regex = /(?!\w*(\w)\w*\1)[Âï¿½]+/ig;
	const regex2 = /[^\u0000-\u007F]+/ig;

	let fixedText = text.replaceAll(regex, ' ');
	fixedText = fixedText.replaceAll(regex2, '');
	return fixedText;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers
