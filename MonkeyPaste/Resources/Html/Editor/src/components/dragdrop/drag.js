var IsDragging = false;
var DragRange = null;

var WasNoneSelectedBeforeDrag = false;
var SelIdxBeforeDrag = -1;

var IsCtrlDown = false; //duplicate
var IsShiftDown = false; //split 
var IsAltDown = false; // w/ formatting (as html)? ONLY formating? dunno

function initDrag() {
   // document.addEventListener('DOMContentLoaded', (event) => {

        function handleDragStart(e) {
            if (IsDragging) {
                return;
		    }

            let sel = getEditorSelection();

            if (!sel || sel.length == 0) {
                e.preventDefault();
                e.stopPropagation();
                return false;

    //            if (!sel || IsSubSelectionEnabled) {
    //                e.preventDefault();
    //                e.stopPropagation();
    //                return false;
				//}
    //            selectAll();
    //            sel = getEditorSelection();
            }

            log('drag start');
            DragRange = sel;
            IsDragging = true;
            e.stopPropagation();

            e.dataTransfer.effectAllowed = 'copyMove';
            let textStr = getText(sel);
            e.dataTransfer.setData('text/plain', textStr);
            let htmlStr = getHtml(sel);
            e.dataTransfer.setData('text/html', htmlStr);
            let deltaJsonStr = getDeltaJson(sel);
            e.dataTransfer.setData('application/json/quill-delta', deltaJsonStr);

            if (isAllSelected()) {
                e.dataTransfer.setData('application/mp', ContentHandle);
            }
            return true;
        }

        function handleDragEnd(e) {
            log('drag end');

            DragRange = null;
            IsDragging = false;
            IsCtrlDown = false;
            IsAltDown = false
            IsShiftDown = false;


            drawOverlay();
        }

        let items = [getEditorContainerElement()];
            items.forEach(function (item) {
                item.addEventListener('dragstart', handleDragStart, true);
                item.addEventListener('dragend', handleDragEnd);
            });
  //  });
}

function isDragging() {
    return DragRange != null;
}