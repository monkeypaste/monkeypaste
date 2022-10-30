
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
    let sp = { x: ep.x + editor_rect.left, y: ep.y + editor_rect.top };

    return sp;
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

async function getBase64ScreenshotOfElementAsync(element, crop_rects) {
    // from https://stackoverflow.com/a/41585230/105028
    let base64Str = '';

    let bgColor = 'white';

    let oldWidth = element.style.width;
    let oldHeight = element.style.height;
    let oldOverflow = element.style.overflow;

    //let oldScrollX = window.scrollX;
    //let oldScrollY = window.scrollY;

    element.style.width = 'auto';
    element.style.height = 'auto';
    element.style.overflow = 'visible';

    var h2c_options = {
        backgroundColor: bgColor,
        allowTaint: false,
        logging: false,
        //scrollX: -window.scrollX,
        //scrollY: -window.scrollY
    };

    if (crop_rects && crop_rects.length > 0) {
        let crop_block_rect = rectsUnion(crop_rects);

        h2c_options.x = crop_block_rect.left;
        h2c_options.y = crop_block_rect.top;
        h2c_options.width = crop_block_rect.width;
        h2c_options.height = crop_block_rect.height;
    }

    html2canvas(element, h2c_options).then(imgCanvas => {
        let imgSrcVal = null;
        //if (crop_rects && crop_rects.length > 1) {
        //    imgSrcVal = cropCanvas2Canvas(imgCanvas, crop_rects, h2c_options);
        //}
        if (!imgSrcVal) {
            // if no selection cropping or cropping failed?
            imgSrcVal = imgCanvas.toDataURL("image/png");
        }

        base64Str = imgSrcVal.replace("data:image/png;base64,", "");
        element.style.width = oldWidth;
        element.style.height = oldHeight;
        element.style.overflow = oldOverflow;

        //log('oldScrollX ', oldScrollX, ' newScrollX ', window.scrollX);
        //log('oldScrollY ', oldScrollY, ' newScrollY ', window.scrollY);

        //window.scrollX = oldScrollX;
        //window.scrollY = oldScrollY;
    });

    while (base64Str == '') {
        await delay(100);
    }
    return base64Str;
}

function cropCanvas2Canvas(imgCanvas, crop_rects, h2c_options) {
    if (crop_rects && crop_rects.length > 1) {
        // crop selection by making empty canvas same size as screen shot
        // then only copy the rects of selection so overflow is excluded

        let block_ctx = imgCanvas.getContext('2d');
        if (block_ctx) {
            let crop_canvas = document.createElement('canvas');

            document.body.appendChild(crop_canvas);

            //crop_canvas.style.left = imgCanvas.style.left;
            //crop_canvas.style.top = imgCanvas.style.top;

            //crop_canvas.width = h2c_options.width;
            //crop_canvas.height = h2c_options.height;

            crop_canvas.width = imgCanvas.width;
            crop_canvas.height = imgCanvas.height;

            let crop_ctx = crop_canvas.getContext('2d');
            if (crop_ctx) {
                block_ctx.globalCompositeOperation = 'source-out';

                crop_ctx.clearRect(0, 0, crop_canvas.width, crop_canvas.height);
                //let block_img = new Image();
                //block_img.src = imgCanvas.toDataURL();
                //document.body.appendChild(block_img);
                //let scale_x = h2c_options.width / crop_canvas.width;
                //let scale_y = h2c_options.height / crop_canvas.height;

                for (var i = 0; i < crop_rects.length; i++) {
                    let cr = crop_rects[i];

                    /*
                     This variant of the function with 9 arguments allows you to select a portion of the image and use that.
                     The selected area is then resized to fit the width and height that you specify. 
                     This allows you to select a section of your image to display. You should note the order of the arguments. 
                     The area to select X/Y/W/H comes before the displayed X/Y/W/H.

                        The image object (as returned by document.getElementById() )
                        The X coordinate of the area to select
                        The Y coordinate of the area to select
                        The width of the area to select
                        The height of the area to select
                        The X coordinate to draw the image at
                        The Y coordinate to draw the image at
                        The width to draw the image
                        The height to draw the image
                    */
                    //crop_ctx.drawImage(
                    //    block_img,
                    //    cr.left, cr.top, cr.width, cr.height,
                    //    cr.left, cr.top, cr.width, cr.height);
                    //cr.left * scale_x, cr.top * scale_y, cr.width * scale_x, cr.height * scale_y);


                    //crop_ctx.putImageData(block_ctx.getImageData(cr.left, cr.top, cr.width, cr.height), cr.left, cr.top);
                }

                //crop_ctx.scale(scale_x, scale_y);

                imgSrcVal = crop_canvas.toDataURL("image/png");
            }

            document.body.removeChild(crop_canvas);
        }
    }
}
