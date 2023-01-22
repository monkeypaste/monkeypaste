// #region Globals

const TableDeltaAttrbs = [
	'table-col',
	'table-cell-line',
	'row',
	'rowspan',
	'colspan'
];

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getDelta(rangeObj) {
	// NOTE if quill is not enabled it return empty contents
	let wasEnabled = quill.isEnabled();
	quill.enable(true);
	rangeObj = rangeObj == null ? { index: 0, length: getDocLength() } : rangeObj;

	let delta = quill.getContents(rangeObj.index, rangeObj.length);
	quill.enable(wasEnabled);

	return delta;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function convertDeltaToHtml(delta) {
	function onCustomTagAttributes(op) {
		//debugger;
		if (op && op.attributes !== undefined && op.attributes.list !== undefined) {
			// LIST TYPE
			let li_type = op.attributes.list;
			let li_val = '';

			if (op.insert.type === 'text') {
				li_val = op.insert.value;
			} else {
				debugger;
			}
			return `<li data-list="${li_type}">${li_val}</li>`;
		}

		if (op && op.attributes !== undefined && TableDeltaAttrbs.some(x => op.attributes[x] !== undefined)) {
			// TABLES

			if (op.attributes['table-col'] !== undefined) {

			}
		}
	}
	let cfg = {
		inlineStyles: true,
		//customTagAttributes: onCustomTagAttributes,
		encodeHtml: true
	};
	let qdc = new window.QuillDeltaToHtmlConverter(delta.ops, cfg);
	qdc.renderCustomWith(function (customOp, contextOp) {
		//if (customOp.insert.type === 'my-blot') {
		//	let val = customOp.insert.value;
		//	return `<span id="${val.id}">${val.text}</span>`;
		//} else {
		//	return 'Unmanaged custom blot!';
		//}
		if (customOp.attributes !== undefined) {
			if (customOp.attributes.templateGuid !== undefined) {
				let temp_span = document.createElement('span');
				return applyTemplateToDomNode(temp_span, getTemplateDefByGuid(customOp.attributes.templateGuid)).outerHTML;
			} else {
				debugger;
			}

		}
	});
	let htmlStr = qdc.convert();
	//html = fixDelta2HtmlCheckables(html);
	//
	//log(html);
	return htmlStr;
}

function convertHtmlToDelta(htmlStr) {
	// create temp dom of htmlStr and escape special chars in text nodes	
	let html_doc = DomParser.parseFromString(htmlStr, 'text/html');
	let text_elms = getAllTextElementsInElement(html_doc.body);
	for (var i = 0; i < text_elms.length; i++) {
		let text_elm = text_elms[i];

		text_elm.nodeValue = encodeHtmlSpecialEntities(text_elm.nodeValue);
	}
	htmlStr = html_doc.body.innerHTML;

	let htmlObj = htmlStr;
	if (UseQuill2) {
		// NOTE quill2 expects {html,text} not just html
		htmlObj = { html: htmlStr };
	}
	let delta = quill.clipboard.convert(htmlObj);
	if (isPlainHtmlConverter()) {
		return delta;
	}

	delta = encodeHtmlEntitiesInDeltaInserts(delta);
	return delta;
}

function cleanDelta(delta1) {
	const Delta = Quill.imports.delta;
	if (delta1 == null) {
		return new Delta();
	}
	if (Array.isArray(delta1)) {
		// if array of merge each into single
		let result = null;
		for (var i = 0; i < delta1.length; i++) {
			result = mergeDeltas(result, delta1[i]);
		}
		return result;
	}

	if (typeof delta1 === 'string' || delta1 instanceof String) {
		delta1 = JSON.parse(delta1);
	}
	if (delta1.compose === undefined || typeof delta1.compose !== 'function') {
		delta1 = Object.assign(new Delta, delta1);
	}
	return delta1;
}

function mergeDeltas(delta1, delta2) {
	delta1 = cleanDelta(delta1);
	delta2 = cleanDelta(delta2);
	return delta1.concat(delta2);
}

function insertDelta(range, deltaOrDeltaJsonStr, source = 'api') {
	let deltaObj = cleanDelta(deltaOrDeltaJsonStr);

	if (!range || range == null) {
		range = { index: 0, length: getDocLength() };
	}

	const Delta = Quill.imports.delta;
	//setTextInRange(range, '', source);
	let to_range_delta = new Delta().retain(range.index).delete(range.length);
	let update_delta = mergeDeltas(to_range_delta, deltaObj);

	//let insert_delta = new Delta([
	//	{ retain: range.index },
	//	{ delete: range.length },
	//	...deltaObj.ops
	//]);
	return quill.updateContents(update_delta, source);
}

function encodeHtmlEntitiesInDeltaInserts(delta) {
	if (delta && delta.ops !== undefined) {
		// unescape html special entities only if they were just escaped
		for (var i = 0; i < delta.ops.length; i++) {
			if (delta.ops[i].insert === undefined) {
				continue;
			}
			delta.ops[i].insert = encodeHtmlSpecialEntities(delta.ops[i].insert);
		}
	}
	return delta;
}

function decodeHtmlEntitiesInDeltaInserts(delta) {
	if (delta && delta.ops !== undefined) {
		// unescape html special entities only if they were just escaped
		for (var i = 0; i < delta.ops.length; i++) {
			if (delta.ops[i].insert === undefined) {
				continue;
			}
			if (delta.ops[i].attributes !== undefined &&
				delta.ops[i].attributes.code !== undefined) {
				// insert has code attribute so don't decode
				continue;
			}
			delta.ops[i].insert = decodeHtmlSpecialEntities(delta.ops[i].insert);
		}
	}
	return delta;
}

function applyDelta(delta, source = 'api') {
	let format_ops = delta.filter((op) => op.format !== undefined);	
	let other_ops = delta.filter((op) => !format_ops.includes(op));

	if (other_ops.length > 0) {
		quill.updateContents(other_ops, source);
		quill.update();
}

	for (var i = 0; i < format_ops.length; i++) {
		const fop = format_ops[i];
		quill.formatText(fop.format.index, fop.format.length, fop.attributes, source);
	}

	quill.update();
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers