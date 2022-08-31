function drawLine(ctx, x1, y1, x2, y2, stroke = 'black', width = 1, dash = [1, 0]) {
    ctx.setLineDash(dash);

    ctx.beginPath();

    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);

    ctx.strokeStyle = stroke;
    ctx.lineWidth = width;
    ctx.stroke();
}

function drawRect(ctx, r, stroke = 'black', width = 1) {
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


function getEditorRect(clean = true) {
    //return { left: 0, top: 0, right: window.outerWidth, bottom: window.outerHeight, width: window.outerWidth, height: window.outerHeight };
    let temp = document.getElementById("editor").getBoundingClientRect();
    temp = cleanRect(temp);
    if (clean) {
        temp.right = temp.width;
        temp.bottom = temp.height;
        temp.left = 0;
        temp.top = 0;
        temp = cleanRect(temp);
	}
    return temp;
}
