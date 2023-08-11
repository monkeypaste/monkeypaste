// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getRectTopLeft(rect) {
    return { x: rect.left, y: rect.top };
}
function getRectTopRight(rect) {
    return { x: rect.right, y: rect.top };
}
function getRectBottomRight(rect) {
    return { x: rect.right, y: rect.bottom };
}
function getRectBottomLeft(rect) {
    return { x: rect.left, y: rect.bottom };
}
function getRectSize(rect) {
    if (!isRect(rect)) {
        return { width: 0, height: 0 };
    }
    rect = cleanRect(rect);
    return { width: rect.width, height: rect.height };
}

function getRectArea(rect) {
    let rect_size = getRectSize(rect);
    return rect_size.width * rect_size.height;
}

function getRectCornerByIdx(rect, idx) {
    if (!rect || idx < 0 || idx >= 4) {
        return { x: 0, y: 0 };
    }

    if (idx == 0) {
        return getRectTopLeft(rect);
    }
    if (idx == 1) {
        return getRectTopRight(rect);
    }
    if (idx == 2) {
        return getRectBottomRight(rect);
    }
    return getRectBottomLeft(rect);
}

function getRectCenter(rect) {
    return {
        x: rect.left + (rect.width / 2),
        y: rect.top + (rect.height / 2)
    };
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isRect(obj) {
    if (isNullOrUndefined(obj)) {
        return false;
    }
    return obj.left !== undefined && obj.top !== undefined && obj.right !== undefined && obj.bottom !== undefined;
}

function isPointInRect(rect, p) {
    if (!isRect(rect) || !isPoint(p)) {
        return false;
    }
    let result = p.x >= rect.left && p.x <= rect.right && p.y >= rect.top && p.y <= rect.bottom;
    return result;
}

function isRectContainOtherRect(rect, other_rect) {
    if (!rect || !other_rect) {
        return false;
    }
    let result =
        isPointInRect(rect, { x: other_rect.left, y: other_rect.top }) &&
        isPointInRect(rect, { x: other_rect.right, y: other_rect.bottom });
    return result;
}
function isRectOverlapOtherRect(rect, other_rect) {
    if (!rect || !other_rect) {
        return false;
    }
    let result =
        isPointInRect(rect, { x: other_rect.left, y: other_rect.top }) ||
        isPointInRect(rect, { x: other_rect.right, y: other_rect.top }) ||
        isPointInRect(rect, { x: other_rect.right, y: other_rect.bottom }) ||
        isPointInRect(rect, { x: other_rect.left, y: other_rect.bottom });
    return result;
}
// #endregion State

// #region Actions

// #endregion Actions

function drawRect(
    ctx,
    rect,
    fill = 'black',
    stroke = 'black',
    strokeWidth = 0,
    fillOpacity = 1.0,
    strokeOpacity = 1.0) {

    fill = rect.fill === undefined ? fill : rect.fill;
    fillOpacity = rect.fillOpacity === undefined ? fillOpacity : rect.fillOpacity;
    stroke = rect.stroke === undefined ? stroke : rect.stroke;
    strokeWidth = rect.strokeWidth === undefined ? strokeWidth : rect.strokeWidth;
    strokeOpacity = rect.strokeOpacity === undefined ? strokeOpacity : rect.strokeOpacity;

    let strokStyleStr = cleanColor(stroke, strokeOpacity != 1.0 ? strokeOpacity : null, 'rgbaStyle');
    ctx.strokeStyle = strokStyleStr;

    let fillStyleStr = cleanColor(fill, fillOpacity != 1.0 ? fillOpacity : null, 'rgbaStyle');
    ctx.fillStyle = fillStyleStr;

    ctx.strokeWidth = strokeWidth;

    ctx.fillRect(rect.left, rect.top, rect.width, rect.height);
    if (strokeWidth > 0) {
        ctx.strokeRect(rect.left, rect.top, rect.width, rect.height);
    }
}


function editorToScreenRect(er) {
    let s_origin = editorToScreenPoint({ x: er.left, y: er.top });

    let sr = {};
    sr.left = s_origin.x;
    sr.top = s_origin.y;
    sr.right = sr.left + er.width;
    sr.bottom = sr.top + er.height;
    sr = cleanRect(sr);
    return sr;
}

function screenToEditorRect(sr) {
    let editor_rect = getEditorContainerRect();
    let er = {};
    er.left = sr.left - editor_rect.left;
    er.top = sr.top - editor_rect.top;
    er.right = er.left + sr.width;
    er.bottom = er.top + sr.height;
    er = cleanRect(er);
    return er;
}

function rectUnion(rect_a, rect_b) {
    let rect_u = {
        left: Math.min(rect_a.left, rect_b.left),
        top: Math.min(rect_a.top, rect_b.top),
        right: Math.max(rect_a.right, rect_b.right),
        bottom: Math.max(rect_a.bottom, rect_b.bottom),
    }
    rect_u = cleanRect(rect_u);
    return rect_u;
}

function rectsUnion(rects) {
    let union_rect = null;
    for (var i = 0; i < rects.length; i++) {
        if (!union_rect) {
            union_rect = rects[i];
        } else {
            union_rect = rectUnion(union_rect, rects[i]);
        }
    }
    return union_rect;
}

function clampRect(rect, clamp_rect) {
    // restri
    rect.left = clamp(rect.left, clamp_rect.left, clamp_rect.right);
    rect.right = clamp(rect.right, clamp_rect.left, clamp_rect.right);
    rect.top = clamp(rect.top, clamp_rect.top, clamp_rect.bottom);
    rect.bottom = clamp(rect.bottom, clamp_rect.top, clamp_rect.bottom);
    return cleanRect(rect);
}

function parsePointFromSides(rect, sides) {
    let p = { x: 0, y: 0 };
    if (!sides) {
        return p;
    }
    if (sides.split('|').includes('left')) {
        p.x = rect.left;
    } else if (sides.split('|').includes('right')) {
        p.x = rect.right;
    }
    if (sides.split('|').includes('top')) {
        p.y = rect.top;
    } else if (sides.split('|').includes('bottom')) {
        p.y = rect.bottom;
    }
    return p;
}

function inflateRect(rect, dl, dt, dr, db) {
    rect.left += dl;
    rect.top += dt;
    rect.right += dr;
    rect.bottom += db;
    rect = cleanRect(rect);
    return rect;
}

function moveRectLocation(rect, loc) {
    let w = rect.width;
    let h = rect.height;
    return cleanRect({
        left: loc.x,
        top: loc.y,
        right: loc.x + w,
        bottom: loc.y + h
    });
}

function parseRect(rectStr) {
    if (isNullOrEmpty(rectStr)) {
        return null;
    }
    let rect_obj = JSON.parse(rectStr);
    if (rect_obj.left === undefined ||
        rect_obj.top === undefined ||
        rect_obj.right === undefined ||
        rect_obj.bottom === undefined) {
        return null;
    }
    rect.left = parseFloat(rect.left);
    rect.top = parseFloat(rect.top);
    rect.right = parseFloat(rect.right);
    rect.bottom = parseFloat(rect.bottom);
    return cleanRect(rect);
}


function cleanRect(rect) {
    let cr = {
        left: rect ? rect.left : 0,
        top: rect ? rect.top : 0,
        right: rect ? rect.right : 0,
        bottom: rect ? rect.bottom : 0,
        width: rect ? rect.right - rect.left : 0,
        height: rect ? rect.bottom - rect.top : 0
    };
    if(!rect) {
        return cr;
    }
    if (!isNullOrUndefined(rect.fill)) {
        cr.fill = rect.fill;
    }
    if (!isNullOrUndefined(rect.stroke)) {
        cr.stroke = rect.stroke;
    }
    if (!isNullOrUndefined(rect.strokeWidth)) {
        cr.strokeWidth = rect.strokeWidth;
    }
    if (!isNullOrUndefined(rect.fillOpacity)) {
        cr.fillOpacity = rect.fillOpacity;
    }
    if (!isNullOrUndefined(rect.strokeOpacity)) {
        cr.strokeOpacity = rect.strokeOpacity;
    }
    return cr;
}

function toBtRect(rect) {
    rect.x = rect.left;
    rect.y = rect.top;
    rect.x1 = rect.right;
    rect.y1 = rect.bottom;
    return rect;
}
function fromBtRect(rect) {
    rect.left = rect.x;
    rect.top = rect.y;
    rect.right = rect.x1;
    rect.bottom = rect.y1;
    return rect;
}
// #region Event Handlers

// #endregion Event Handlers