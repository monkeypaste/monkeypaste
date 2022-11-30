// #region Globals

// #endregion Globals

// #region Life Cycle
function loadImageContent(itemDataStr) {
	// itemData must remain base64 image string
	hideAllToolbars();
	enableReadOnly();
	disableSubSelection();

	let img_html = '<p class="ql-align-center"><img class="content-image" src="data:image/png;base64,' + itemDataStr + '"></p>';
	setRootHtml(img_html);
}
// #endregion Life Cycle

// #region Getters
function getImageContentWidth() {
	// TODO need to test still if images are being scaled on copy but definitely need to calculate differently
	return getContentWidth();
}
function getImageContentHeight() {
	return getContentHeight();
}

function getImageContentData() {
	if (ContentItemType != 'Image') {
		return null;
	}
	let img_elm = document.getElementsByClassName('content-image')[0]
	let img_data = img_elm.getAttribute('src').replace('data:image/png;base64,', '');
	return img_data;
}

function getEncodedImageContentText() {
	// TODO could return analytic result or ascii here
	return '';
}
function getDecodedImageContentText(encoded_text) {
	return encoded_text;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions
function convertImageContentToFormats(isForOle, formats) {
	// NOTE (at least currently) selection is ignored for file items
	let items = [];
	for (var i = 0; i < formats.length; i++) {
		let format = formats[i];
		let data = null;
		if (isHtmlFormat(format)) {
			data = getHtml();
			if (format.toLowerCase() == 'html format') {
				// NOTE web html doesn't use fragment format
				data = createHtmlClipboardFragment(data);
			} 
		} else if (isPlainTextFormat(format)) {
			// handled by host
		} else if (isImageFormat(format)) {
			// handled by host
		} else if (isCsvFormat(format)) {
			// handled by host
		}
		if (!data || data == '') {
			continue;
		}
		let item = {
			format: format,
			data: data
		};
		items.push(item);
	}
	return items;
}
function appendImageContentData(data) {
	return;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers