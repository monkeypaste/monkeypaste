
function initImageContent(itemDataStr) {
	// itemData must remain base64 image string
	quill.enable(false);
	hideAllToolbars();
	enableReadOnly();
	disableSubSelection();

	let img_html = '<p class="ql-align-center"><img class="content-image" src="data:image/png;base64,' + itemDataStr + '"></p>';
	setHtml(img_html);
}

function getImageContentData() {
	if (ContentItemType != 'Image') {
		return null;
	}
	let img_elm = document.getElementsByClassName('content-image')[0]
	let img_data = img_elm.getAttribute('src').replace('data:image/png;base64,', '');
	return img_data;
}