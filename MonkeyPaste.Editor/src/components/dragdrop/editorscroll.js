// #region Globals
var AutoScrolledOffset = null;

var AutoScrollVelX = 0;
var AutoScrollVelY = 0;

var AutoScrollAccumlator = 5;
var AutoScrollBaseVelocity = 25;

const MIN_AUTO_SCROLL_DIST = 30;

var AutoScrollInterval = null;
// #endregion Globals

// #region Life Cycle

function initEditorScroll() {
    getEditorContainerElement().addEventListener('scroll', onEditorContainerScroll);
}
// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isAutoScrolling() {
    if (!AutoScrollInterval || (AutoScrollVelX == 0 && AutoScrollVelX == 0)) {
        return false;
    }
    return AutoScrollVelX != 0 || AutoScrollVelY != 0;
}
// #endregion State

// #region Actions

function startAutoScroll() {
    //AutoScrolledOffset
    // drop class makes .ql-editor huge so no wrapping this finds actual max width and sets so won't overscroll...
    let actual_content_width = 0;
    //let max_y = 0;
    let lines = getLineCount();
    for (var i = 0; i < lines; i++) {
        let line_rect = getLineRect(i, false);
        actual_content_width = Math.max(actual_content_width, line_rect.width);
        //max_y = Math.max(max_y, line_rect.height);
    }
    let editor_elm = getEditorElement();
    let adjusted_editor_width = editor_elm.getBoundingClientRect().width;

    // add 100 in case template at the end ( i think its from extra spaces or somethign...)
    let container_width = getEditorContainerElement().getBoundingClientRect().width;
    if (actual_content_width > container_width) {
        // only add extra padding if content overflows or h scrollbar will be falsely visible
        // since that pad will always make it bigger
        adjusted_editor_width = actual_content_width + 100;
    } else {
        adjusted_editor_width = container_width;
    }


    editor_elm.style.width = adjusted_editor_width + 'px';

    scrollToDocRange(DragSelectionRange, editor_elm);

    AutoScrollInterval = setInterval(onAutoScrollTick, 300, editor_elm);
}

function stopAutoScroll(isLeave) {
    if (AutoScrollInterval == null) {
        return;
    }
    getEditorElement().style.width = '';
    clearInterval(AutoScrollInterval);
    AutoScrollInterval = null;
    AutoScrollVelX = 0;
    AutoScrollVelY = 0;

    if (isLeave && !IsDragging) {
        scrollToHome();
    }
}

function scrollToDocRange(docRange, target) {
    target = target ? target : getEditorContainerElement();
    if (!docRange) {
        return;
    }
    let range_rects = getRangeRects(docRange);
    if (!range_rects || range_rects.length == 0) {
        scrollToHome();
        return;
    }
    scrollToOffset(range_rects[0], target);
}

function scrollEditorTop(new_top) {
    let eh = getEditorVisibleHeight();
    if (new_top > eh) {
        getEditorContainerElement().scrollTop = new_top - eh;
    } else {
        getEditorContainerElement().scrollTop = 0;
    }
}
function scrollToHome(target) {
    if (!target) {
        // maybe default to editor here?
        return;
    }
    scrollToOffset(null);
}

function scrollToOffset(offset, target) {
    target = !target ? document.body : target;
    offset = !offset ? { left: 0, top: 0 } : offset;

    target.scrollLeft = offset.left;
    target.scrollTop = offset.top;
}
// #endregion Actions

// #region Event Handlers

function onAutoScrollTick(e) {
    if (WindowMouseLoc == null) {
        log('auto-scroll reject, mp is null');
        return;
    }
    let window_rect = getWindowRect();
    if (!isPointInRect(window_rect, WindowMouseLoc)) {
        log('auto-scroll rejected mp ' + WindowMouseLoc.x + ',' + WindowMouseLoc.y + ' is outside ')
        return;
    }
    //debugger;

    let orig_scroll_x = document.body.scrollLeft;
    let orig_scroll_y = document.body.scrollTop;

    if (Math.abs(window_rect.right - WindowMouseLoc.x) <= MIN_AUTO_SCROLL_DIST) {
        document.body.scrollLeft += AutoScrollVelX;
    } else if (Math.abs(window_rect.left - WindowMouseLoc.x) <= MIN_AUTO_SCROLL_DIST) {
        document.body.scrollLeft -= AutoScrollVelX;
    }

    if (orig_scroll_x != document.body.scrollLeft) {
        AutoScrollVelX += AutoScrollAccumlator;
    }
}
function onEditorContainerScroll(e) {
 //   if (isShowingFindReplaceToolbar()) {
 //       updateFindReplaceRangeRects();
 //   } else if (BlurredSelectionRects) {
 //       // TODO update these guys
    //}
    drawOverlay();
}
// #endregion Event Handlers