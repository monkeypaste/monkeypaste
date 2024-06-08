
// #region Life Cycle

function initPlainHtmlConverter() {
	// NOTE needs to match plainhtmlconverter.cs version
	globals.ContentHandle = '[CONVERTER]';

	if (!isRunningOnHost()) {
		document.head.getElementsByTagName('title')[0].innerText = 'Editor (converter)';
	}

	globals.quill = initQuill();
	initClipboard();
	getEditorElement().classList.add('ql-editor-converter');

	globals.IsConverterLoaded = true;
	setEditorIsLoaded(true);

	onInitComplete_ntf();
}

// #endregion Life Cycle

// #region Getters
function getEditorInlineStyleHtml() {
	let sup_guid = suppressTextChanged();

	// store original html
	let editor_elm = getEditorElement();
	let orig_html = editor_elm.outerHTML;

	// convert all relevant props to inline and get html
	let props = [
		//'background',
		'color',
		'direction',
		'font-family',
		'font-size',
		'font-stretch',
		'font-style',
		'text-decoration'
	];
	computedStyleToInlineStyle(editor_elm, true, props);
	let result = editor_elm.innerHTML;

	// restore original html
	editor_elm.outerHTML = orig_html;
	unsupressTextChanged(sup_guid);

	// remove any syntax ui
	let result_doc = globals.DomParser.parseFromString(result, 'text/html');
	result_doc.querySelectorAll('select.ql-ui').forEach(x => x.remove());

	// escape entities
	result = encodeHtmlSpecialEntitiesFromHtmlDoc(null, result_doc);
	// remove <br>
	result = convertHtmlLineBreaks(result_doc.body.innerHTML);
	return result;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isPlainHtmlConverter() {
	return getEditorElement().classList.contains('ql-editor-converter');
}
// #endregion State

// #region Actions
function convertPlainHtml(dataStr, formatType, verifyText, bgOpacity = 0.0) {
	
	let stop = startStopwatch('conversion time');

	if (!globals.IsConverterLoaded) {
		log('convertPlainHtml error! converter not initialized, returning null');
		return null;
	}
	let needs_encoding = formatType != 'rtf2html';
	let DO_VALIDATE = true;
	let ENFORCE_VALIDATE = false;

	//log("Converting '" + formatType + "'. The data is: ");
	//log(dataStr);


	if (formatType == 'plaintext') {
		setEditorText(dataStr, 'user');
	} else {
		let clean_html = stripLineBreaks(cleanHtmlForFragmentMarkers(dataStr));

		let html_doc = globals.DomParser.parseFromString(clean_html, 'text/html');

		// images aren't converted so if present in content its composite
		let is_composite = Array.from(html_doc.querySelectorAll('img')).length > 0;
		if (is_composite) {
			// for mixed content ignore validations
			DO_VALIDATE = false;
		}
		let html_str = needs_encoding ?
			html_doc.body.innerHTML :
			dataStr;
		setEditorHtml(html_str, 'user');
	}
	updateQuill();

	let output_html = '';

	if (isTableInDocument()) {
		// delta-to-html doesn't convert tables
		output_html = getHtml(null, needs_encoding);
		DO_VALIDATE = false;
	} else {
		// TODO? does html w/ lists need to skip validation too? 
		// should probably check that but its hot and i'm done
		output_html = getHtml(null, needs_encoding);
	}
	if (isNullOrWhiteSpace(output_html)) {
		// fallback and use delta2html, i think its a problem when there's only 1 block and content was plain text
		output_html = getHtml(null, needs_encoding);
	}
	// swap pre's for spans cause it screws stuf upf

	let is_conv_html_valid = true;

	if (DO_VALIDATE) {
		output_html = verifyConv(verifyText, output_html, needs_encoding, ENFORCE_VALIDATE);
	}

	//setEditorHtml(output_html);
	let output_delta = convertHtmlToDelta(output_html);
	//let themed_html = getHtml(null, true, false, true, true);
	let themed_html = getEditorInlineStyleHtml();
	//setEditorHtml(themed_html);
	output_html = convertHtmlLineBreaks(output_html);

	stop();
	return {
		themed_html: themed_html,
		html: output_html,
		delta: output_delta,
		valid: is_conv_html_valid
		//icon: iconBase64
	};
}

function verifyConv(verifyText, output_html, needs_encoding, enforceValidation) {
	if (isNullOrUndefined(verifyText)) {
		return output_html;
	}
	let convText = getText();
	let c_ascii = toAscii(convText);

	let v_ascii = toAscii(verifyText.replaceAll(`\r\n`, `\n`));
	if (!v_ascii.endsWith('\n')) {
		// BUG1 when actual text doesn't end w/ newline make sure quills newline is removed
		// BUG2 dom parser adding TWO extra new lines to html fragment of html text so
		// removing extras until match or letting it fail still if not the case
		let last_conv_char = c_ascii.charAt(c_ascii.length - 1);
		while (last_conv_char == '\n') {
			c_ascii = trimQuillTrailingLineEndFromText(c_ascii);
			if (c_ascii.length == 0) {
				break;
			}
			last_conv_char = c_ascii.charAt(c_ascii.length - 1);
		}
	}
	// compare content only, new lines aren't critical and commonly create failure
	let v_comp = v_ascii.replaceAll('\n', '');
	let c_comp = c_ascii.replaceAll('\n', '');
	const diff_idx = getFirstDifferenceIdx(v_comp, c_comp);
	is_conv_html_valid = diff_idx < 0;

	if (!enforceValidation || is_conv_html_valid) {
		if (is_conv_html_valid) {
			log('conversion validate: PASSED');
		}
		if (!needs_encoding) {
			// encode final output
			output_html = getHtml(null, true);
		}
		const is_actual_inline_only = v_ascii.indexOf('\n') < 0;
		if (is_actual_inline_only) {
			output_html = output_html.replace('<p', '<span').replace('p>', 'span>');
		}
	} else {
		log('conversion validate: FAILED');
		let first_verify_diff_char = getEscapedStr(v_comp.charAt(diff_idx));
		let first_conv_diff_char = getEscapedStr(c_comp.charAt(diff_idx));

		log(`first diff: idx ${diff_idx} verify: '${first_verify_diff_char}' conv: '${first_conv_diff_char}'`);
		log('verify length: ' + v_ascii.length);
		log('conv length: ' + c_ascii.length);
		log('verify text:');
		log(verifyText);
		log('conv text:');
		log(convText);
		onException_ntf('conversion error.');
		output_html = encodeHtmlSpecialEntitiesFromPlainText(verifyText);
	}
	return output_html;
}

function locateFaviconBase64(htmlStr) {
	const BASE64_PREFIX = 'data:image/png;base64,';
	const SVG_PREFIX = 'data:image/svg+xml,';


	let html_doc = globals.DomParser.parseFromString(htmlStr, 'text/html');
	let favicon_links = Array.from(html_doc.head.querySelectorAll('link[rel="icon"],link[rel="shortcut icon"]'));
	let results = [];
	for (var i = 0; i < favicon_links.length; i++) {
		let favicon_link = favicon_links[i];
		if (favicon_link.getAttribute('href').startsWith(SVG_PREFIX)) {
			let svg_html = favicon_link.getAttribute('href').split(SVG_PREFIX)[1];

			//var favicon_xml = (new XMLSerializer).serializeToString(svg_html);

			//var ctx = getOverlayContext();
			var img = new Image;
			img.onload = function (e) {
				let w = e.currentTarget.width;
				let h = e.currentTarget.height;

				let cnv = document.createElement('canvas');				
				cnv.width = w;
				cnv.height = h;

				let ctx = cnv.getContext('2d');

				ctx.drawImage(e.currentTarget, 0, 0, w, h);
				let favicon_base64 = cnv.toDataURL().split(BASE64_PREFIX)[1]; 
			};
			//img.src = "data:image/svg+xml;base64," + btoa(svg_html);
			img.src = favicon_link.getAttribute('href');
		}
		
	}

}

function loadImageAsPNG(url, height, width) {
	return new Promise((resolve, reject) => {
		let sourceImage = new Image();

		sourceImage.onload = () => {
			let png = new Image();
			let cnv = document.createElement('canvas'); 
			cnv.height = height;
			cnv.width = width;

			let ctx = cnv.getContext('2d');

			ctx.drawImage(sourceImage, 0, 0, height, width);
			png.src = cnv.toDataURL(); // defaults to image/png
			resolve(png);
		}
		image.onerror = reject;

		image.src = url;
	});
}

function swapPreForDivTags(htmlStr) {
	let html_doc = globals.DomParser.parseFromString(htmlStr, 'text/html');
	let elms = html_doc.querySelectorAll('pre');
	for (var i = 0; i < elms.length; i++) {
		let node = elms[i];
		let div_elm = html_doc.createElement('p');
		div_elm.classList = node.classList;
		div_elm.style = node.style;
		div_elm.innerHTML = node.innerHTML;
		let parent_elm = node.parentElement;
		parent_elm.replaceChild(div_elm, node);
	}
	return html_doc.body.innerHTML;
}

function fixPlainHtmlColorContrast(htmlStr, opacity) {
	const bg_color = findElementBackgroundColor(getEditorElement());
	const is_bg_bright = isBright(bg_color);
	const fallback_fg = is_bg_bright ? cleanColor('black') : cleanColor('white');

	let html_doc = globals.DomParser.parseFromString(htmlStr, 'text/html');
	let elms = html_doc.querySelectorAll(globals.InlineTags.join(", ") + ',' + globals.BlockTags.join(','));
	for (var i = 0; i < elms.length; i++) {
		try {
			if (!isNullOrUndefined(elms[i].style.backgroundColor)) {
				let bg_rgba = cleanColor(elms[i].style.backgroundColor);
				bg_rgba.a = opacity;
				let newBg = rgbaToCssColor(bg_rgba);
				elms[i].style.backgroundColor = newBg;
			}
			if (!isNullOrUndefined(elms[i].style.color)) {
				let fg_rgba = elms[i].style.color.startsWith('var') ? fallback_fg : cleanColor(elms[i].style.color);
				const is_fg_bright = isBright(fg_rgba);
				if (is_bg_bright != is_fg_bright) {
					// contrast is ok, ignore
					continue;
				}
				if (isRgbFuzzyBlackOrWhite(fg_rgba)) {
					// if fg isn't a particular color just adjust to bg
					fg_rgba = cleanColor(is_bg_bright ? 'black' : 'white');
				} else {
					// for unique color 
					const amount = is_fg_bright ? -50 : 50;
					fg_rgba = shiftRgbaLightness(fg_rgba, amount);
				}
				fg_rgba.a = 1;
				let newFg = rgbaToCssColor(fg_rgba);
				elms[i].style.color = newFg;

			}
		} catch (ex) {
			log(ex);
			debugger;
		}
	}
	return html_doc.body.innerHTML;
}

function forceDeltaBgOpacity(delta, opacity) {
	if (!delta || delta.ops === undefined || delta.ops.length == 0) {
		return delta;
	}
	delta.ops
		.filter(x => x.attributes !== undefined && x.attributes.background !== undefined)
		.forEach(x => x.attributes.background = cleanColor(x.attributes.background, 0, 'rgbaStyle'));

	delta.ops
		.filter(x => x.attributes !== undefined && x.attributes.background !== undefined)
		.forEach(x => log(x.attributes.background));

	return delta;
}

function fixHtmlBug1(htmlStr) {
	// replace <span>Â </span>
	return htmlStr.replaceAll('<span>Â </span>', '');
}

function removeUnicode(str) {
	str = str.replace(/[\uE000-\uF8FF]/ig, '');
	return str;
}
function fixUnicode(text) {
	// replaces any combo of chars in [] with single space
	const regex = /(?!\w*(\w)\w*\1)[Âï¿½]+/ig;
	const regex2 = /[^\u0000-\u007F]+/ig;

	let fixedText = text.replaceAll(regex, ' ');
	fixedText = fixedText.replaceAll(regex2, '');
	return fixedText;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers
