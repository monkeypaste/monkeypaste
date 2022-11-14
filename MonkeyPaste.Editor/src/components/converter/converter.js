var IsConverterLoaded = false;

function initPlainHtmlConverter(envName) {
	EnvName = envName;

	quill = initQuill();
	getEditorContainerElement().firstChild.setAttribute('id', 'quill-editor');

	getEditorElement().classList.add('ql-editor-converter');

	//addPlainHtmlClipboardMatchers();
	//document.getElementsByClassName("ql-toolbar")[0].classList.add("env-wpf");
	//disableReadOnly();
	//hideEditorToolbar();
	//window.addEventListener(
	//	"resize",
	//	function (event) {
	//		updateAllElements();
	//	},
	//	true
	//);

	//updateAllElements();

	IsConverterLoaded = true;
	IsLoaded = true;
	//return "CONVERTER LOADED";
}


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
		//setRootHtml(escapeHtml(dataStr));
		//insertText(0, dataStr);
		insertText(0, escapeHtml(dataStr),'silent');
		//setRootText(escapeHtml(dataStr));
	} else if (formatType == 'html') {
		if (dataStr.toLowerCase().indexOf('<p>') < 0) {
			dataStr = '<p>' + dataStr + '</p>';
		}
		dataStr = unescapeHtml(dataStr);
		// NOTE insertHtml will remove spaces between spans...
		insertHtml(0, dataStr, 'api', false);
		//setRootHtml(dataStr);
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
		if ( elms[i].style.backgroundColor === undefined || elms[i].style.backgroundColor == '') {
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


