function getLinesMinMaxY(lines) {
    let min_y = lines.length > 0 ? getLineMinY(lines.sort((a, b) => getLineMinY(a) < getLineMinY(b))[0]) : 0;
    let max_y = lines.length > 0 ? getLineMaxY(lines.sort((a, b) => getLineMaxY(a) < getLineMaxY(b))[0]) : 0;
    return {
        min: min_y,
        max: max_y
    };
}

function getLineMinY(line) {
    return Math.min(line.y1, line.y2);
}
function getLineMaxY(line) {
    return Math.max(line.y1, line.y2);
}

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
