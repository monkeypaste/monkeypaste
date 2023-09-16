
// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getDelta(rangeObj) {
	// NOTE if quill is not enabled it return empty contents
	let wasEnabled = globals.quill.isEnabled();
	globals.quill.enable(true);
	rangeObj = rangeObj == null ? { index: 0, length: getDocLength() } : rangeObj;

	let delta = globals.quill.getContents(rangeObj.index, rangeObj.length);
	globals.quill.enable(wasEnabled);

	return delta;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function convertDeltaToHtml(delta, encodeHtmlEntities) {
	/*
	 * {
				'small': 'font-size: 0.75em',
				'large': 'font-size: 1.5em',
				'huge': 'font-size: 2.5em'
	   },*/
	//var inlineStyles = {
		//font: {
		//	'serif': 'font-family: Georgia, Times New Roman, serif',
		//	'monospace': 'font-family: Monaco, Courier New, monospace'
		//},
		//size: globals.DefaultFontSizes.map((x) => { return {`ql-size-${x.split('px')[0]}`: `font-size: ${x}` }}),
			//indent: (value, op) => {
			//	var indentSize = parseInt(value, 10) * 3;
			//	var side = op.attributes['direction'] === 'rtl' ? 'right' : 'left';
			//	return 'padding-' + side + ':' + indentSize + 'em';
			//},
			//	direction: (value, op) => {
			//		if (value === 'rtl') {
			//			return 'direction:rtl' + (op.attributes['align'] ? '' : '; text-align: inherit');
			//		} else {
			//			return '';
			//		}
			//	}
	//};
	for (var i = 0; i < delta.ops.length; i++) {
		let op = delta.ops[i];
		if (!isNullOrUndefined(op.attributes)) {
			if (!isNullOrUndefined(op.attributes.templateGuid)) {
				// move template to insert
				op.insert = {
					'template': op.attributes
				};
				delete op.attributes;
			}
		}
	}

	
	let cfg = {
		//inlineStyles: true,
		//allowBackgroundClasses: true,
		customCssClasses: onCustomCssClasses,
		customTagAttributes: onCustomTagAttributes,
		encodeHtml: encodeHtmlEntities
	};
	let qdc = new window.QuillDeltaToHtmlConverter(delta.ops, cfg);
	qdc.renderCustomWith(function (customOp, contextOp) {
		if (customOp.insert !== undefined && customOp.insert.type === 'template') {
			let temp_span = document.createElement('span');
			return applyTemplateToDomNode(temp_span, getTemplateDefByGuid(customOp.insert.value.templateGuid)).outerHTML;
		}
	});
	let htmlStr = qdc.convert();
	return htmlStr;
}

function convertHtmlToDelta(htmlStr) {
	htmlStr = encodeHtmlSpecialEntitiesFromHtmlDoc(htmlStr);
	const whitespace_sub = '[BOOYABOOYA]';
	htmlStr = htmlStr.replaceAll('&nbsp;', whitespace_sub);

	let htmlObj = { html: htmlStr };
	var delta = globals.quill.clipboard.convert(htmlObj);

	delta = fixHtmlToDeltaWhitespaceSpecialEntities(delta, whitespace_sub);

	if (isPlainHtmlConverter()) {
		return delta;
	}

	delta = fixHtmlToDeltaTemplateInserts(delta);

	return delta;
}

