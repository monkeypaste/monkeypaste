// #region Globals

var RootAnnotations = [];
var SelectedAnnotationGuid = null;
var HoverAnnotationGuid = null;

// #endregion Globals

// #region Life Cycle

function initAnnotations() {

}
function loadAnnotations(annotationsObjOrJsonStr) {
	if (isNullOrEmpty(annotationsObjOrJsonStr)) {
		return;
	}
	let root_annotations_to_load = toJsonObjFromBase64Str(annotationsObjOrJsonStr);
	if (!root_annotations_to_load) {
		return;
	}
	if (!Array.isArray(root_annotations_to_load)) {
		root_annotations_to_load = [root_annotations_to_load];
	}

	for (var i = 0; i < root_annotations_to_load.length; i++) {
		let annotation_obj = root_annotations_to_load[i];
		if (!annotation_obj) {
			continue;
		}
		let cur_root_ann = findAnnotationByGuid(annotation_obj.guid);
		if (cur_root_ann) {
			log('warning! loading existing ann ' + annotation_obj.guid + ' orig will be replaced.');
			// annotation already exists, replate item
			let cur_root_ann_idx = RootAnnotations.indexOf(cur_root_ann);
			if (cur_root_ann_idx < 0 || cur_root_ann_idx >= RootAnnotations.length) {
				//prob w/ index of...
				debugger;
				continue;
			}
			RootAnnotations[cur_root_ann_idx] = annotation_obj;
		} else {
			RootAnnotations.push(annotation_obj);
		}
		
	}

	showAnnotations();
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
	actual_size = actual_size == null ? { width: ContentImageWidth, height: ContentImageHeight } : actual_size;

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
	let _fill = 'pink';
	let _stroke = 'orange';
	let _strokeOpacity = 1.0;
	let _fillOpacity = 0.25;
	let _strokeWidth = 2;

	let is_hover_or_desc = isSelfOrDescendantAnnotationByGuid(ann, HoverAnnotationGuid);
	let is_sel_or_desc = isSelfOrDescendantAnnotationByGuid(ann, SelectedAnnotationGuid);
	if (is_hover_or_desc) {
		if (is_sel_or_desc) {
			_stroke = 'lime';
		} else {
			_stroke = 'yellow';
		}
		_fillOpacity = 0.5;
	} else if (is_sel_or_desc) {
		_stroke = 'red'
	}

	return {
		fill: _fill,
		stroke: _stroke,
		strokeWidth: _strokeWidth,
		fillOpacity: _fillOpacity,
		strokeOpacity: _strokeOpacity
	};
}

function getSelectedAnnotation() {
	if (isNullOrWhiteSpace(SelectedAnnotationGuid)) {
		return null;
	}
	return findAnnotationByGuid(SelectedAnnotationGuid);
}

function getVisibleAnnotations() {
	if (!hasAnnotations() || SelectedAnnotationGuid == null) {
		return [];
	}
	let sel_ann = getSelectedAnnotation();
	if (!sel_ann) {
		return [];
	}
	let root_ann = getRootAnnotation(sel_ann);
	let result = getAnnotationAndAllDescendants(root_ann);
	return result;
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

function getRootAnnotation(ann) {
	let cur_ann = ann;
	let parent_ann = getParentAnnotation(cur_ann);
	while (parent_ann != null) {
		cur_ann = parent_ann;
		parent_ann = getParentAnnotation(cur_ann);
	}
	return cur_ann;
}

function getParentAnnotation(ann) {
	if (isNullOrUndefined(ann)) {
		return null;
	}
	let parent_ann =
		getAllAnnotations()
			.filter(x => isParentAnnotation(x))
			.find(x => x.children.some(y => y.guid == ann.guid));

	return parent_ann;
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
		let cur_result = getAnnotationAndAllDescendants(cur_ann.children[i]);
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
	resetForcedCursor();

	removeAllChildren(getAnnotationRoiContainerElement());
}

function isParentAnnotation(ann) {
	if (isNullOrUndefined(ann)) {
		return false;
	}
	let result =
		ann.children !== undefined &&
		Array.isArray(ann.children);
	return result;
}

function isSelfOrDescendantAnnotationByGuid(ann, parent_ann_guid) {
	if (isNullOrEmpty(parent_ann_guid) ||
		isNullOrUndefined(ann)) {
		return false;
	}
	if (ann.guid == parent_ann_guid) {
		return true;
	}
	let cur_parent = getParentAnnotation(ann);
	while (cur_parent != null) {
		if (cur_parent.guid == parent_ann_guid) {
			return true;
		}
		cur_parent = getParentAnnotation(cur_parent);
	}
	return false;
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
	if (cur_ann &&
		!isNullOrUndefined(cur_ann.guid) &&
		cur_ann.guid == ann_guid) {
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

function findAnnotationUnderWindowPoint(wp) {
	if (!isPoint(wp) || !hasAnnotations()) {
		return null;
	}
	let visible_anns = getVisibleAnnotations();

	// NOTE since annotations can overlap and/or be
	// within another this selects hit by smallest area under pointer
	let hits_by_area =
		visible_anns
			.filter(x => isPointInRect(getAnnotationRect(x), wp))
			.sort((a, b) => { return getRectArea(a) - getRectArea(b) });
	if (hits_by_area.length == 0) {
		return null;
	}
	return hits_by_area[0];
}

function selectAnnotation(ann_or_annGuid, fromHost = false) {
	let ann_guid = null;
	if (isString(ann_or_annGuid)) {
		ann_guid = ann_or_annGuid;
	} else if (!isNullOrUndefined(ann_or_annGuid)) {
		ann_guid = ann_or_annGuid.guid;
	}
	log('selected annotation: ' + ann_guid + ' fromHost: ' + fromHost);

	SelectedAnnotationGuid = ann_guid;
	drawOverlay();
	if (!fromHost) {
		onAnnotationSelected_ntf(SelectedAnnotationGuid);
	}
}

function hoverAnnotation(ann_or_annGuid, fromHost = false) {
	let ann_guid = null;
	if (isString(ann_or_annGuid)) {
		ann_guid = ann_or_annGuid;
	} else if (!isNullOrUndefined(ann_or_annGuid)) {
		ann_guid = ann_or_annGuid.guid;
	}
	//log('hover annotation: ' + ann_guid + ' fromHost: ' + fromHost);
	HoverAnnotationGuid = ann_guid;
	drawOverlay();
	if (fromHost) {
		return;
	}
}

function hideAnnotations() {
	if (!hasAnnotations()) {
		return;
	}

	selectAnnotation(null);
	drawOverlay();
}

function showAnnotations(fromHost = false) {
	if (!hasAnnotations()) {
		return;
	}
	if (isNullOrUndefined(SelectedAnnotationGuid)) {
		selectAnnotation(RootAnnotations[0].guid, fromHost);
	}
	drawOverlay();
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
	let hit_ann = findAnnotationUnderWindowPoint(WindowMouseLoc);
	if (isNullOrUndefined(hit_ann)) {
		return;
	}
	selectAnnotation(hit_ann);
}

function onAnnotationWindowPointerMove(e) {
	let hover_ann = findAnnotationUnderWindowPoint(WindowMouseLoc);
	hoverAnnotation(hover_ann);

	let forced_cursor = hover_ann ? 'pointer' : 'default';
	forceCursor(forced_cursor);
}

// #endregion Event Handlers