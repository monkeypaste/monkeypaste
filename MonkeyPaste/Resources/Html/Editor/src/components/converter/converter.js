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

function convertPlainHtml(plainHtml) {
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

	log('');
	log('RichHtml: ');
	log(qhtml);
	return qhtml;
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