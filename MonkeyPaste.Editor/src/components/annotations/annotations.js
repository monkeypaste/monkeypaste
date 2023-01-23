// #region Globals

var RootAnnotations = [];
var SelectedAnnotationGuid = null;
var HoverAnnotationGuid = null;

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
		RootAnnotations.push(annotation_obj);
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

function getAnnotationRect(ann, actual_size) {
	if (!isRoiAnnotation(ann)) {
		return null;
	}

	let ann_rect = cleanRect(ann);
	// scale to editor
	actual_size = actual_size == null ? getContentImageDataSize() : actual_size;

	let xr = getContentWidth() / actual_size.width;
	let yr = getContentHeight() / actual_size.height;

	xr = isNaN(xr) || xr == 0 ? 1 : xr;
	yr = isNaN(yr) || yr == 0 ? 1 : yr;

	ann_rect.left *= xr;
	ann_rect.right *= xr;

	ann_rect.top *= yr;
	ann_rect.bottom *= yr;
	ann_rect = cleanRect(ann_rect);

	// translate to content position (center)
	let content_rect = getContentImageElement().getBoundingClientRect();
	ann_rect.left += content_rect.x;
	ann_rect.right += content_rect.x; 

	ann_rect.top += content_rect.y;
	ann_rect.bottom += content_rect.y;

	ann_rect = cleanRect(ann_rect);

	return {
		...ann_rect,
		...getAnnotationRectStyle(ann)
	};
}

function getAnnotationRectStyle(ann) {
	if (!isRoiAnnotation(ann)) {
		return {};
	}
	var fill = 'pink';
	var stroke = 'orange';
	if (ann.guid == HoverAnnotationGuid) {
		if (ann.guid == SelectedAnnotationGuid) {
			stroke = 'lime';
		} else {
			stroke = 'yellow';
		}
	} else if (ann.guid == SelectedAnnotationGuid) {
		stroke = 'red'
	}
	var strokeOpacity = 1.0;
	var fillOpacity = 0.5;
	var strokeWidth = 2;

	return {
		fill: this.fill,
		stroke: this.stroke,
		strokeWidth: this.strokeWidth,
		fillOpacity: this.fillOpacity,
		strokeOpacity: this.strokeOpacity
	};
}

function getSelectedAnnotation() {
	if (isNullOrWhiteSpace(SelectedAnnotationGuid)) {
		return null;
	}
	return findAnnotationByGuid(SelectedAnnotationGuid);
}

function getAllAnnotations() {
	if (!hasAnnotations()) {
		return [];
	}
	let result = [];
	for (var i = 0; i < RootAnnotations.length; i++) {
		let cur_result = getAnnotationAndAllDescendants(RootAnnotations[i]);
		result.push(...cur_result);
	}
	return result;
}

function getAnnotationAndAllDescendants(cur_ann) {
	if (isNullOrUndefined(cur_ann)) {
		return [];
	}
	let result = [cur_ann];
	let descendants = getAnnotationDescendants(cur_ann);
	result.push(...descendants);
	return result;
}

function getAnnotationDescendants(cur_ann) {
	let result = [];
	if (!isParentAnnotation(cur_ann)) {
		return result;
	}
	for (var i = 0; i < cur_ann.children.length; i++) {
		let cur_result = getAnnotationDescendants(cur_ann.children[i]);
		result.push(...cur_result);
	}
	return result;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function hasAnnotations() {
	return RootAnnotations && RootAnnotations.length > 0;
}

function resetAnnotations() {
	SelectedAnnotationGuid = null;
	RootAnnotations = [];

	removeAllChildren(getAnnotationRoiContainerElement());
}

function isParentAnnotation(ann) {
	if (isNullOrUndefined(ann)) {
		return false;
	}
	return
		ann.children !== undefined &&
		Array.isArray(ann.children);
}

function isRoiAnnotation(ann) {
	return isRect(ann);
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
	if (isParentAnnotation(annotation)) {
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

function findAnnotationByGuid(ann_guid, cur_ann) {
	if (!hasAnnotations() || isNullOrWhiteSpace(ann_guid)) {
		return null;
	}
	if (isNullOrUndefined(cur_ann)) {
		//root level
		for (var i = 0; i < RootAnnotations.length; i++) {
			let result = findAnnotationByGuid(ann_guid, RootAnnotations[i]);
			if (result) {
				return result;
			}
		}
	}
	if (cur_ann.guid == ann_guid) {
		return cur_ann;
	}
	if (!isParentAnnotation(cur_ann)) {
		return null;
	}
	for (var i = 0; i < cur_ann.children.length; i++) {
		let result = findAnnotationByGuid(ann_guid, cur_ann.children[i]);
		if (result) {
			return result;
		}
	}
	return null;
}

function findAnnotationUnderWindowPoint(wp,ann) {
	let ann_and_desc = null;
	if (isNullOrUndefined(ann)) {
		ann_and_desc = getAllAnnotations();
	} else {
		ann_and_desc = getAnnotationAndAllDescendants(ann);
	}

	// NOTE since annotations can overlap and/or be
	// within another this selects hit by smallest area under pointer
	let hits_by_area =
		ann_and_desc
			.filter(x => isPointInRect(cleanRect(x), wp))
			.sort((a, b) => { return getRectArea(a) - getRectArea(b) });
	if (hits_by_area.length == 0) {
		return null;
	}
	return hits_by_area[0];
}

function selectAnnotation(ann_guid, fromHost = false) {
	log('selected annotation: ' + ann_guid + ' fromHost: ' + fromHost);

	SelectedAnnotationGuid = ann_guid;
	drawOverlay();
	if (fromHost) {
		return;
	}
	onAnnotationSelected_ntf(SelectedAnnotationGuid);
}

function hoverAnnotation(ann_guid, fromHost = false) {
	log('hover annotation: ' + ann_guid + ' fromHost: ' + fromHost);
	HoverAnnotationGuid = ann_guid;
	drawOverlay();
	if (fromHost) {
		return;
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

function onAnnotationWindowPointerClick(e) {
	if (!hasAnnotations()) {
		return;
	}
	let sel_ann = getSelectedAnnotation();
	if (isNullOrUndefined(sel_ann)) {
		return;
	}
	let hit_ann = findAnnotationUnderWindowPoint(WindowMouseLoc, sel_ann);
	if (isNullOrUndefined(hit_ann)) {
		return;
	}
	selectAnnotation(hit_ann.guid);
}

function onAnnotationWindowPointerMove(e) {
	if (!hasAnnotations()) {
		return;
	}
	let sel_ann = getSelectedAnnotation();
	if (isNullOrUndefined(sel_ann)) {
		document.body.style.cursor = 'default';
		return;
	}
	let hit_ann = findAnnotationUnderWindowPoint(WindowMouseLoc, sel_ann);
	if (isNullOrUndefined(hit_ann)) {
		hoverAnnotation(null);
		document.body.style.cursor = 'default';
		return;
	}
	hoverAnnotation(hit_ann.guid);
	document.body.style.cursor = 'pointer';
}

// #endregion Event Handlers