
function dist(p1, p2) {
    return Math.sqrt(Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2));
}

function distSqr(p1, p2) {
    return Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2);
}

function isPointInRect(rect, p) {
    if (!rect || !p) {
        return false;
	}
    return p.x >= rect.left && p.x <= rect.right && p.y >= rect.top && p.y <= rect.bottom;
}

function rectContainsRect(rect, r) {
    if (!rect || !p) {
        return false;
    }
    return isPointInRect(rect, { x: r.left, y: r.top }) && isPointInRect(rect, { x: r.right, y: r.bottom });
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

function inflateRect(rect, dl, dt, dr, db) {
    rect.left += dl;
    rect.top += dt;
    rect.right += dr;
    rect.bottom += db;
    rect = cleanRect(rect);
    return rect;
}

function cleanRect(rect) {
    return {
        left: rect ? rect.left : 0,
        top: rect ? rect.top : 0,
        right: rect ? rect.right : 0,
        bottom: rect ? rect.bottom : 0,
        width: rect ? rect.right - rect.left : 0,
        height: rect ? rect.bottom - rect.top : 0
    };
}
