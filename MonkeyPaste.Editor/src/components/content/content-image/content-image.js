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
	// TODO need to test still if images are being scaled on copy but definitely need to calculate differently
	return getContentWidth();
}
function getImageContentHeight() {
	return getContentHeight();
}

function getContentImageDataSize() {
	if (ContentImageWidth == -1 &&
		ContentImageHeight == -1) {
		ContentImageWidth = 0;
		ContentImageHeight = 0;

		let tmp = document.createElement('img');
		tmp.onload = function (e) {
			ContentImageWidth = tmp.width;
			ContentImageHeight = tmp.height;
			//document.body.removeChild(tmp);
			log('img size w: ' + ContentImageWidth + ' h: ' + ContentImageHeight);
		}
		tmp.setAttribute('src', 'data:image/png;base64,' + getContentData());
		//document.body.appendChild(tmp);
	}
	// avoid divide by zero
	return {
		width: Math.max(1,ContentImageWidth),
		height: Math.max(1,ContentImageHeight)
	};
}
function getImageDataHeight() {
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