function cleanDelta(delta1) {
	const Delta = Quill.imports.delta;
	if (delta1 == null) {
		return new Delta();
	}
	if (Array.isArray(delta1)) {
		// if array of merge each into single
		if (delta1.length == 0) {
			return [];
		}
		let result = delta1[0];
		for (var i = 1; i < delta1.length; i++) {
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
	// BUG quill changes order of ops for concat and compose
	// maybe since testing w/ a delete attrib but either
	// way it will screw it up so do not use it! (like below comment)
	//return delta1.concat(delta2);

	const Delta = Quill.imports.delta;
	let merged_delta = new Delta([
		...delta1.ops,
		...delta2.ops
	]);
	return merged_delta;
}

function insertDelta(range, deltaOrDeltaJsonStr, source = 'api') {
	let deltaObj = cleanDelta(deltaOrDeltaJsonStr);

	if (!range || range == null) {
		range = { index: 0, length: getDocLength() };
	}

	const Delta = Quill.imports.delta;
	let to_range_delta = new Delta().retain(range.index).delete(range.length);
	let update_delta = mergeDeltas(to_range_delta, deltaObj);
	return globals.quill.updateContents(update_delta, source);
}

function fixHtmlToDeltaTemplateInserts(delta) {
	// loading/saving templates have same prob where inserts
	// only set attributes with blank insert, needs attributes as insert.,
	if (isNullOrUndefined(delta)) {
		return delta;
	}
	for (var i = 0; i < delta.ops.length; i++) {
		if (delta.ops[i].attributes === undefined ||
			delta.ops[i].attributes.templateGuid === undefined) {
			continue;
		}
		delta.ops[i].insert = { 'template': delta.ops[i].attributes };
	}
	return delta;
}

function fixHtmlToDeltaWhitespaceSpecialEntities(delta,whitespaceSubStr) {
	// upgrading to quill 2 for converter broke whitespae conversion again
	// before html is converted to delta but AFTER special html entities are encoded
	// &nbsp; is subbed w/ a random string but both clipboard.convert and pastedangerously 
	// still remove 
	if (isNullOrUndefined(delta)) {
		return delta;
	}
	for (var i = 0; i < delta.ops.length; i++) {
		if (isNullOrUndefined(delta.ops[i].insert) ||
			!isString(delta.ops[i].insert)) {
			continue;
		}
		delta.ops[i].insert = delta.ops[i].insert.replaceAll(whitespaceSubStr, ' ');
	}
	return delta;
}
function encodeHtmlEntitiesInDeltaInserts(delta) {
	if (delta && delta.ops !== undefined) {
		// unescape html special entities only if they were just escaped
		for (var i = 0; i < delta.ops.length; i++) {
			if (delta.ops[i].insert === undefined) {
				continue;
			}
			delta.ops[i].insert = encodeHtmlSpecialEntitiesFromPlainText(delta.ops[i].insert);
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
			if (!isString(delta.ops[i].insert)) {
				// insert is template blot (probably will be others, have no idea why this is a problem now)
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
		globals.quill.updateContents(other_ops, source);
		updateQuill();
	}

	for (var i = 0; i < format_ops.length; i++) {
		const fop = format_ops[i];
		globals.quill.formatText(fop.format.index, fop.format.length, fop.attributes, source);
	}

	updateQuill();
}
// #endregion Actions

// #region Event Handlers
function onCustomCssClasses(op) {
	if (!op ||
		op.attributes === undefined) {
		return;
	}

	let custom_classes = [];
	if (op.attributes.fontBgColorOverride == 'on') {
		custom_classes.push('font-bg-color-override-on');
	}
	if (op.attributes.fontColorOverride == 'on') {
		custom_classes.push('font-color-override-on');
	}
	return custom_classes;
}
function onCustomTagAttributes(op) {
	if (!op ||
		op.attributes === undefined) {
		return;
	}

	//if (op.type == 'template') {
	//	let temp_span = document.createElement('span');
	//	return applyTemplateToDomNode(temp_span, getTemplateDefByGuid(op.template.templateGuid)).outerHTML;
	//}
	var classes = [];
	if (op.attributes.size !== undefined) {
		// FONT SIZE (NOT UNIQUE)
		let font_size_class = `ql-size-${op.attributes.size.split('px')[0]}`;
		//log('class added: ' + font_size_class);
		//classes.push(font_size_class);
	}
	if (op.attributes.list !== undefined) {
		// LIST TYPE
		let li_type = op.attributes.list;
		let li_val = '';

		if (op.insert.type === 'text') {
			li_val = op.insert.value;

			// TODO add font classes here if any (wrap in span)
		}
		//return `<li data-list="${li_type}">${li_val}</li>`;

		return { 'data-list': li_type };
	} else if (globals.TableDeltaAttrbs.some(x => op.attributes[x] !== undefined)) {
		// TABLES

		if (op.attributes['table-col'] !== undefined) {

		}
	} else if (!isNullOrUndefined(op.insert) && op.insert.type == 'text') {
		if (classes.length > 0) {
			//var formatted_result = `<span class="${classes.join(' ')}">${op.insert.value}</span>`;
			//log(formatted_result);
			//return formatted_result;
			return { 'class': classes.join(' ') };
		}
	}
}
// #endregion Event Handlers