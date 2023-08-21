// #region Globals
//const globals.CONTENT_CLASS_PREFIX = 'content';
//var globals.ContentClassAttrb = null;

//var globals.ContentHandle = null;
//var globals.ContentItemType = 'Text';


//const globals.InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
//const globals.BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote', 'pre']
//const globals.AllDocumentTags = [...globals.InlineTags, ...globals.BlockTags];

// #endregion Globals

// #region Life Cycle

function loadContent(
	isContentReadOnly,
	isContentSubSelectionEnabled,
	contentHandle,
	contentType,
	contentData,
	searches,
	append_state,
	annotationsJsonStr,
	sel_state,
	paste_button_info) {
	// NOTE only called fromHost (or tester which calls _ext)

	let is_reload = contentHandle == globals.ContentHandle;
	let was_sub_sel_enabled = null;
	let was_editable = null;

	try {
		if (is_reload) {
			was_sub_sel_enabled = isSubSelectionEnabled();
			was_editable = !isReadOnly();
		} else {
			// when actually a new item and not reload
			globals.quill.history.clear();
		}

		globals.quill.enable(true);

		globals.ContentHandle = contentHandle;
		globals.ContentItemType = contentType;

		// set editor content classes
		initContentClassStyle();

		let sel_to_restore = sel_state;

		if (is_reload && !sel_to_restore && !searches) {
			// NOTE1 when content is reloaded, any selection will be lost so save to restore before reloading
			// but rely on host req state cause content is reloading a couple times (shouldn't) during unpin

			// NOTE2 ignore sel state if this is a reload while search is active or cur highlight maybe be out of view
			sel_to_restore = getDocSelection();
		} 
		if (!is_reload) {
			clearTableSelectionStates();
			loadPasteButton();
			resetSelection();
			resetColorPaletteState();

			resetContent();
			resetAnnotations();
		}		 

		deactivateFindReplace(false);

		loadContentData(contentData);

		updateQuill();
		if (!is_reload) {
			// need to wait for content before enabling append
			// or it won't scroll to end (only relevant for !pre state)
			updateAppendModeState(append_state, true);
			if (isContentSubSelectionEnabled) {
				enableSubSelection(true, paste_button_info);
			} else {
				disableSubSelection(true);
			}
			if (isContentReadOnly) {
				enableReadOnly(true);
			} else {
				disableReadOnly(true);
			}
		}

		if (is_reload && append_state == null) {
			// handle special case where query tile append ended
			// so its append state is cleared (was only enabled before popout triggered)
			updateAppendModeState(null, true);
		}
		
		if (globals.ContentItemType != 'Text') {
			globals.quill.enable(false);
		}

		if (globals.ContentItemType == 'Image') {
			// NOTE pass annotations so load after image dimensions are known
			populateContentImageDataSize(annotationsJsonStr);
		} else {
			loadAnnotations(annotationsJsonStr);
		}

		updateAllElements();
		updateQuill();

		loadFindReplace(searches);

		if (sel_to_restore != null) {
			log('restoring selection: ' + JSON.stringify(sel_to_restore));
			// only set sel before component updates do scroll after
			sel_to_restore = cleanDocRange(sel_to_restore);
			setDocSelection(sel_to_restore.index, sel_to_restore.length, 'silent');

			scrollToSelState(sel_to_restore);
		} 

	} catch (ex) {
		onException_ntf('error loading item ', ex);
	}

	try {
		let msg = getContentAsMessage();
		onContentLoaded_ntf(msg);
	} catch (ex) {
		onException_ntf('error creating load item resp, sending empty ', ex);
		onContentLoaded_ntf('');
	}

	if (was_sub_sel_enabled != null &&
		was_sub_sel_enabled) {
		//retain focus state on reload (null when not reload)
		enableSubSelection(false, paste_button_info);
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

function initContentClassStyle() {
	if (globals.ContentItemType == 'Text') {
		getEditorContainerElement().classList.add('text-content');
		getEditorContainerElement().classList.remove('image-content');
		getEditorContainerElement().classList.remove('file-list-content');
		return;
	}
	if (globals.ContentItemType == 'Image') {
		getEditorContainerElement().classList.remove('text-content');
		getEditorContainerElement().classList.add('image-content');
		getEditorContainerElement().classList.remove('file-list-content');
		return;
	}
	if (globals.ContentItemType == 'FileList') {
		getEditorContainerElement().classList.remove('text-content');
		getEditorContainerElement().classList.remove('image-content');
		getEditorContainerElement().classList.add('file-list-content');
		return;
	} 

	getEditorContainerElement().classList.remove('text-content');
	getEditorContainerElement().classList.remove('image-content');
	getEditorContainerElement().classList.remove('file-list-content');
}
function initContentClassAttributes() {
	const Parchment = Quill.imports.parchment;
	let suppressWarning = true;
	let config = {
		scope: Parchment.Scope.ANY,
	};
	globals.ContentClassAttrb = new Parchment.ClassAttributor('contentType',globals.CONTENT_CLASS_PREFIX, config);

	Quill.register(globals.LinkTypeAttrb, suppressWarning);

}
// #endregion Life Cycle

// #region Getters

function getContentHandle() {
	return globals.ContentHandle;
}

function getContentAsMessage() {
	updateQuill();
	return {
		contentHeight: getContentHeight(),
		editorWidth: getEditorWidth(),
		editorHeight: getEditorHeight(),
		itemData: getContentData(),
		itemSize1: parseInt_safe(getContentWidthByType()),
		itemSize2: parseInt_safe(getContentHeightByType()),
		hasTemplates: hasTemplates(),
		hasEditableTable: hasEditableTable()
	};
}

function getContentData() {
	if (globals.ContentItemType == 'Text') {
		return getTextContentData();
	}
	if (globals.ContentItemType == 'Image') {
		return getImageContentData();
	}
	if (globals.ContentItemType == 'FileList') {
		return getFileListContentData();
	}
	return '';
}

function getEncodedContentText(range) {
	if (globals.ContentItemType == 'Text') {
		return getEncodedTextContentText(range);
	}
	if (globals.ContentItemType == 'Image') {
		return getEncodedImageContentText(range);
	}
	if (globals.ContentItemType == 'FileList') {
		return getEncodedFileListContentText(range);
	}
	return '';
}

function getDecodedContentText(encodedText) {
	if (globals.ContentItemType == 'Text') {
		return getDecodedTextContentText(encodedText);
	}
	if (globals.ContentItemType == 'Image') {
		return getDecodedImageContentText(encodedText);
	}
	if (globals.ContentItemType == 'FileList') {
		return getDecodedFileListContentText(encodedText);
	}
	return '';
}

function getContentBg(htmlStr, contrast_opacity = 0.5) {
	if (globals.ContentItemType != 'Text') {
		return cleanColor();
	}

	let html_doc = globals.DomParser.parseFromString(htmlStr, 'text/html');
	let elms = html_doc.querySelectorAll(globals.InlineTags.join(", ") + ',' + globals.BlockTags.join(','));

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
	var bounds = globals.quill.getBounds(0, getDocLength());
	bounds = cleanRect(bounds);
	return parseFloat(bounds.width);
}

function getContentHeight() {
	var bounds = globals.quill.getBounds(0, getDocLength());
	bounds = cleanRect(bounds);
	return parseFloat(bounds.height);
}


function getContentWidthByType() {
	if (globals.ContentItemType == 'Text') {
		return getTextContentCharCount();
	}
	if (globals.ContentItemType == 'FileList') {
		return getTotalFileSize();
	}
	if (globals.ContentItemType == 'Image') {
		return getImageContentWidth();
	}
}

function getContentHeightByType() {
	if (globals.ContentItemType == 'Text') {
		return getTextContenLineCount();
	}
	if (globals.ContentItemType == 'FileList') {
		return getFileCount();
	}
	if (globals.ContentItemType == 'Image') {
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
	//return globals.ContentItemType != 'Image';
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
	return globals.ContentItemType == 'Text';
}
// #endregion State

// #region Actions

function transferContent(dt, source_doc_range, dest_doc_range, source) {
	let result = null;
	switch (globals.ContentItemType) {
		case 'Text':
			result = transferTextContent(dt, source_doc_range, dest_doc_range, source);
			break;
		case 'FileList':
			result = transferFileListContent(dt, source_doc_range, dest_doc_range, source);
			break;
	}
	return result;
}
function convertContentToFormats(selectionOnly, formats) {
	updateQuill();
	let items = null;
	if (globals.ContentItemType == 'Text') {
		items = convertTextContentToFormats(selectionOnly, formats);
	} else if (globals.ContentItemType == 'FileList') {
		items = convertFileListContentToFormats(selectionOnly, formats);
	} else if (globals.ContentItemType == 'Image') {
		items = convertImageContentToFormats(selectionOnly, formats);
	}
	return items;
}

function appendContentData(data) {
	if (isNullOrEmpty(data)) {
		return;
	}
	if (globals.ContentItemType == 'Text') {
		appendTextContentData(data);
	} else if (globals.ContentItemType == 'FileList') {
		appendFileListContentData(data);
	} else if (globals.ContentItemType == 'Image') {
		appendImageContentData(data);
	}
}

function loadContentData(contentData) {
	// enusre globals.IsLoaded is false so msg'ing doesn't get clogged up
	setEditorIsLoaded(false);

	if (globals.ContentItemType == 'Image') {
		loadImageContent(contentData);
	} else if (globals.ContentItemType == 'FileList') {
		loadFileListContent(contentData);
	} else if (globals.ContentItemType == 'Text') {
		loadTextContent(contentData);
	}

	setEditorIsLoaded(true);
}

function updateContentSizeAndPosition() {
	if (globals.ContentItemType == 'Image') {
		updateImageContentSizeAndPosition();
		return;
	}
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers