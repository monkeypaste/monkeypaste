// #region Globals

// #endregion Globals

// #region Life Cycle

function loadFileListContent(itemDataStr) {
	// item data is MpQuillFileListDataFragment

	// itemData must remain file-paths separated by new-line

	if (globals.FileListClassAttrb == null) {
		initFileListClassAttrb();
	}

	hideAllToolbars();
	disableTableContextMenu();
	disableTableInteraction();
	enableReadOnly();
	disableSubSelection();
	globals.FileListItems = [];
	let fldfObj = toJsonObjFromBase64Str(itemDataStr);
	for (var i = 0; i < fldfObj.fileItems.length; i++) {
		let flif = fldfObj.fileItems[i];
		globals.FileListItems.push(flif);
	}
	createFileList();

	loadLinkHandlers();	
}

function initFileListClassAttrb() {
	const Parchment = Quill.imports.parchment;
	let suppressWarning = true;
	let config = {
		scope: Parchment.Scope.ANY,
	};
	globals.FileListClassAttrb = new Parchment.ClassAttributor('fileList', 'file-list', config);

	Quill.register(globals.FileListClassAttrb, suppressWarning);
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
function getEncodedFileListContentText() {
	return getFileListContentData();
}

function getDecodedFileListContentText(encoded_text) {
	return encoded_text;
}

function getFileListContentData(isForOle) {
	let paths_str = '';
	const sel_idxs = isForOle && getSelectedFileItemIdxs().length > 0 ? getSelectedFileItemIdxs() : null;
	for (var i = 0; i < globals.FileListItems.length; i++) {
		if (sel_idxs && !sel_idxs.includes(i)) {
			continue;
		}
		paths_str += globals.FileListItems[i].filePath;
		if (i < globals.FileListItems.length - 1) {
			paths_str += globals.DefaultCsvProps.RowSeparator;
		}
	}
	return paths_str;
}

function getFileListItemDocRange(fliIdx) {
	const doc_len = getDocLength();
	let fli_doc_range = null;
	for (var i = 0; i < doc_len; i++) {
		const idx_rc = getTableCellAtDocIdx(i);
		if (idx_rc == null) {
			continue;
		}
		if (idx_rc[0] == fliIdx) {
			if (fli_doc_range == null) {
				fli_doc_range = { index: i, length: 0 };
			} else {
				fli_doc_range.length = i - fli_doc_range.index;
			}
		} else if (idx_rc[0] > fliIdx) {
			break;
		}
	}
	return fli_doc_range;
}

function getSelectedFileItemIdxs() {
	const sel_fli_elms = getSelectedFileListRowElements();
	const fli_elms = getFileListRowElements();
	let idxs = [];
	for (var i = 0; i < fli_elms.length; i++) {
		if (sel_fli_elms.includes(fli_elms[i])) {
			idxs.push(i);
		}
	}
	return idxs;
}

function getSelectedFileListRowElements() {
	return getFileListRowElements().filter(x => x.classList.contains('selected'));
}

function getFileListRowElements() {
	return Array.from(document.querySelectorAll('tr'));
}

function getTotalFileSize() {
	return '';
}

function getFileCount() {
	return globals.FileListItems ? globals.FileListItems.length : 0;
}

function getPathUri(path) {
	let uri = null;
	try {
		uri = new URL(path);
		if (uri) {
			return uri.href;
		}
	} catch (ex) {
		log('Exception creating uri for path: ' + path);
		log(ex);
	}
	return '';
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isMultiFileSelectEnabled() {
	if (isAnyAppendEnabled()) {
		return false;
	}
	return isSubSelectionEnabled();
}
// #endregion State

// #region Actions

function createFileList() {
	let file_list_tbody_inner_html = '';
	for (var i = 0; i < globals.FileListItems.length; i++) {
		let row_id = getTableItemIdentifier('row');
		let fp = globals.FileListItems[i].filePath;
		let fp_icon = globals.FileListItems[i].fileIconBase64;
		let file_item_tr_outer_html =
			'<tr class="file-list-row" data-row="' + row_id + '">' +
			// ICON COLUMN
			'<td class="file-list-cell" data-row="' + row_id + '" rowspan="1" colspan="1">' +
			'<p class="qlbt-cell-line" data-row="' + row_id + '" data-cell="' + getTableItemIdentifier('cell') + '" data-rowspan="1" data-colspan="1">' +
			'<img class="file-list-icon" src="data:image/png;base64,' + fp_icon + '">' +
			'</p></td>' +
			// PATH COLUMN
			'<td class="file-list-cell" data-row="' + row_id + '" rowspan="1" colspan="1">' +
			'<p class="qlbt-cell-line ql-align-right" data-row="' + row_id + '" data-cell="' + getTableItemIdentifier('cell') + '" data-rowspan="1" data-colspan="1">' +
			`<a class="link-type-fileorfolder file-list-path ql-font-consolas ql-align-right" href="${getPathUri(fp)}">${formatFilePathDisplayValue(fp)}</a>` +
			'</p></td>' +
			// REMOVE COLUMN
			'<td class="file-list-cell" data-row="' + row_id + '" rowspan="1" colspan="1">' +
			'<p class="qlbt-cell-line ql-align-center" data-row="' + row_id + '" data-cell="' + getTableItemIdentifier('cell') + '" data-rowspan="1" data-colspan="1">' +
			'<a class="link-type-delete-item file-list-remove ql-align-center">x</span>' +
			'</p></td>' +
			'</tr>';
		file_list_tbody_inner_html += file_item_tr_outer_html;

	}
	let file_list_table_html =
		'<div id="fileListTableDiv" class="quill-better-table-wrapper">' +
		'<table class="quill-better-table file-list-table">' +
		'<colgroup><col class="file-list-icon-col"><col class="file-list-path-col"><col class="file-list-remove-col"></colgroup>' +
		'<tbody>' + file_list_tbody_inner_html + '</tbody></table></div>';

	setRootHtml(file_list_table_html);
	addFileEventHandlers();
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
		let lwc_format = formats[i].toLowerCase();
		let data = null;
		if (isHtmlFormat(lwc_format)) {
			// BUG this ignores selected items cause its confusing and won't really be needed
			data = getHtml();
			if (lwc_format == 'html format') {
				// NOTE web html doesn't use fragment format
				data = createHtmlClipboardFragment(data);
			}
		} else if (isPlainTextFormat(lwc_format)) {
			data = getFileListContentData(isForOle);
		} else if (isImageFormat(lwc_format)) {
			// BUG this ignores selected items cause its confusing and won't really be needed

			// trigger async screenshot notification where host needs 
			// to null and wait for value to avoid async issues
			getDocRangeAsImageAsync(getContentRange())
				.then((result) => {
					onCreateContentScreenShot_ntf(result);
				});
			data = globals.PLACEHOLDER_DATAOBJECT_TEXT;
		} else if (isCsvFormat(lwc_format)) {
			data = getFileListContentData(isForOle).split(globals.DefaultCsvProps.RowSeparator).join(',');
		} else if (lwc_format == 'filenames' ||
					lwc_format == 'filedrop') {
			// need to provide if partial selection and text is not included
			// only send idx's to avoid converting uri's in host
			data = getFileListContentData(isForOle);
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

function addFileEventHandlers() {
	const fli_row_elms = getFileListRowElements();
	for (var i = 0; i < fli_row_elms.length; i++) {
		const fli_row_elm = fli_row_elms[i];
		fli_row_elm.addEventListener('click', onFileListItemRowClick);
	}
}

function appendFileListContentData(data) {
	// input 'MpQuillFileListDataFragment'
	const append_doc_range = getAppendDocRange();
	const append_cell = getTableCellAtDocIdx(append_doc_range.index);
	let insert_idx = -1;
	if (append_doc_range.mode == 'pre') {
		if (globals.IsAppendManualMode) {
			insert_idx = append_cell[0];
		} else {
			insert_idx = Math.max(0, append_cell[0] - 1);
		}
	} else {
		insert_idx = append_cell[0] + 1;
	}
	const cur_sel_idxs = getSelectedFileItemIdxs();
	let updated_sel_idxs = [];
	let append_items = toJsonObjFromBase64Str(data).fileItems;
	let updated_items = [];
	for (var i = 0; i < globals.FileListItems.length; i++) {
		if (i == insert_idx) {
			for (var j = 0; j < append_items.length; j++) {
				if (!globals.IsAppendPreMode) {
					updated_sel_idxs.push(i + j);
				}
				updated_items.push(append_items[j]);
			}
		}
		if (globals.IsAppendPreMode && cur_sel_idxs.includes(i)) {
			updated_sel_idxs.push(updated_items.length);
		}
		updated_items.push(globals.FileListItems[i]);
	}
	if (insert_idx == globals.FileListItems.length) {
		for (var j = 0; j < append_items.length; j++) {
			if (!globals.IsAppendPreMode) {
				updated_sel_idxs.push(updated_items.length);
			}
			updated_items.push(append_items[j]);
		}
	}

	loadFileListContent(toBase64FromJsonObj({ fileItems: updated_items }));
	updated_sel_idxs.forEach(x => getFileListRowElements()[x].classList.add('selected'));

	scrollToAppendIdx();
	onContentChanged_ntf();
}

function excludeRowByAnchorElement(a_elm) {
	if (!a_elm) {
		return;
	}
	let row_elm = getAncestorByTagName(a_elm, 'tr');
	if (!row_elm) {
		return;
	}
	
	let row_idx = getFileListRowElements().indexOf(row_elm);
	if (row_idx < 0) {
		return;
	}
	// NOTE remove item before content change so host receives updated list
	globals.FileListItems.splice(row_idx, 1);

	globals.quill.enable(true);

	let btm = getBetterTableModule(true);
	let tableBlot = quillFindBlot(getTableElements()[0].firstChild);
	const row_boundary = btm.tableSelection.boundary;
	tableBlot.deleteRow(row_boundary, getEditorContainerElement());
	updateQuill();

	globals.quill.enable(false);

	//createFileList();

	//onContentChanged_ntf();
}
// #endregion Actions

// #region Event Handlers

function onFileListItemRowClick(e) {
	if (!isSubSelectionEnabled()) {
		return;
	}
	if (isMultiFileSelectEnabled()) {
		e.currentTarget.classList.toggle('selected');
		return;
	}

	const fli_elms = getFileListRowElements();
	for (var i = 0; i < fli_elms.length; i++) {
		const fli_elm = fli_elms[i];
		if (fli_elm == e.currentTarget) {
			fli_elm.classList.add('selected');
		} else {
			fli_elm.classList.remove('selected');
		}
	}
}

// #endregion Event Handlers