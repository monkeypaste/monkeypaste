var IsDropping = false;
var DropIdx = -1;
var IsDropCancel = false; // flagged from drag_end  evt resetDragDrop then unset in editorSelectionChange which restores selection

var LastDragOverDateTime = null;

var IsSplitDrop = false;
var IsPreBlockDrop = false;
var IsPostBlockDrop = false;

var DropEffect = 'none';

var PreDropState = null;

const AllowedEffects = ['copy', 'copyLink', 'copyMove', 'link', 'linkMove', 'move'];

function initDrop() {
   // document.addEventListener('DOMContentLoaded', (event) => {

        function handleDragEnter(e) {
            if (IsDropping) {
                // NOTE called on every element drag enters, only need once
                return;
            }
            log('drag enter');
            IsDropping = true;
            WindowMouseLoc = null;
            items.forEach(function (item) {
                item.classList.add('drop');
            });

            enableSubSelection();
        }

        function handleDragOver(e) {
            let emp = getEditorMousePos(e);

            e.preventDefault();
            // VALIDATE

            
            if (!isDataTransferDataValid(e.dataTransfer)) {
                return false;
            }


            // DROP EFFECT
            let isModChanged =
                IsCtrlDown != e.ctrlKey ||
                IsAltDown != e.altKey ||
                IsShiftDown != e.shiftKey;

            IsCtrlDown = e.ctrlKey;
            IsAltDown = e.altKey;
            IsShiftDown = e.shiftKey;

            if (isModChanged) {
                log('mod changed: Ctrl: ' + (IsCtrlDown ? "YES" : "NO"));
                drawOverlay();
            }

            if (!AllowedEffects.includes(e.dataTransfer.effectAllowed)) {
                return false;
            }

            if (isDragCopy()) {
                e.dataTransfer.dropEffect = 'copy';
            } else if (isDragCut()) {
                e.dataTransfer.dropEffect = 'move';
            } else {
                e.dataTransfer.dropEffect = 'none';
                return false;
			}


            // DEBOUNCE (my own type but word comes from https://css-tricks.com/debouncing-throttling-explained-examples/)
            let min_drag_mouse_delta_dist = 1;
            let cur_date_time = Date.now();

            LastDragOverDateTime = LastDragOverDateTime == null ? cur_date_time : LastDragOverDateTime;
            let m_dt = LastDragOverDateTime - cur_date_time;

            WindowMouseLoc = WindowMouseLoc == null ? emp : WindowMouseLoc;
            let m_delta_dist = dist(emp, WindowMouseLoc);
            let m_v = m_delta_dist / m_dt;

            WindowMouseLoc = emp;
            LastDragOverDateTime = cur_date_time;

            let debounce = m_delta_dist != 0 || m_v != 0;
            if (debounce) {
                return false;
			}
            // DROP IDX

            DropIdx = getDocIdxFromPoint(emp);
            drawOverlay();

            return false;
        }


        function handleDragLeave(e) {
            let emp = getEditorMousePos(e);
            let editor_rect = getEditorContainerRect();
            if (isPointInRect(editor_rect, emp)) {
                return;
			}

            log('drag leave');
            IsDropping = false;
            DropIdx = -1;

            items.forEach(function (item) {
                item.classList.remove('drop');
            });

            if (isReadOnly() && !IsDragging) {
                disableSubSelection();
            }
            drawOverlay();
        }

        function handleDrop(e) {
            e.stopPropagation(); // stops the browser from redirecting.

            log('drop');

            IsDropping = false;
            DropIdx = -1;

            items.forEach(function (item) {
                item.classList.remove('drop');
            });

            if (isReadOnly() && !IsDragging) {
                disableSubSelection();
            }
            drawOverlay();
            return false;
        }

        let items = [getEditorContainerElement()];
        items.forEach(function (item) {
            item.addEventListener('dragenter', handleDragEnter,true);
            item.addEventListener('dragover', handleDragOver, true);
            item.addEventListener('dragleave', handleDragLeave, true);
            item.addEventListener('drop', handleDrop, true);
        });
   // });
}
