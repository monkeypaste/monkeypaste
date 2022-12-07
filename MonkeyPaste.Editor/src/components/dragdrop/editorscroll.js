// #region Globals

var SuppressNextEditorScrollChangedNotification = false;

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

function getEditorScroll() {
    return {
        left: parseInt(getEditorContainerElement().scrollLeft),
        top: parseInt(getEditorContainerElement().scrollTop)
    };
}
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

function didEditorScrollChange(old_scroll, new_scroll) {
    if (!old_scroll && !new_scroll) {
        return false;
    }
    if (old_scroll && !new_scroll) {
        return true;
    }
    if (new_scroll && !old_scroll) {
        return true;
    }
    return old_scroll.left != new_scroll.left || old_scroll.top != new_scroll.top;
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

    scrollDocRangeIntoView(DragSelectionRange);

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

function scrollDocRangeIntoView(docRange) {
    let dom_range = convertDocRangeToDomRange(docRange);
    let scroll_elm = dom_range.endContainer;
    if (!scroll_elm) {
        scroll_elm = dom_range.startContainer;
        if (!scroll_elm) {
            log('error scrolling to doc range: ' + docRange);
            return;
        }
    }
    if (scroll_elm.nodeType === 3) {
        let docRange_rects = getRangeRects(docRange);
        if (!docRange_rects || docRange_rects.length == 0) {
            scroll_elm = scroll_elm.parentNode;
        } else {
            // clear scroll elm to disable element scroll and scroll manually by rect
            scroll_elm = null;
            getEditorContainerElement().scrollTop = docRange_rects[0].top;
        }
    } 
    if (scroll_elm) {
        scroll_elm.scrollIntoView();
    }
}


function setEditorScroll(new_scroll) {
    getEditorContainerElement().scrollLeft = new_scroll.left;
    getEditorContainerElement().scrollTop = new_scroll.top;
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
    let scroll_elm = getEditorContainerElement();//document.body;

    let orig_scroll_x = scroll_elm.scrollLeft;
    let orig_scroll_y = scroll_elm.scrollTop;

    if (Math.abs(window_rect.right - WindowMouseLoc.x) <= MIN_AUTO_SCROLL_DIST) {
        scroll_elm.scrollLeft += AutoScrollVelX;
    } else if (Math.abs(window_rect.left - WindowMouseLoc.x) <= MIN_AUTO_SCROLL_DIST) {
        scroll_elm.scrollLeft -= AutoScrollVelX;
    }

    if (orig_scroll_x != scroll_elm.scrollLeft) {
        AutoScrollVelX += AutoScrollAccumlator;
    }
}
function onEditorContainerScroll(e) {
 //   if (isShowingFindReplaceToolbar()) {
 //       updateFindReplaceRangeRects();
 //   } else if (BlurredSelectionRects) {
 //       // TODO update these guys
    //}

    if (SuppressNextEditorScrollChangedNotification) {
        SuppressNextEditorScrollChangedNotification = false;
    } else {
        onScrollChanged_ntf(getEditorScroll());
    }
    drawOverlay();
}
// #endregion Event Handlers