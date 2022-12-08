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

    scrollDocRangeIntoView(getDocSelection());

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

    if (isLeave && !isDragging()) {
        scrollToHome();
    }
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

// #endregion Event Handlers