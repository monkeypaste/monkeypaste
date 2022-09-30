var ImgBase64 = '';

function initImageContent(itemDataStr) {
	// itemData must remain base64 image string
	quill.enable(false);
	hideAllToolbars();
	enableReadOnly();
	disableSubSelection();

	ImgBase64 = itemDataStr;

	let img_html = '<p class="ql-align-center"><img class="content-image" src="data:image/png;base64,' + ImgBase64 + '"></p>';
	setHtml(img_html);
}