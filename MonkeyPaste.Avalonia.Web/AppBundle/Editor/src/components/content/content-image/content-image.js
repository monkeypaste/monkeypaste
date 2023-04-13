// #region Globals

const CONTENT_IMAGE_CLASS = 'content-image';

var ContentImageWidth = -1;
var ContentImageHeight = -1;

// #endregion Globals

// #region Life Cycle
function loadImageContent(itemDataStr) {
	// itemData must remain base64 image string
	hideAllToolbars();
	enableReadOnly();
	disableSubSelection();

	ContentImageWidth = -1;
	ContentImageHeight = -1;

	let img = document.createElement('img');
	img.classList.add('content-image');
	img.setAttribute('src', `data:image/png;base64,${itemDataStr}`);	

	let p = document.createElement('p');
	p.classList.add('ql-align-center');
	p.appendChild(img);

	//let img_html = `<p class="ql-align-center"><img src="data:image/png;base64,${itemDataStr}"></p>`;
	//setRootHtml(img_html);
	setRootHtml('');
	getEditorElement().appendChild(p);

	ContentClassAttrb.add(getEditorElement().firstChild.firstChild, 'image');
	updateImageContentSizeAndPosition();

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
	if (ContentImageWidth < 0) {
		log('WARNING! image size not populated, using fallback width...');
		return getContentWidth();
	}
	return ContentImageWidth;
}
function getImageContentHeight() {
	if (ContentImageHeight < 0) {
		log('WARNING! image size not populated, using fallback height...');
		return getContentHeight();
	}
	return ContentImageHeight;
}

function populateContentImageDataSize(annotationsJsonStr, is_reload) {
	if (ContentImageWidth >= 0 &&
		ContentImageHeight >= 0) {
		log('image size already populated, ignoring request');
		return;
	}

	ContentImageWidth = 0;
	ContentImageHeight = 0;

	let tmp = document.createElement('img');
	tmp.onload = function (e) {
		ContentImageWidth = tmp.width;
		ContentImageHeight = tmp.height;

		loadAnnotations(annotationsJsonStr);
		log('img size w: ' + ContentImageWidth + ' h: ' + ContentImageHeight);
	}
	tmp.setAttribute('src', 'data:image/png;base64,' + getContentData());
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

function resetContentImage() {
	ContentImageWidth = -1;
	ContentImageHeight = -1;
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
			format: lwc_format,
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