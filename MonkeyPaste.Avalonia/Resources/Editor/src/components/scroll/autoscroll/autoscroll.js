
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
    let actual_content_width = 0;
    let actual_content_height = 0;
    let lines = getLineCount();
    for (var i = 0; i < lines; i++) {
        let line_rect = getLineRect(i, false);
        actual_content_width = Math.max(actual_content_width, line_rect.width);
        if (i == lines - 1) {
            actual_content_height = line_rect.bottom;
        }
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

    let container_height = getEditorContainerElement().getBoundingClientRect().height;
    let adjusted_editor_height = Math.max(actual_content_height, container_height);

    editor_elm.style.width = `${adjusted_editor_width}px`;
    editor_elm.style.height = `${adjusted_editor_height}px`;

    scrollDocRangeIntoView(getDocSelection());

    globals.AutoScrollInterval = setInterval(onAutoScrollTick, 300, editor_elm);
}

function stopAutoScroll(isLeave) {
    if (globals.AutoScrollInterval == null) {
        return;
    }
    getEditorElement().style.width = '';
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