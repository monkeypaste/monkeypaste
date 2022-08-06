function updateOverlayBounds() {
	let editorRect = document.getElementById('editor').getBoundingClientRect();

	let overlayCanvas = document.getElementById('overlayCanvas');
	overlayCanvas.style.left = editorRect.left;
	overlayCanvas.style.top = editorRect.top;
	overlayCanvas.width = editorRect.width;
	overlayCanvas.height = editorRect.height;
}

function drawLine(x1,y1,x2,y2,style,thickness) {
    let canvas = document.getElementById('overlayCanvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');

    // set line stroke and line width
    ctx.strokeStyle = style;
    ctx.lineWidth = thickness;

    // draw a red line
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

}