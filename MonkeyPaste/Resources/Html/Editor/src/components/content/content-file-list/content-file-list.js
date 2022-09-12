var FileListItems = [];

function initFileListContent(itemDataStr) {
	// item data is MpQuillFileListDataFragment

	// itemData must remain file-paths separated by new-line

	hideEditorAndAllToolbars();
	setupEmptyFileList();
	FileListItems = [];

	let fldfObj = toJsonObjFromBase64Str(itemDataStr);

	fldfObj.fileItems.forEach((flif, idx) => {
		addFileListItem(flif.filePath,flif.fileIconBase64, idx);
	});
}

function addFileListItem(fp,fi) {
	if (!fp || fp.trim == '') {
		log('insertFileListItem error fp is empty, ignoring item');
	}

	getFileListContainerElement().innerHTML += getFileListRootItemElement().outerHTML;
	let fli_elm = getFileListContainerElement().lastChild;
	fli_elm.style.display = 'flex';
	fli_elm.classList.remove('hidden');
	setFileListItemIcon(fli_elm, fi);
	setFileListItemFilePath(fli_elm, fp);
	FileListItems.push(fp);
}

function setFileListItemIcon(fli_elm, iconBase64) {
	let icon_img_elm = getFileListItemIconImageElement(fli_elm);
	icon_img_elm.setAttribute('src', 'data:image/png;base64,' + iconBase64);
}

function getFileListItemIconImageElement(fli_elm) {
	if (!fli_elm) {
		return null;
	}
	var icon_elm = fli_elm.getElementsByTagName('img')[0];
	return icon_elm;
}

function setFileListItemFilePath(fli_elm,fp) {
	if (!fli_elm) {
		return null;
	}
	var span_elm = getFileListItemFilePathSpanElement(fli_elm);
	span_elm.innerText = formatFilePathDisplayValue(fp);
}

function getFileListItemFilePathSpanElement(fli_elm) {
	if (!fli_elm) {
		return null;
	}
	var span_elm = fli_elm.getElementsByTagName('span')[0];
	return span_elm;
}

function setupEmptyFileList() {
	getFileListContainerElement().classList.remove('hidden');
	getFileListRootItemElement().style.display = 'none';

	let flc_children = getFileListContainerElement().children;
	let children_to_remove = flc_children.length - 1;
	while (children_to_remove > 0) {
		let to_remove = flc_children[flc_children.length - 1]
		getFileListContainerElement().removeChild(to_remove);
		children_to_remove--;
	}
}
function disableFileList() {
	FileListItems = [];
	hideFileList();
}
function hideFileList() {
	getFileListContainerElement().classList.add('hidden');
}

function formatFilePathDisplayValue(fp) {
	//let max_length = 10;
	//let s_idx = Math.max(0, fp.length - max_length - 1);
	//let dv = substringByLength(s_idx, max_length);
	let fp_parts = fp.split('\\');

	return fp_parts[fp_parts.length - 1];
}

function getFileListRootItemElement() {
	let flri_elm = document.getElementById('fileListRootItem');
	return flri_elm;
}

function getFileListContainerElement() {
	let flc_elm = document.getElementById('fileListContainer');
	return flc_elm;
}