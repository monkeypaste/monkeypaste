// #region Globals

var SuppressNextEditorScrollChangedNotification = false;

var LastScrollBarXIsVisible = false;
var LastScrollBarYIsVisible = false;

// #endregion Globals

// #region Life Cycle

function initScroll() {
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

function getVerticalScrollBarWidth() {
    return getEditorContainerElement().offsetWidth - getEditorContainerElement().clientWidth;
}

function getHorizontalScrollBarHeight() {
    return getEditorContainerElement().offsetHeight - getEditorContainerElement().clientHeight;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

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

function isScrollBarXVisible() {
    return getEditorContainerElement().scrollWidth > getEditorContainerElement().clientWidth;
}
function isScrollBarYVisible() {
    return getEditorContainerElement().scrollHeight > getEditorContainerElement().clientHeight;
}

// #endregion State

// #region Actions

function scrollDocRangeIntoView(docRange, opts) {
    // opts as bool = alignToTop
    // opts as obj = behavior (smooth|instant|auto), block (*start*|center|end|nearest), inline (start|center|end|*nearest*)
    const def_start_opts = { behavior: 'auto', block: 'start', inline: 'nearest' };
    const def_end_opts = { behavior: 'auto', block: 'end', inline: 'nearest' };

    const start_elm = getElementAtDocIdx(docRange.index, true);
    const end_elm = getElementAtDocIdx(Math.min(getDocLength()-1,docRange.index + docRange.length), true);

    start_elm.scrollIntoView(isNullOrUndefined(opts) ? def_start_opts : opts);
    end_elm.scrollIntoView(isNullOrUndefined(opts) ? def_end_opts : opts);

    //let dom_range = convertDocRangeToDomRange(docRange);
    //let scroll_elm = dom_range.endContainer;
    //if (!scroll_elm) {
    //    scroll_elm = dom_range.startContainer;
    //    if (!scroll_elm) {
    //        log('error scrolling to doc range: ' + docRange);
    //        return;
    //    }
    //}
    //if (scroll_elm.nodeType === 3) {
    //    //let docRange_rects = getRangeRects(docRange);
    //    //if (!docRange_rects || docRange_rects.length == 0) {
    //    //    scroll_elm = scroll_elm.parentNode;
    //    //} else {
    //    //    // clear scroll elm to disable element scroll and scroll manually by rect
    //    //    scroll_elm = null;
    //    //    getEditorContainerElement().scrollTop = docRange_rects[0].bottom;
    //    //}
    //    scroll_elm = scroll_elm.parentNode;
    //}
    //if (scroll_elm) {
    //    scroll_elm.scrollIntoView(false);
    //}

    ////getEditorContainerElement().scrollLeft += extraX;
    ////getEditorContainerElement().scrollTop += extraY;

    ////let new_scroll_x = extraX;
    ////let new_scroll_y = extraY;

    ////let docRange_rects = getRangeRects(docRange);
    ////if (!docRange_rects || docRange_rects.length == 0) {
    ////    log('scroll to range error, no rects found for range: ' + docRange);
    ////    return;
    ////}
    ////new_scroll_y += docRange_rects[docRange_rects.length - 1].bottom;
    ////// ignoring x for now...
    ////getEditorContainerElement().scrollTop = new_scroll_y;
}

function showElementScrollbars(elm) {
    if (!elm) {
        return;
    }
    elm.classList.add('show-scrollbars');
    elm.classList.remove('hide-scrollbars');
}

function hideElementScrollbars(elm) {
    if (!elm) {
        return;
    }

    elm.classList.remove('show-scrollbars');
    elm.classList.add('hide-scrollbars');
}

function setEditorScroll(new_scroll) {
    getEditorContainerElement().scrollLeft = new_scroll.left;
    getEditorContainerElement().scrollTop = new_scroll.top;
}


function hideAllScrollbars() {
    hideEditorScrollbars();
    hideTableScrollbars();
}

function showAllScrollbars() {
    showEditorScrollbars();
    showTableScrollbars();
}


function scrollToHome() {
    getEditorContainerElement().scrollTop = 0;
}

function scrollToEnd() {
    getEditorContainerElement().scrollTop = getEditorElement().offsetHeight;
}

function updateScrollBarSizeAndPositions() {
    const cur_x = isScrollBarXVisible();
    const cur_y = isScrollBarYVisible();
    if (cur_x != LastScrollBarXIsVisible ||
        cur_y != LastScrollBarYIsVisible) {
        onScrollBarVisibilityChanged_ntf(cur_x, cur_y);

    }
    LastScrollBarXIsVisible = cur_x;
    LastScrollBarYIsVisible = cur_y;
}

// #endregion Actions

// #region Event Handlers


function onEditorContainerScroll(e) {
    updateFindReplaceRangeRects();
    //if (isShowingFindReplaceToolbar()) {
        
    //} //else if (BlurredSelectionRects) {
    //       // TODO update these guys
    //}

    //if (SuppressNextEditorScrollChangedNotification) {
    //    SuppressNextEditorScrollChangedNotification = false;
    //} else {
    //    onScrollChanged_ntf(getEditorScroll());
    //}
    drawOverlay();
}
// #endregion Event Handlers