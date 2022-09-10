var IsConverterLoaded = false;

function initPlainHtmlConverter() {
	reqMsg = {
		envName: 'wpf',
		isReadOnlyEnabled: true,
		usedTextTemplates: {},
		isPasteRequest: false,
		itemEncodedHtmlData: ''
	}

	EnvName = reqMsg.envName;

	loadQuill('wpf');

	document.getElementsByClassName("ql-toolbar")[0].classList.add("env-wpf");
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

function convertPlainHtml(plainHtml) {
	if (!IsConverterLoaded) {
		log('convertPlainHtml error! converter not initialized, returning null');
		return null;
	}
	let doc_range = { index: 0, length: getDocLength() };
	setTextInRange(doc_range, '');

	//quill.deleteText(0, quill.getLength());
	quill.clipboard.dangerouslyPasteHTML(plainHtml);
	quill.update();
	return getHtml();

	//log("Converting This Plain Html:");
	//log(plainHtml);
	//setHtml(plainHtml);
	//quill.deleteText(0, quill.getLength());
	//quill.clipboard.dangerouslyPasteHTML(plainHtml);
	//quill.update();
	//return getHtml();
}