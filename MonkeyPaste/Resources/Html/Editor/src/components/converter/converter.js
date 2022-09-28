var IsConverterLoaded = false;

function initPlainHtmlConverter(envName, useBetterTable) {
	EnvName = envName;

	initQuill(useBetterTable);

	//document.getElementsByClassName("ql-toolbar")[0].classList.add("env-wpf");
	disableReadOnly();
	//hideEditorToolbar();
	window.addEventListener(
		"resize",
		function (event) {
			updateAllSizeAndPositions();
		},
		true
	);

	updateAllSizeAndPositions();

	IsConverterLoaded = true;
	IsLoaded = true;
	//return "CONVERTER LOADED";
}


function convertPlainHtml(plainHtml, forceBgOpacity = 0.7) {
	if (!IsConverterLoaded) {
		log('convertPlainHtml error! converter not initialized, returning null');
		return null;
	} 
	plainHtml = unescapeHtml(plainHtml);
	//plainHtml = removeUnicode(plainHtml);

	log("Converting This Plain Html:");
	log(plainHtml);

	let doc_range = { index: 0, length: getDocLength() };
	setTextInRange(doc_range, '');

	insertHtml(0, plainHtml);

	//setHtml(plainHtml);

	quill.update();
	let qhtml = getHtml();
	qhtml = removeUnicode(qhtml);
	qhtml = fixUnicode(qhtml);
	qhtml = forceHtmlBgAlpha(qhtml);

	setHtml(qhtml);

	log('');
	log('RichHtml: ');
	log(qhtml);
	return qhtml;
}

function forceHtmlBgAlpha(htmlStr, opacity = .25) {
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

function removeUnicode(str) {
	str = str.replace(/[\uE000-\uF8FF]/ig, '');
	return str;
}
function fixUnicode(text) {
	// replaces any combo of chars in [] with single space
	const regex = /(?!\w*(\w)\w*\1)[Âï¿½]+/ig;
	let fixedText = text.replaceAll(regex, ' ');
	return fixedText;
}


function parseHtmlClipboardFormat(cbDataStr) {
	let cbData = {
		sourceUrl: '',
		html: ''
	};
	let sourceUrlToken = 'SourceURL:';
	let source_url_start_idx = cbDataStr.indexOf(sourceUrlToken) + sourceUrlToken.length;
	if (source_url_start_idx >= 0) {
		let source_url_length = substringByLength(cbDataStr, source_url_start_idx).indexOf(envNewLine());
		if (source_url_length >= 0) {
			let parsed_url = substringByLength(cbDataStr, source_url_start_idx, source_url_length);
			if (isValidHttpUrl(parsed_url)) {
				cbData.sourceUrl = parsed_url;
			}
		}
	}

	let htmlStartToken = '<!--StartFragment-->';
	let htmlEndToken = '<!--EndFragment-->';

	let html_start_idx = cbDataStr.indexOf(htmlStartToken) + htmlStartToken.length;
	if (html_start_idx >= 0) {
		let html_length = cbDataStr.indexOf(htmlEndToken);
		cbData.html = substringByLength(cbDataStr, html_start_idx, html_length);
	}

	return cbData;
}