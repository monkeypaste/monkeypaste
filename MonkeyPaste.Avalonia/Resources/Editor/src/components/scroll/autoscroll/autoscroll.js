
// #region Life Cycle

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isAutoScrolling() {
    if (!globals.AutoScrollInterval || (globals.AutoScrollVelX == 0 && globals.AutoScrollVelX == 0)) {
        return false;
    }
    return globals.AutoScrollVelX != 0 || globals.AutoScrollVelY != 0;
}

// #endregion State

// #region Actions

function startAutoScroll() {
    // .drop class makes .ql-editor huge so no wrapping
    // this finds actual max width and sets so won't overscroll...
    unwrapContentScroll();

    scrollDocRangeIntoView(getDocSelection());

    globals.AutoScrollInterval = setInterval(onAutoScrollTick, 300, getEditorElement());
}

function stopAutoScroll(isLeave) {
    if (globals.AutoScrollInterval == null) {
        return;
    }
    wrapContentScroll();
    clearInterval(globals.AutoScrollInterval);
    globals.AutoScrollInterval = null;
    globals.AutoScrollVelX = 0;
    globals.AutoScrollVelY = 0;

    if (isLeave && !isDragging()) {
        scrollToHome();
    }
}
// #endregion Actions

// #region Event Handlers

function onAutoScrollTick(e) {
    if (globals.WindowMouseLoc == null) {
        log('auto-scroll reject, mp is null');
        return;
    }
    let window_rect = getWindowRect();
    if (!isPointInRect(window_rect, globals.WindowMouseLoc)) {
        log('auto-scroll rejected mp ' + globals.WindowMouseLoc.x + ',' + globals.WindowMouseLoc.y + ' is outside ')
        return;
    }
    //debugger;
    let scroll_elm = getEditorContainerElement();//document.body;

    let orig_scroll_x = scroll_elm.scrollLeft;
    let orig_scroll_y = scroll_elm.scrollTop;

    if (Math.abs(window_rect.right - globals.WindowMouseLoc.x) <= globals.MIN_AUTO_SCROLL_DIST) {
        scroll_elm.scrollLeft += globals.AutoScrollVelX;
    } else if (Math.abs(window_rect.left - globals.WindowMouseLoc.x) <= globals.MIN_AUTO_SCROLL_DIST) {
        scroll_elm.scrollLeft -= globals.AutoScrollVelX;
    }

    if (orig_scroll_x != scroll_elm.scrollLeft) {
        globals.AutoScrollVelX += globals.AutoScrollAccumlator;
    }
}

// #endregion Event Handlers