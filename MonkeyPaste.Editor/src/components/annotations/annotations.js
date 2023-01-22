// #region Globals

var AnnotationRects = [];
var Annotations = [];

// #endregion Globals

// #region Life Cycle

function initAnnotations() {

}


function loadAnnotations(annotationsObjOrJsonStr) {
	resetAnnotations();

	if (isNullOrEmpty(annotationsObjOrJsonStr)) {
		return;
	}
	let root_annotations = toJsonObjFromBase64Str(annotationsObjOrJsonStr);
	if (!root_annotations) {
		return;
	}
	if (!Array.isArray(root_annotations)) {
		root_annotations = [root_annotations];
	}

	for (var i = 0; i < root_annotations.length; i++) {
		let annotation_obj = root_annotations[i];
		//let actual_size = null;
		//if (annotation_obj.type == 'RootAnnotation') {
		//	actual_size = getRectSize(cleanRect(annotation_obj));
		//}
		//createAnnotation(annotation_obj, null, actual_size);
		Annotations.push(annotation_obj);
	}

	drawOverlay();
}


// #endregion Life Cycle

// #region Getters

function getAnnotationRoiContainerElement() {
	return document.getElementById('annotationRoiOverlay');
}

function getAnnotationRoiElements() {
	return Array.from(getAnnotationRoiContainerElement().getElementsByClassName('annotation-roi'));
}

function getAnnotationRect(annotation_rect,actual_size) {
	if (!annotation_rect) {
		return null;
	}
	annotation_rect = cleanRect(annotation_rect);
	// scale to editor
	actual_size = actual_size == null ? getContentImageDataSize() : actual_size;

	let xr = getContentWidth() / actual_size.width;
	let yr = getContentHeight() / actual_size.height;

	xr = isNaN(xr) || xr == 0 ? 1 : xr;
	yr = isNaN(yr) || yr == 0 ? 1 : yr;

	annotation_rect.left *= xr;
	annotation_rect.right *= xr;

	annotation_rect.top *= yr;
	annotation_rect.bottom *= yr;
	annotation_rect = cleanRect(annotation_rect);

	// translate to content position (center)
	let content_rect = getContentImageElement().getBoundingClientRect();
	annotation_rect.left += content_rect.x;
	annotation_rect.right += content_rect.x; 

	annotation_rect.top += content_rect.y;
	annotation_rect.bottom += content_rect.y;

	annotation_rect = cleanRect(annotation_rect);
	return annotation_rect;
}

function getAnnotationFillColor(annotation) {
	return 'orange';
}
function getAnnotationStrokeColor(annotation) {
	return 'lime';
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function hasAnnotations() {
	return Annotations && Annotations.length > 0;
}

function resetAnnotations() {
	AnnotationRects = [];
	Annotations = [];

	while (getAnnotationRoiContainerElement().hasChildNodes()) {
		getAnnotationRoiContainerElement().removeChild(getAnnotationRoiContainerElement().lastChild)
	}
}

// #endregion State

// #region Actions

function createAnnotation(rootAnnotation, annotation, actual_size) {
	annotation = annotation == null ? rootAnnotation : annotation;

	let annotation_rect = getAnnotationRect(annotation, actual_size);
	if (annotation_rect) {
		//debugger;
		let annotation_elm = document.createElement('div');
		annotation_elm.classList.add('annotation-roi');

		annotation_elm.setAttribute('rootData', JSON.stringify(rootAnnotation));
		annotation_elm.setAttribute('thisData', JSON.stringify(annotation));
		annotation_elm.addEventListener('click', onAnnotationRoiClick);

		getAnnotationRoiContainerElement().appendChild(annotation_elm);

		updateAnnotationSizesAndPositions();
	}
	if (annotation.children !== undefined &&
		Array.isArray(annotation.children)) {
		for (var i = 0; i < annotation.children.length; i++) {
			createAnnotation(rootAnnotation, annotation.children[i]);
		}
	}
	//let annotation_label = findAnnotationLabel(annotation);
	//if (!annotation_label) {
	//	annotation_label = '';
	//}
	//let annotation_score = findAnnotationLabel(annotation);
	//if (!annotation_label) {
	//	annotation_label = '';
	//}
}

function updateAnnotationSizesAndPositions() {
	let roi_elms = getAnnotationRoiElements();
	for (var i = 0; i < roi_elms.length; i++) {
		let annotation_elm = roi_elms[i];
		let annotation_rect = getAnnotationRect(JSON.parse(annotation_elm.getAttribute('thisData')));
		//annotation_elm.style.transform = `translate(${annotation_rect.left},${annotation_rect.top})`;
		annotation_elm.style.marginLeft = `${annotation_rect.left}px`;
		annotation_elm.style.marginTop = `${annotation_rect.top}px`;
		annotation_elm.style.width = `${annotation_rect.width}px`;
		annotation_elm.style.height = `${annotation_rect.height}px`;
	}
}
// #endregion Actions

// #region Event Handlers

function onAnnotationRoiClick(e) {
	let annotation_elm = e.target;
	if (!annotation_elm) {
		annotation_elm = e.currentTarget;
	}
	if (annotation_elm) {
		alert(annotation_elm.getAttribute('thisData'));
	}
}
// #endregion Event Handlers