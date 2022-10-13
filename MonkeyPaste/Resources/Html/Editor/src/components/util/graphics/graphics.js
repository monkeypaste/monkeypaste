
function drawPolygon(ctx, points, stroke = 'black', fill = 'black', width = 0) {
    ctx.beginPath();

    ctx.moveTo(r.left, r.top);
    ctx.lineTo(r.right, r.top);
    ctx.lineTo(r.right, r.bottom);
    ctx.lineTo(r.left, r.bottom);
    ctx.lineTo(r.left, r.top);

    ctx.strokeStyle = stroke;
    ctx.lineWidth = width;
    ctx.stroke();
}

function editorToScreenPoint(ep) {
    let editor_rect = getEditorContainerRect();
    return { x: ep.x + editor_rect.left, y: ep.y + editor_rect.top };
}

function screenToEditorPoint(sp) {
    let editor_rect = getEditorContainerRect();
    return { x: sp.x - editor_rect.left, y: sp.y - editor_rect.top };
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

function screenToEditorRect(sp) {
    let editor_rect = getEditorContainerRect();
    return { x: sp.x - editor_rect.left, y: sp.y - editor_rect.top };
}
