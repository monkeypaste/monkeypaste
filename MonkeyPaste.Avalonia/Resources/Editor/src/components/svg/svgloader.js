// #region Globals

// #endregion Globals

// #region Life Cycle

function initSvgElements() {
	Array.from(document.querySelectorAll(`[class*=${globals.SVG_CLASS_PREFIX}]`))
		.forEach(svg_container_elm => {
			const svg_key =
				Array.from(svg_container_elm.classList)
					.find(x => x.startsWith(globals.SVG_CLASS_PREFIX))
					.replace(globals.SVG_CLASS_PREFIX, '');

			const classes =
				svg_container_elm.hasAttribute(globals.SVG_INNER_CLASS_ATTR) ?
					svg_container_elm.getAttribute(globals.SVG_INNER_CLASS_ATTR) : null;

			const use_defaults =
				svg_container_elm.classList.contains(globals.SVG_NO_DEFAULT_CLASS) == false;
			svg_container_elm.innerHTML = getSvgHtml(svg_key, classes, use_defaults);
		});
}

// #endregion Life Cycle

// #region Getters

function getSvgHtml(svgKey, classes, apply_default_styles = true) {
	const svg_elm = createSvgElement(svgKey, classes, apply_default_styles);
	return svg_elm.outerHTML;
}
// #endregion Getters

// #region Setters

function setSvgElmColor(svgElm, chex) {
	//svgElm.style.color = chex;
	//svgElm.style.fill = chex;
	//svgElm.style.stroke = chex;

	Array.from(svgElm.querySelectorAll('path, g')).forEach(x => {
		setElementComputedStyleProp(x, 'fill', cleanColor(chex, null, 'rgbaStyle'));
	});
}
// #endregion Setters

// #region State

// #endregion State

// #region Actions

function createSvgElement(svgKey, classes, apply_default_styles = true) {
	if (isNullOrWhiteSpace(svgKey) || globals.SvgElements[svgKey] == null) {
		svgKey = 'empty';
	}
	let htmlStr = globals.SvgElements[svgKey];
	classes = isNullOrEmpty(classes) ? 'svg-icon' : classes;
	htmlStr = htmlStr.replace('<svg ', `<svg class='${classes}' `);

	let dummy = document.createElement('div');
	dummy.innerHTML = htmlStr;

	if (apply_default_styles == false) {
		return dummy.firstChild;
	}
	const styled_svg_elm = applyShapeStyles(dummy.firstChild);
	return styled_svg_elm;
}

function applyShapeStyles(svgElm) {
	if (!svgElm) {
		return svgElm;
	}
	let fills = Array.from(svgElm.querySelectorAll('path,circle,g,polygon'));
	for (var i = 0; i < fills.length; i++) {
		fills[i].classList.add('ql-fill');
	}
	let strokes = Array.from(svgElm.querySelectorAll('line,polyline,path,rect,circle'));
	for (var i = 0; i < strokes.length; i++) {
		strokes[i].classList.add('ql-stroke');
	}
	return svgElm;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers