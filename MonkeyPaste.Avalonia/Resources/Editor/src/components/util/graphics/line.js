function drawLine(ctx, line, stroke = 'black', width = 1, dash = [1, 0]) {
    if (!line) {
        return;
    }
    if (line.ignoreLineStyle !== undefined) {
        // NOTE this is used so caret preview isn't dashed
        dash = [1, 0];
    }
    ctx.setLineDash(dash);

    ctx.beginPath();

    ctx.moveTo(line.x1, line.y1);
    ctx.lineTo(line.x2, line.y2);

    ctx.strokeStyle = stroke;
    ctx.lineWidth = width;
    ctx.stroke();
}

function cleanLine(line) {
    let cleaned_line = {
        x1: line ? line.x1 : 0,
        y1: line ? line.y1 : 0,
        x2: line ? line.x2 : 0,
        y2: line ? line.y2 : 0,
    };
    cleaned_line.height = Math.abs(cleaned_line.y2 - cleaned_line.y1);
    return cleaned_line;
}
