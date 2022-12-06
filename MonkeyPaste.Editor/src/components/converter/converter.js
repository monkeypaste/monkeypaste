// #region Globals

var IsConverterLoaded = false;

// #endregion Globals

// #region Life Cycle

function initPlainHtmlConverter() {
	quill = initQuill();
	getEditorContainerElement().firstChild.setAttribute('id', 'quill-editor');

	getEditorElement().classList.add('ql-editor-converter');

	IsConverterLoaded = true;
	IsLoaded = true;

	onInitComplete_ntf();
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

	if (formatType == 'text') {
		//dataStr = unescapeHtml(dataStr);
		//insertHtml(0, dataStr, 'api');
		//setRootHtml(escapeHtmlSpecialEntities(dataStr));
		//insertText(0, dataStr);
		insertText(0, escapeHtmlSpecialEntities(dataStr), 'silent');
		//setRootText(escapeHtmlSpecialEntities(dataStr));
	} else if (formatType == 'html') {
		//if (dataStr.toLowerCase().indexOf('<p>') < 0) {
		//	dataStr = '<p>' + dataStr + '</p>';
		//}
		//dataStr = unescapeHtml(dataStr);
		//dataStr = decodeURIComponent(unescapeHtml(encodeURIComponent(dataStr)));
		// NOTE insertHtml will remove spaces between spans...
		//insertHtml(0, dataStr, 'api', false);
		//setRootHtml(dataStr);

		//const delta = quill.clipboard.convert(dataStr);
		//quill.setContents(delta, 'silent')
		const delta = convertHtmlToDelta(dataStr);
		setContents(delta);
	}

	quill.update();
	let qhtml = getHtml();
	// NOTE this maybe only necessary on windows
	//qhtml = fixHtmlBug1(qhtml);
	//qhtml = removeUnicode(qhtml);
	//qhtml = fixUnicode(qhtml);
	//qhtml = forceBgOpacity(qhtml, bgOpacity);

	setRootHtml(qhtml);

	log('');
	log('RichHtml: ');
	log(qhtml);
	return qhtml;
}
function forceBgOpacity(htmlStr, opacity) {
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
