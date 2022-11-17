// #region Globals

var ContentHandle = null;
var ContentItemType = 'Text';

var ContentScreenshotBase64Str = null;

const InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
const BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote']
const AllDocumentTags = [...InlineTags, ...BlockTags];

// #endregion Globals

// #region Life Cycle
function initContent() {
	registerTemplateBlots();
}

function loadContent(contentHandle, contentType, contentData, isPasteRequest, searchStateObj) {
	if (contentHandle != ContentHandle) {
		// when actually a new item and not reload
		quill.history.clear();
	}
	quill.enable(true);

	resetSelection();
	resetColorPaletteState();

	ContentHandle = contentHandle;
	ContentItemType = contentType;

	if (ContentItemType.includes('.')) {
		log('hey item type is ' + ContentItemType);
		ContentItemType = ContentItemType.split('.')[1];
		log('now item type is ' + ContentItemType);
	}

	// enusre IsLoaded is false so msg'ing doesn't get clogged up
	IsLoaded = false;

	//let contentBg_rgba = getContentBg(contentData);

	enableReadOnly();

	log('Editor loaded');

	if (ContentItemType == 'Image') {
		loadImageContent(contentData);
	} else if (ContentItemType == 'FileList') {
		loadFileListContent(contentData);
	} else if (ContentItemType == 'Text') {
		loadTextContent(contentData, isPasteRequest);
	}

	//getEditorElement().style.backgroundColor = rgbaToCssColor(contentBg_rgba);
	
	quill.update();
	if (ContentItemType != 'Text') {
		quill.enable(false);
	}
	updateAllElements();

	if (searchStateObj == null) {
		if (isShowingFindReplaceToolbar()) {
			resetFindReplaceToolbar();
			hideFindReplaceToolbar();
		}
		if (CurFindReplaceDocRanges) {
			hideScrollbars();
			resetFindReplaceResults();
		}
	} else {
		showScrollbars();
		setFindReplaceInputState(searchStateObj);
		populateFindReplaceResults();
		onQuerySearchRangesChanged_ntf(CurFindReplaceDocRanges.length);
	}

	IsReadyToPaste = !hasAnyInputRequredTemplate();
	IsLoaded = true;
	drawOverlay();
	quill.update();
	onContentLoaded_ntf();
}

// #endregion Life Cycle

// #region Getters

function getContentAsMessage() {
	quill.update();
	return {
		editorWidth: getEditorWidth(),
		editorHeight: getEditorHeight(),
		itemData: getContentData(),
		lines: parseInt_safe(getContentHeightByType()),
		length: parseInt_safe(getContentWidthByType()),
		hasTemplates: hasTemplates()
	};
}

function getContentData() {
	if (ContentItemType == 'Text') {
		return getTextContentData();
	}
	if (ContentItemType == 'Image') {
		return getImageContentData();
	}
	if (ContentItemType == 'FileList') {
		return getFileListContentData();
	}
	return '';
}

function getEncodedContentText(range) {
	if (ContentItemType == 'Text') {
		return getEncodedTextContentText(range);
	}
	if (ContentItemType == 'Image') {
		return getEncodedImageContentText(range);
	}
	if (ContentItemType == 'FileList') {
		return getEncodedFileListContentText(range);
	}
	return '';
}

function getDecodedContentText(encodedText) {
	if (ContentItemType == 'Text') {
		return getDecodedTextContentText(encodedText);
	}
	if (ContentItemType == 'Image') {
		return getDecodedImageContentText(encodedText);
	}
	if (ContentItemType == 'FileList') {
		return getDecodedFileListContentText(encodedText);
	}
	return '';
}

function getContentBg(htmlStr, contrast_opacity = 0.5) {
	if (ContentItemType != 'Text') {
		return cleanColor();
	}

	let html_doc = DomParser.parseFromString(htmlStr, 'text/html');
	let elms = html_doc.querySelectorAll(InlineTags.join(", ") + ',' + BlockTags.join(','));

	let bright_fg_count = 0;
	let dark_fg_count = 0;
	for (var i = 0; i < elms.length; i++) {
		let has_fg = !isNullOrWhiteSpace(elms[i].style.color);
		if (has_fg) {
			if (isBright(elms[i].style.color)) {
				bright_fg_count++;
			} else {
				dark_fg_count++;
			}
		}
	}
	log('brights: ' + bright_fg_count);
	log('darks' + dark_fg_count);
	// TODO need to pass theme info in init for color stuff
	let contrast_bg_rgba = cleanColor();
	if (bright_fg_count > dark_fg_count) {
		contrast_bg_rgba = cleanColor('black', contrast_opacity);
	} else if (bright_fg_count < dark_fg_count) {
		contrast_bg_rgba = cleanColor('white', contrast_opacity);
	}
	return contrast_bg_rgba;
}

function getContentWidth() {
	var bounds = quill.getBounds(0, getDocLength());
	bounds = cleanRect(bounds);
	return parseFloat(bounds.width);
}

function getContentHeight() {
	var bounds = quill.getBounds(0, getDocLength());
	bounds = cleanRect(bounds);
	return parseFloat(bounds.height);
}

async function getContentImageBase64Async(sel) {
	let sel_rects = null;
	if (sel) {
		sel_rects = getRangeRects(sel, false);
	}
	let base64Str = await getBase64ScreenshotOfElementAsync(getEditorContainerElement(), sel_rects);

	return base64Str;
}

function getContentWidthByType() {
	if (ContentItemType == 'Text') {
		return getTextContentCharCount();
	}
	if (ContentItemType == 'FileList') {
		return getTotalFileSize();
	}
	if (ContentItemType == 'Image') {
		return getImageContentWidth();
	}
}

function getContentHeightByType() {
	if (ContentItemType == 'Text') {
		return getTextContenLineCount();
	}
	if (ContentItemType == 'FileList') {
		return getFileCount();
	}
	if (ContentItemType == 'Image') {
		return getImageContentHeight();
	}
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function canEnableSubSelection() {
	return ContentItemType != 'Image';
}
function canDisableReadOnly() {
	return ContentItemType == 'Text';
}
// #endregion State

// #region Actions

function convertContentToFormats(isForOle, formats) {
	quill.update();
	let items = null;
	if (ContentItemType == 'Text') {
		items = convertTextContentToFormats(isForOle, formats);
	} else if (ContentItemType == 'FileList') {
		items = convertFileListContentToFormats(isForOle, formats);
	} else if (ContentItemType == 'Image') {
		items = convertImageContentToFormats(isForOle, formats);
	}
	return items;
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers