// #region Globals
var FileListItems = [];
// #endregion Globals

// #region Life Cycle

function loadFileListContent(itemDataStr) {
	// item data is MpQuillFileListDataFragment

	// itemData must remain file-paths separated by new-line
	hideAllToolbars();
	enableReadOnly();
	disableSubSelection();
	FileListItems = [];
	//ContentData = '';
	let fldfObj = toJsonObjFromBase64Str(itemDataStr);
	for (var i = 0; i < fldfObj.fileItems.length; i++) {
		let flif = fldfObj.fileItems[i];
		FileListItems.push(flif);
		//ContentData = flif.filePath + envNewLine();
	}
	createFileList();
	quill.enable(false);
}

// #endregion Life Cycle

// #region Getters

function getTableItemIdentifier(prefix) {
	let chars = 'abcdefghijklmnopqrstuvwxyz0123456789';
	let id = '';
	for (let i = 0; i < 4; i++) {
		id += chars[getRandomInt(chars.length - 1)];
	}
	return prefix + '-' + id;
}

function getFileListTableDivElement() {
	let flc_elm = document.getElementById('fileListTableDiv');
	return flc_elm;
}


function getFileListContentData() {
	let paths_str = '';
	for (var i = 0; i < FileListItems.length; i++) {
		paths_str += FileListItems[i].filePath;
		if (i < FileListItems.length - 1) {
			paths_str += envNewLine();
		}
	}
	return paths_str;
}

function getTotalFileSize() {
	return '';
}

function getFileCount() {
	return FileListItems.length;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function createFileList() {
	let file_list_tbody_inner_html = '';
	for (var i = 0; i < FileListItems.length; i++) {
		let row_id = getTableItemIdentifier('row');
		let file_item_tr_outer_html =
			'<tr class="file-list-row" data-row="' + row_id + '">' +
			'<td class="file-list-cell" data-row="' + row_id + '" rowspan="1" colspan="1">' +
			'<p class="qlbt-cell-line" data-row="' + row_id + '" data-cell="' + getTableItemIdentifier('cell') + '" data-rowspan="1" data-colspan="1">' +
			'<img class="file-list-icon" src="data:image/png;base64,' + FileListItems[i].fileIconBase64 + '">' +
			'</p></td>' +
			'<td class="file-list-cell" data-row="' + row_id + '" rowspan="1" colspan="1">' +
			'<p class="qlbt-cell-line ql-align-right" data-row="' + row_id + '" data-cell="' + getTableItemIdentifier('cell') + '" data-rowspan="1" data-colspan="1">' +
			'<span class="file-list-path ql-font-consolas ql-align-right">' + formatFilePathDisplayValue(FileListItems[i].filePath) + '</span>' +
			'</p></td>';
		file_list_tbody_inner_html += file_item_tr_outer_html;

	}
	let file_list_table_html =
		'<div id="fileListTableDiv" class="quill-better-table-wrapper">' +
		'<table class="quill-better-table file-list-table">' +
		'<colgroup><col class="file-list-icon-col"><col class="file-list-path-col"></colgroup>' +
		'<tbody>' + file_list_tbody_inner_html + '</tbody></table></div>';

	setRootHtml(file_list_table_html);
}

function formatFilePathDisplayValue(fp) {
	let fp_parts = [];
	if (fp.includes('\\')) {
		fp_parts = fp.split('\\');
	} else if (fp.includes('/')) {
		fp_parts = fp.split('/');
	} else {
		fp_parts.push(fp);
	}

	return fp_parts[fp_parts.length - 1];
}


function convertFileListContentToFormats(isForOle, formats) {
	// NOTE (at least currently) selection is ignored for file items
	let items = [];
	for (var i = 0; i < formats.length; i++) {
		let format = formats[i];
		let data = null;
		if (isHtmlFormat(format)) {
			data = getHtml();
			if (isForOle && format.toLowerCase() == 'html format') {
				// NOTE web html doesn't use fragment format
				data = createHtmlClipboardFragment(htmlStr, sel);
			} else {
				data = htmlStr;
			}
		} else if (isPlainTextFormat(format)) {
			data = getFileListContentData();
		} else if (isImageFormat(format)) {
			// trigger async screenshot notification where host needs 
			// to null and wait for value to avoid async issues
			onCreateContentScreenShot_ntf();
			data = 'pending...';
		} else if (isCsvFormat(format)) {
			data = getFileListContentData().join(',');
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
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers