// #region Globals

var AnnotationRects = [];
var Annotations = [];

// #endregion Globals

// #region Life Cycle

function loadAnnotations(annotationsJsonStr) {
	resetAnnotations();

	if (isNullOrEmpty(annotationsJsonStr)) {
		return;
	}
	let root_annotations = JSON.parse(annotationsJsonStr);
	if (!root_annotations) {
		return;
	}
	if (!Array.isArray(root_annotations)) {
		root_annotations = [root_annotations];
	}

	for (var i = 0; i < root_annotations.length; i++) {
		let annotation_obj = JSON.parse(root_annotations[i]);
		createAnnotation(annotation_obj);
		Annotations.push(annotation_obj);
	}
	if (Annotations.length > 0) {

	}
}

function createAnnotation(rootAnnotation, annotation) {
	annotation = annotation == null ? rootAnnotation : annotation;

	let annotation_rect = getAnnotationRect(annotation);
	if (annotation_rect) {
		debugger;
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
			createAnnotation(rootAnnotation,annotation.children[i]);
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

// #endregion Life Cycle

// #region Getters

function getAnnotationRoiContainerElement() {
	return document.getElementById('annotationRoiOverlay');
}

function getAnnotationRoiElements() {
	return Array.from(getAnnotationRoiContainerElement().getElementsByClassName('annotation-roi'));
}

function getAnnotationRect(annotation) {
	if (!annotation) {
		return null;
	}
	if (annotation.x !== undefined &&
		annotation.y !== undefined) {
		let rect = { left: parseFloat(annotation.x), top: parseFloat(annotation.y) };
		if (annotation.width !== undefined) {
			rect.right = rect.left + parseFloat(annotation.width);
		}
		if (annotation.height !== undefined) {
			rect.bottom = rect.top + parseFloat(annotation.height);
		}

		// scale to editor
		let actual_size = getContentImageDataSize();

		let xr = getContentWidth() / actual_size.width;
		let yr = getContentHeight() / actual_size.height;

		rect.left *= xr; rect.right *= xr;
		rect.top *= yr; rect.bottom *= yr;

		// translate to content position (center)
		let content_rect = getContentImageElement().getBoundingClientRect();
		rect.left += content_rect.x;
		rect.top += content_rect.y;

		rect = cleanRect(rect);
		return rect;
	}
	return null;
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