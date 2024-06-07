// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters
async function getEditorAsImageAsync() {
    var img_base64 = null;
    var sw = performance.now();
    //let disp = document.getElementById('overlayCanvas').style.display;
    //document.getElementById('overlayCanvas').style.display = 'none';

    html2canvas(
        document.body,
        {
            backgroundColor: '#00ff00'
            //allowTaint: true,
            //logging: true,
            //ignoreElements: (element) => {
            //    if (element == document.body) {
            //        //return false;
            //    }
            //    if (isChildOfElement(element, document.head, true) ||
            //        isChildOfElement(element, getEditorContainerElement(), true)) {
            //        return false;
            //    }
            //    return true;
            //}
        }
    ).then(
        function (canvas) {
            //document.body.appendChild(canvas);
            let canvas_url = canvas.toDataURL('image/png');
            canvas_url = canvas_url.split(',')[1];
            log(canvas_url);
            img_base64 = canvas_url;
            //document.body.removeChild(canvas);
        }
    );
    //html2canvas(document.body).then(canvas => {
    //    img_base64 = canvas.toDataURL("image/png").split(';base64,')[1];
    //});
    while (true) {
        if (img_base64 != null) {
            break;
        }
        if (performance.now() - sw > 3000) {
            // timeout
            break;
        }
        await delay(100);
    }
    //document.getElementById('overlayCanvas').style.display = disp;
    //return img_base64;
}

async function getDocRangeAsImageAsync(sel) {
    // from https://stackoverflow.com/a/41585230/105028
    let crop_rects = null;
    if (sel) {
        crop_rects = getRangeRects(sel, false);
    }
    let editor_elm = getEditorContainerElement();

    let base64Str = null;

    let bgColor = findElementBackgroundColor(editor_elm, 'white');

    let oldWidth = editor_elm.style.width;
    let oldHeight = editor_elm.style.height;
    let oldOverflow = editor_elm.style.overflow;

    //let oldScrollX = window.scrollX;
    //let oldScrollY = window.scrollY;


    var h2c_options = {
        backgroundColor: bgColor,
        allowTaint: false,
        logging: false,
        //scrollX: -window.scrollX,
        //scrollY: -window.scrollY
    };

    var total_rect = rectsUnion(crop_rects);
    if (crop_rects && crop_rects.length > 0) {
        h2c_options.x = total_rect.left;
        h2c_options.y = total_rect.top;
        h2c_options.width = total_rect.width;
        h2c_options.height = total_rect.height;
    }

    //editor_elm.style.width = 'auto';
    //editor_elm.style.height = 'auto';
    //editor_elm.style.overflow = 'visible';

    let imgCanvas = await html2canvas(editor_elm, h2c_options);
        //.then(imgCanvas => {
            let imgSrcVal = null;
            if (crop_rects && crop_rects.length > 1) {
                // NOTE not sure why but canvas is bigger than total_rect so get ratio
                let scale_x = imgCanvas.width / total_rect.width;
                let scale_y = imgCanvas.height / total_rect.height;
                let ctx = imgCanvas.getContext('2d');
                ctx.resetTransform();
                ctx.fillStyle = bgColor;
                let gap_ranges = getDocRangeLineIntersectRanges(sel);
                for (var i = 0; i < gap_ranges.length; i++) {
                    // NOTE each range should always only have 1 rect
                    // NOTE2 flagged inflateEmptyRange to false so empty range doesn't have width (avoids if sel is line start first char will be omitted)
                    let gap_rect = getRangeRects(gap_ranges[i],true,true,false)[0];

                    gap_rect.right = Math.min(total_rect.right, gap_rect.right);
                    gap_rect = cleanRect(gap_rect);
                    //gap_rect = clampRect(gap_rect, total_rect);
                    // to simplify transforming intersects
                    // they should always be in TL & BR corners
                    let x = gap_rect.left * scale_x;
                    let y = gap_rect.top * scale_y;
                    let w = gap_rect.width * scale_x;
                    let h = gap_rect.height * scale_y;
                    if (i == 0) {
                        // move pre to TL
                        x = 0;
                        y = 0;
                    } else {
                        // move post to BR
                        x = imgCanvas.width - w;
                        y = imgCanvas.height - h;
                    }
                    ctx.fillRect(x,y,w,h);
                }
            } 
            imgSrcVal = imgCanvas.toDataURL("image/png");
            base64Str = imgSrcVal.replace("data:image/png;base64,", "").replace("data:,","");
            editor_elm.style.width = oldWidth;
            editor_elm.style.height = oldHeight;
            editor_elm.style.overflow = oldOverflow;

            //log('oldScrollX ', oldScrollX, ' newScrollX ', window.scrollX);
            //log('oldScrollY ', oldScrollY, ' newScrollY ', window.scrollY);

            //window.scrollX = oldScrollX;
            //window.scrollY = oldScrollY;
       // });

    //while (base64Str == null) {
    //    await delay(100);
    //}

    return base64Str;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions


function cropCanvas2Canvas(imgCanvas, crop_rects, h2c_options, sel) {
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
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers