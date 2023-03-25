function roundRectCornerCap(ctx, x, y, radius = 5, corner = 'tr', fill = true, stroke = false) {
    ctx.beginPath();

    let cap_radius = radius / 2;
    //move to origin
    ctx.moveTo(x, y);
    if (corner == 'tr') {
        // line origin to top
        ctx.lineTo(x, y - cap_radius);
        //tr curve
        ctx.quadraticCurveTo(x, y, x + cap_radius, y);
        ctx.closePath();
    } else if (corner == 'br') {
        // line origin to bottom
        ctx.lineTo(x, y + cap_radius);
        //br curve
        ctx.quadraticCurveTo(x, y, x + cap_radius, y);
        ctx.closePath();
    } else if (corner == 'bl') {
        // line origin to bottom
        ctx.lineTo(x, y + cap_radius);
        //bl curve
        ctx.quadraticCurveTo(x, y, x - cap_radius, y);
        ctx.closePath();
    } else if (corner == 'tl') {
        // line origin to top
        ctx.lineTo(x, y - cap_radius);
        //tl curve
        ctx.quadraticCurveTo(x, y, x - cap_radius, y);
        ctx.closePath();
    }

    if (fill) {
        ctx.fill();
    }
    if (stroke) {
        ctx.stroke();
    }
}

function roundRect(
    ctx,
    x,
    y,
    width,
    height,
    radius,
    fill = false,
    stroke = true) {
    // from https://stackoverflow.com/questions/1255512/how-to-draw-a-rounded-rectangle-using-html-canvas
    /**
     * Draws a rounded rectangle using the current state of the canvas.
     * If you omit the last three params, it will draw a rectangle
     * outline with a 5 pixel border radius
     * @param {CanvasRenderingContext2D} ctx
     * @param {Number} x The top left x coordinate
     * @param {Number} y The top left y coordinate
     * @param {Number} width The width of the rectangle
     * @param {Number} height The height of the rectangle
     * @param {Number} [radius = 5] The corner radius; It can also be an object 
     *                 to specify different radii for corners
     * @param {Number} [radius.tl = 0] Top left
     * @param {Number} [radius.tr = 0] Top right
     * @param {Number} [radius.br = 0] Bottom right
     * @param {Number} [radius.bl = 0] Bottom left
     * @param {Boolean} [fill = false] Whether to fill the rectangle.
     * @param {Boolean} [stroke = true] Whether to stroke the rectangle.
    */


    if (typeof radius === 'number') {
        radius = { tl: radius, tr: radius, br: radius, bl: radius };
    } else {
        radius = { ...{ tl: 0, tr: 0, br: 0, bl: 0 }, ...radius };
    }

    // MY CHANGES BEGIN

    //radius.tl = radius.tl < 0 ? 0 : radius.tl;
    //radius.tr = radius.tr < 0 ? 0 : radius.tr;
    //radius.br = radius.br < 0 ? 0 : radius.br;
    //radius.bl = radius.bl < 0 ? 0 : radius.bl;

    // MY CHANGES END

    //ctx.beginPath();
    //// move to tl top
    //ctx.moveTo(x + radius.tl, y);
    ////line to tr top
    //ctx.lineTo(x + width - radius.tr, y);
    ////tr curve
    //ctx.quadraticCurveTo(x + width, y, x + width, y + radius.tr);
    ////tr2br line
    //ctx.lineTo(x + width, y + height - radius.br);
    //// br curve
    //ctx.quadraticCurveTo(x + width, y + height, x + width - radius.br, y + height);
    ////br2bl line
    //ctx.lineTo(x + radius.bl, y + height);
    //// bl curve
    //ctx.quadraticCurveTo(x, y + height, x, y + height - radius.bl);
    //// bl2tl line
    //ctx.lineTo(x, y + radius.tl);
    //// tl curve
    //ctx.quadraticCurveTo(x, y, x + radius.tl, y);
    //ctx.closePath();

    ctx.beginPath();
    ctx.moveTo(x + radius.tl, y);
    ctx.lineTo(x + width - radius.tr, y);
    ctx.quadraticCurveTo(x + width, y, x + width, y + radius.tr);
    ctx.lineTo(x + width, y + height - radius.br);
    ctx.quadraticCurveTo(x + width, y + height, x + width - radius.br, y + height);
    ctx.lineTo(x + radius.bl, y + height);
    ctx.quadraticCurveTo(x, y + height, x, y + height - radius.bl);
    ctx.lineTo(x, y + radius.tl);
    ctx.quadraticCurveTo(x, y, x + radius.tl, y);
    ctx.closePath();

    if (fill) {
        ctx.fill();
    }
    if (stroke) {
        ctx.stroke();
    }

    // Now you can just call
    //var ctx = document.getElementById("rounded-rect").getContext("2d");
    //// Draw using default border radius,
    //// stroke it but no fill (function's default values)
    //roundRect(ctx, 5, 5, 50, 50);
    //// To change the color on the rectangle, just manipulate the context
    //ctx.strokeStyle = "rgb(255, 0, 0)";
    //ctx.fillStyle = "rgba(255, 255, 0, .5)";
    //roundRect(ctx, 100, 5, 100, 100, 20, true);
    //// Manipulate it again
    //ctx.strokeStyle = "#0f0";
    //ctx.fillStyle = "#ddd";
    //// Different radii for each corner, others default to 0
    //roundRect(ctx, 300, 5, 200, 100, {
    //    tl: 50,
    //    br: 25
    //}, true);
}

function drawRoundedRect(ctx, sharp_rect, corner_radii, fill = 'black', stroke = 'black', lineWidth = 0, opacity = 1.0) {
    let strokStyleStr = cleanColor(stroke, opacity != 1.0 ? opacity : null,'rgbaStyle');
    ctx.strokeStyle = strokStyleStr;
    let fillStyleStr = cleanColor(fill, opacity != 1.0 ? opacity : null, 'rgbaStyle');
    ctx.fillStyle = fillStyleStr;
    ctx.lineWidth = lineWidth;
    let draw_lines = lineWidth > 0;
    let draw_fill = true;

    roundRect(ctx, sharp_rect.left, sharp_rect.top, sharp_rect.width, sharp_rect.height, corner_radii, draw_fill, draw_lines);

    //   if (corner_radii.tl < 0) {
    //       roundRectCornerCap(ctx, sharp_rect.left, sharp_rect.top, -corner_radii.tl, 'tl',draw_fill, draw_lines)
    //}
    //   if (corner_radii.tr < 0) {
    //       roundRectCornerCap(ctx, sharp_rect.right, sharp_rect.top, -corner_radii.tr, 'tr', draw_fill, draw_lines)
    //   }
    //   if (corner_radii.br < 0) {
    //       roundRectCornerCap(ctx, sharp_rect.right, sharp_rect.bottom, -corner_radii.br, 'br', draw_fill, draw_lines)
    //   }
    //   if (corner_radii.bl < 0) {
    //       roundRectCornerCap(ctx, sharp_rect.left, sharp_rect.bottom, -corner_radii.bl, 'bl', draw_fill, draw_lines)
    //   }
}