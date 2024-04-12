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

	if (!isAnyAppendEnabled()) {
		enableReadOnly();
		disableSubSelection();
	}
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
	globals.FileListClassAttrb = registerClassAttributor('fileList', 'file-list', globals.Parchment.Scope.ANY);
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

function getFileListContentData(selectedOnly) {
	let paths = [];
	const sel_idxs = selectedOnly && getSelectedFileItemIdxs().length > 0 ? getSelectedFileItemIdxs() : null;
	for (var i = 0; i < globals.FileListItems.length; i++) {
		if (sel_idxs && !sel_idxs.includes(i)) {
			continue;
		}
		paths.push(globals.FileListItems[i].filePath);
	}
	let paths_str = paths.join(envNewLine());
	return paths_str;
}

function getFileListItemIdxContainingDocIdx(docIdx) {
	for (var i = 0; i < getFileCount(); i++) {
		let fli_range = getFileListItemDocRange(i);
		if (isDocIdxInRange(docIdx, fli_range)) {
			return i;
		}
	}
	// when out of range assume its lasst item
	return getFileCount() - 1;
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
function getFileListDeleteCellElements() {
	return Array.from(document.querySelectorAll('.file-list-remove-cell'));
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


// #region FileItem Create
function createFileList() {
	setRootElement(createFileListTableElement());
	addFileEventHandlers();
}
function createFileListTableElement() {
	// CONTAINER
	let cnt_elm = document.createElement('div');
	cnt_elm.setAttribute('id', 'fileListTableDiv');
	cnt_elm.classList.add('quill-better-table-wrapper');

	// TABLE
	let tbl_elm = document.createElement('table');
	cnt_elm.appendChild(tbl_elm);
	tbl_elm.classList.add('quill-better-table', 'file-list-table');

	// COLGROUPS
	let cg_elm = document.createElement('colgroup');
	tbl_elm.appendChild(cg_elm);
	['file-list-icon-col', 'file-list-path-col', 'file-list-remove-col']
		.forEach(x => {
			let col_elm = document.createElement('col');
			col_elm.classList.add(x);
			cg_elm.appendChild(col_elm);
		});

	// TABLE BODY
	let tb_elm = document.createElement('tbody');
	tbl_elm.appendChild(tb_elm);

	// ROWS
	for (var i = 0; i < globals.FileListItems.length; i++) {
		let fp = globals.FileListItems[i].filePath;
		let fp_icon = globals.FileListItems[i].fileIconBase64;
		tbl_elm.appendChild(createFileListRowElement(fp, fp_icon));
	}

	return cnt_elm;
}

function createFileListRowElement(fp, fp_icon) {
	let row_id = getTableItemIdentifier('row');

	// ROW
	let tr_elm = document.createElement('tr');
	tr_elm.classList.add('file-list-row');
	tr_elm.setAttribute('data-row', row_id);

	// ICON
	tr_elm.appendChild(createFileListIconCell(row_id, fp_icon));

	// PATH
	tr_elm.appendChild(createFileListPathCell(row_id, fp));

	// REMOVE
	tr_elm.appendChild(createFileListRemoveCell(row_id));

	return tr_elm;
} 

function createFileListIconCell(row_id, fp_icon) {
	// CELL
	let td_icon_elm = createFileListCellElement(row_id);

	// PARAGRAPH
	let p_elm = createFileListParagraphElement(row_id);
	td_icon_elm.appendChild(p_elm);

	// ICON
	let img_elm = document.createElement('img');
	p_elm.appendChild(img_elm);
	img_elm.classList.add('file-list-icon');
	img_elm.setAttribute('src', `${globals.BASE64_IMG_SRC_PREFIX}${fp_icon}`);

	return td_icon_elm;
}
function createFileListPathCell(row_id, fp) {
	// CELL
	let td_path_elm = createFileListCellElement(row_id);

	// PARAGRAPH
	let p_elm = createFileListParagraphElement(row_id);
	td_path_elm.appendChild(p_elm);
	p_elm.classList.add('ql-align-right', 'file-list-path-block');

	// PATH ANCHOR
	let a_elm = document.createElement('a');
	p_elm.appendChild(a_elm);
	a_elm.classList.add('link-type-fileorfolder', 'file-list-path', 'ql-font-consolas', 'ql-align-right');
	a_elm.innerText = formatFilePathDisplayValue(fp);

	return td_path_elm;
}


function createFileListRemoveCell(row_id) {
	// CELL
	let td_remove_elm = createFileListCellElement(row_id);
	td_remove_elm.classList.add('file-list-remove-cell');

	// PARAGRAPH
	let p_elm = createFileListParagraphElement(row_id);
	td_remove_elm.appendChild(p_elm);
	p_elm.classList.add('ql-align-center')

	// REMOVE ANCHOR
	let a_elm = document.createElement('a');
	p_elm.appendChild(a_elm);
	a_elm.classList.add('link-type-delete-item', 'file-list-remove', 'ql-align-center');

	// REMOVE ICON
	let img_elm = document.createElement('img');
	a_elm.appendChild(img_elm);
	img_elm.setAttribute('src', `${globals.BASE64_IMG_SRC_PREFIX}${globals.BASE64_IMG_REMOVE}`);

	return td_remove_elm;
}

function createFileListCellElement(row_id) {
	let cell_elm = document.createElement('td');
	cell_elm.classList.add('file-list-cell');
	cell_elm.setAttribute('data-row', row_id);
	cell_elm.setAttribute('rowspan', 1);
	cell_elm.setAttribute('colspan', 1);
	return cell_elm;
}

function createFileListParagraphElement(row_id) {
	let p_elm = document.createElement('p');
	p_elm.classList.add('qlbt-cell-line');
	p_elm.setAttribute('data-row', row_id);
	p_elm.setAttribute('data-cell', getTableItemIdentifier('cell'));
	p_elm.setAttribute('rowspan', 1);
	p_elm.setAttribute('colspan', 1);
	return p_elm;
}
// #endregion
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


async function convertFileListContentToFormatsAsync(selectionOnly, formats) {
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
			data = getFileListContentData(selectionOnly);
		} 
		else if (isCsvFormat(lwc_format)) {
			data = getFileListContentData(selectionOnly).split(globals.DefaultCsvProps.RowSeparator).join(',');
		} else if (isFileListFormat(lwc_format)) {
			// need to provide if partial selection and text is not included
			data = getFileListContentData(selectionOnly);
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

function addFileEventHandlers() {
	const fli_row_elms = getFileListRowElements();
	for (var i = 0; i < fli_row_elms.length; i++) {
		const fli_row_elm = fli_row_elms[i];
		fli_row_elm.addEventListener('click', onFileListItemRowClick);
	}
}

function transferFileListContent(data_or_dt, source_fli_row_idxs, dest_doc_range, sel_transfer) {
	// input 'MpQuillFileListDataFragment' or DataTransfer

	// LOCATE FRAGMENT

	let data =
		isString(data_or_dt) ?
			data_or_dt :
			data_or_dt.getData(globals.FILE_ITEM_FRAGMENT_FORMAT);

	if (!isString(data)) {
		log('no file fragment to transfer, ignoring');
		return;
	}

	// RESOLVE INSERT IDX

	const append_cell = getTableCellAtDocIdx(dest_doc_range.index);
	let insert_idx = -1;
	if (dest_doc_range.mode == 'pre') {
		if (isAppendManualMode()) {
			insert_idx = append_cell[0];
		} else {
			insert_idx = Math.max(0, append_cell[0] - 1);
		}
	} else {
		insert_idx = append_cell[0] + 1;
	}

	// INSERT FRAGMENT FILES

	const cur_sel_idxs = getSelectedFileItemIdxs();
	let updated_sel_idxs = [];
	let append_items = toJsonObjFromBase64Str(data).fileItems;
	let updated_items = [];
	for (var i = 0; i < globals.FileListItems.length; i++) {
		if (i == insert_idx) {
			for (var j = 0; j < append_items.length; j++) {
				if (!isAppendPreMode()) {
					updated_sel_idxs.push(i + j);
				}
				updated_items.push(append_items[j]);
			}
		}
		if (isAppendPreMode() && cur_sel_idxs.includes(i)) {
			updated_sel_idxs.push(updated_items.length);
		}
		updated_items.push(globals.FileListItems[i]);
	}
	if (insert_idx == globals.FileListItems.length) {
		for (var j = 0; j < append_items.length; j++) {
			if (!isAppendPreMode()) {
				updated_sel_idxs.push(updated_items.length);
			}
			updated_items.push(append_items[j]);
		}
	}

	// REMOVE SOURCES (NON-APPEND)

	if (Array.isArray(source_fli_row_idxs) &&
		source_fli_row_idxs.length > 0) {
		// adj source rows
		for (var i = 0; i < source_fli_row_idxs.length; i++) {
			if (source_fli_row_idxs[i] <= insert_idx) {
				continue;
			}
			source_fli_row_idxs[i] += append_items.length;
		}

		// recreate updated_items w/o sources
		let final_items = [];
		for (var i = 0; i < updated_items.length; i++) {
			if (source_fli_row_idxs.includes(i)) {
				//omit source item
				continue;
			}
			final_items.push(updated_items[i]);
		}
		updated_items = final_items;
	}

	// SET TO SELECT (NON-APPEND)
	if (!isAnyAppendEnabled()) {
		// for non-append sel new items
		updated_sel_idxs = [];
		for (var i = 0; i < updated_items.length; i++) {
			if (append_items.includes(updated_items[i])) {
				updated_sel_idxs.push(i);
			}
		}
	}

	loadFileListContent(toBase64FromJsonObj({ fileItems: updated_items }));
	updated_sel_idxs.forEach(x => getFileListRowElements()[x].classList.add('selected'));

	let fli_doc_range = getFileListItemDocRange(updated_sel_idxs[0]);
	let append_idx = fli_doc_range && fli_doc_range.index !== undefined ? fli_doc_range.index : 0;
	let append_range = {
		index: append_idx
	};
	let last_range = getFileListItemDocRange(updated_sel_idxs[updated_sel_idxs.length - 1]);
	append_range.length = last_range ? last_range.index + last_range.length : 0;
	return append_range;
}

function appendFileListContentData(data) {
	const append_doc_range = getAppendDocRange();

	transferFileListContent(data, null, append_doc_range, 'user');

	scrollToAppendIdx();
	onContentChanged_ntf();
}

function removeFileItemByAnchorElement(a_elm) {
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
	let sup_guid = suppressTextChanged();
	globals.quill.enable(true);

	let btm = getBetterTableModule(true);
	let tableBlot = quillFindBlot(getTableElements()[0].firstChild);
	const row_boundary = btm.tableSelection.boundary;
	tableBlot.deleteRow(row_boundary, getEditorContainerElement());
	updateQuill();

	globals.quill.enable(false);

	//createFileList();
	unsupressTextChanged(sup_guid);
	onContentChanged_ntf();
}

function selectAllFileItems() {
	const fli_elms = getFileListRowElements();
	for (var i = 0; i < fli_elms.length; i++) {
		const fli_elm = fli_elms[i];
		fli_elm.classList.add('selected');
	}
}

function deselectAllFileItems() {
	const fli_elms = getFileListRowElements();
	for (var i = 0; i < fli_elms.length; i++) {
		const fli_elm = fli_elms[i];
		fli_elm.classList.add('selected');
	}
}
// #endregion Actions

// #region Event Handlers

function onFileListItemRemoveRowClick(e) {

	removeFileItemByAnchorElement(e.currentTarget);
}
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