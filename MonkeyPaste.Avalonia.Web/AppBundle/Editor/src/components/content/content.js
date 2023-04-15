// #region Globals
const CONTENT_CLASS_PREFIX = 'content';
var ContentClassAttrb = null;

var ContentHandle = null;
var ContentItemType = 'Text';

var ContentScreenshotBase64Str = null;

const InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
const BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote', 'pre']
const AllDocumentTags = [...InlineTags, ...BlockTags];

// #endregion Globals

// #region Life Cycle

function loadContent(
	contentHandle,
	contentType,
	contentData,
	searches,
	isAppendLineMode,
	isAppendMode,
	annotationsJsonStr) {
	let is_reload = contentHandle == ContentHandle;
	let was_sub_sel_enabled = null;
	let was_editable = null;
	if (is_reload) {
		was_sub_sel_enabled = isSubSelectionEnabled();
		was_editable = !isReadOnly();
	} else {
		// when actually a new item and not reload
		quill.history.clear();
	}

	quill.enable(true);

	ContentHandle = contentHandle;
	ContentItemType = contentType;

	let sel_to_restore = null;
	if (is_reload) {
		// when content is reloaded, any selection will be lost so save to restore
		sel_to_restore = getDocSelection();
	} else {
		loadPasteButton();
		disableAppendMode();
		resetSelection();
		resetColorPaletteState();

		enableReadOnly();
		disableSubSelection();	
		resetContent();
		resetAnnotations();
	}


	if (!IsFindReplaceInactive) {
		log('activated findreplace detected during load, deactivating...');
	}
	IsFindReplaceInactive = true;

	loadContentData(contentData);

	updateQuill();
	if (ContentItemType != 'Text') {
		quill.enable(false);
	}

	if (ContentItemType == 'Image') {
		// NOTE pass annotations so load after image dimensions are known
		populateContentImageDataSize(annotationsJsonStr);
	} else {
		loadAnnotations(annotationsJsonStr);
	}
	
	loadFindReplace(searches);

	if (sel_to_restore != null) {
		sel_to_restore = cleanDocRange(sel_to_restore);
		setDocSelection(sel_to_restore)
	}

	updateAllElements();
	updateQuill();
	onContentLoaded_ntf(getContentAsMessage());


	//retain focus state on reload
	if (was_sub_sel_enabled != null && was_sub_sel_enabled) {
		enableSubSelection();
	}
	if (was_editable != null && was_editable) {
		disableReadOnly();
	}
	if (is_reload) {

		log('Editor re-loaded');
	} else {

		log('Editor loaded');
	}
}

function initContentClassAttributes() {
	const Parchment = Quill.imports.parchment;
	let suppressWarning = true;
	let config = {
		scope: Parchment.Scope.ANY,
	};
	ContentClassAttrb = new Parchment.ClassAttributor('contentType',CONTENT_CLASS_PREFIX, config);

	Quill.register(LinkTypeAttrb, suppressWarning);

}
// #endregion Life Cycle

// #region Getters

function getContentHandle() {
	return ContentHandle;
}

function getContentAsMessage() {
	updateQuill();
	return {
		editorWidth: getEditorWidth(),
		editorHeight: getEditorHeight(),
		itemData: getContentData(),
		itemSize1: parseInt_safe(getContentWidthByType()),
		itemSize2: parseInt_safe(getContentHeightByType()),
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

function resetContent() {
	resetForcedCursor();
	resetContentImage();
}

function isContentEmpty() {
	let pt = getText();
	return !pt || pt == '\n';
}

function canEnableSubSelection() {
	//return ContentItemType != 'Image';
	return true;
}

function canDisableSubSelection() {
	if (isAppendNotifier()) {
		return false;
	}
	if (isAnyAppendEnabled()) {
		return false;
	}
	if (isDropping()) {
		return false;
	}
	return true;
}

function canDisableReadOnly() {
	return ContentItemType == 'Text' && !isAppendNotifier();
}
// #endregion State

// #region Actions

function convertContentToFormats(isForOle, formats) {
	updateQuill();
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

function appendContentData(data) {
	if (isNullOrEmpty(data)) {
		return;
	}
	if (ContentItemType == 'Text') {
		appendTextContentData(data);
	} else if (ContentItemType == 'FileList') {
		appendFileListContentData(data);
	} else if (ContentItemType == 'Image') {
		appendImageContentData(data);
	}
}

function loadContentData(contentData) {
	// enusre IsLoaded is false so msg'ing doesn't get clogged up
	IsLoaded = false;

	if (ContentItemType == 'Image') {
		loadImageContent(contentData);
	} else if (ContentItemType == 'FileList') {
		loadFileListContent(contentData);
	} else if (ContentItemType == 'Text') {
		loadTextContent(contentData);
	}

	IsLoaded = true;
}

function updateContentSizeAndPosition() {
	if (ContentItemType == 'Image') {
		updateImageContentSizeAndPosition();
	}
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers