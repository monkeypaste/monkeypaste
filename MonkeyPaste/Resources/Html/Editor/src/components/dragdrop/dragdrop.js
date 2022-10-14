
const MIN_DRAG_DIST = 10;


function initDragDrop() {
    initDrop();
    initDrag();
}

function resetDragDrop(fromHost = false, wasDragCanceled = false) {
    if (isRunningInHost() && !fromHost) {
        return;
	}
    if (IsDragging) {
        
    }
    
    
   

    
}
