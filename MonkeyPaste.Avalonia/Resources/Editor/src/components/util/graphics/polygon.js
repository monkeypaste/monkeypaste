
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




