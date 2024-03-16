
// #region Life Cycle
function loadImageContentAsync(itemDataStr, annotationsJsonStr) {
	// itemData must remain base64 image string
	hideAllToolbars();
	enableReadOnly();
	disableSubSelection();

	globals.ContentImageNaturalWidth = -1;
	globals.ContentImageNaturalHeight = -1;

	let img = document.createElement('img');
	img.classList.add('content-image');
	img.setAttribute('src', `data:image/png;base64,${itemDataStr}`);
	img.onload = function (e) {
		globals.ContentImageNaturalWidth = img.naturalWidth;
		globals.ContentImageNaturalHeight = img.naturalHeight;

		onContentImageLoaded_ntf(globals.ContentImageNaturalWidth, globals.ContentImageNaturalHeight);
	}


	let p = document.createElement('p');
	p.classList.add('ql-align-center');
	p.appendChild(img);

	setRootHtml('');
	getEditorElement().replaceChild(p, getEditorElement().firstChild);

	globals.ContentClassAttrb.add(getEditorElement().firstChild.firstChild, 'image');
	updateImageContentSizeAndPosition();
	updateQuill();

	//while (!isContentImageDimsSet()) {
		//	await delay(100);
	//}
	//log('loaded: ' + isContentImageDimsSet());
}
// #endregion Life Cycle

// #region Getters

function getContentImageElement() {
	let imgl = Array.from(getEditorElement().getElementsByClassName('content-image'));
	if (!imgl || imgl.length < 1) {
		return null;
	}
	return imgl[0];
}

function getImageContentWidth() {
	if (globals.ContentImageNaturalWidth < 0) {
		log('WARNING! image size not populated, using fallback width...');
		return getContentWidth();
	}
	return globals.ContentImageNaturalWidth;
}
function getImageContentHeight() {
	if (globals.ContentImageNaturalHeight < 0) {
		log('WARNING! image size not populated, using fallback height...');
		return getContentHeight();
	}
	return globals.ContentImageNaturalHeight;
}

function getImageContentData() {
	if (globals.ContentItemType != 'Image') {
		return null;
	}
	let img_elms = document.getElementsByClassName('content-image');
	if (img_elms.length == 0) {
		return null;
	}
	let img_elm = img_elms[0]
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

function resetContentImage() {
	globals.ContentImageNaturalWidth = -1;
	globals.ContentImageNaturalHeight = -1;
}

function isContentImageDimsSet() {
	if (globals.ContentImageNaturalWidth > 0 &&
		globals.ContentImageNaturalHeight > 0) {
		return true;
	}
	return false;
}

// #endregion State

// #region Actions

function convertImageContentToFormats(isForOle, formats) {
	// NOTE (at least currently) selection is ignored for file items
	let items = [];
	for (var i = 0; i < formats.length; i++) {
		let lwc_format = formats[i].toLowerCase();
		let data = null;
		if (isHtmlFormat(lwc_format)) {
			data = getHtml();
			if (lwc_format == 'html format') {
				// NOTE web html doesn't use fragment format
				data = createHtmlClipboardFragment(data);
			} 
		} else if (isPlainTextFormat(lwc_format)) {
			// handled by host
		} else if (isImageFormat(lwc_format)) {
			// handled by host
		} else if (isCsvFormat(lwc_format)) {
			// handled by host
		}
		if (!data || data == '') {
			continue;
		}
		let item = {
			format: formats[i],
			data: data
		};
		items.push(item);
	}
	return items;
}

function appendImageContentData(data) {
	return;
}

function updateImageContentSizeAndPosition() {
	return;
	let img_elm = getContentImageElement();
	if (!img_elm) {
		return;
	}
	// NOTE when using 100% for max dim the editor just overflows and container isn't accounted for
	let ecw = getEditorContainerRect().width;
	let ech = getEditorContainerRect().height;

	setElementComputedStyleProp(img_elm, 'max-width', `${ecw}px`);
	setElementComputedStyleProp(img_elm, 'max-height', `${ech}px`);
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers