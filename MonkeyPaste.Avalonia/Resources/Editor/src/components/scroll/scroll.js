
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

function getEditorVerticalScrollBarWidth() {
    return getEditorContainerElement().offsetWidth - getEditorContainerElement().clientWidth;
}

function getEditorHorizontalScrollBarHeight() {
    return getEditorContainerElement().offsetHeight - getEditorContainerElement().clientHeight;
}

function getScrollableElements() {
    let scroll_elms = [getEditorContainerElement()];

    let table_elms = getTableElements();
    for (var i = 0; i < length; i++) {
        scroll_elms.push(table_elms[i]);
    }
    return scroll_elms;
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

function isAnyVerticalScrollBarVisible() {
    let scroll_elms = getScrollableElements();
    for (var i = 0; i < scroll_elms.length; i++) {
        let scroll_elm = scroll_elms[i];
        if (!scroll_elm || scroll_elm.scrollHeight === undefined || scroll_elm.clientHeight == undefined) {
            continue;
        }
        if (scroll_elm.scrollHeight > scroll_elm.clientHeight) {
            return true;
        }
    }
    return false;
}
function isAnyHorizontalScrollBarVisible() {
    let scroll_elms = getScrollableElements();
    for (var i = 0; i < scroll_elms.length; i++) {
        let scroll_elm = scroll_elms[i];
        if (!scroll_elm || scroll_elm.scrollWidth === undefined || scroll_elm.clientWidth == undefined) {
            continue;
        }
        if (scroll_elm.scrollWidth > scroll_elm.clientWidth) {
            return true;
        }
    }
    return false;
}


// #endregion State

// #region Actions

function scrollToSelState(selStateOrRangeObj) {
    if (!selStateOrRangeObj) {
        return;
    }
    if (isNullOrUndefined(selStateOrRangeObj.scrollLeft) ||
        isNullOrUndefined(selStateOrRangeObj.scrollTop)) {
        // presume param is a reload sel range
        scrollDocRangeIntoView(selStateOrRangeObj);
        return;
    }
    getEditorContainerElement().scrollLeft = parseFloat(selStateOrRangeObj.scrollLeft);
    getEditorContainerElement().scrollTop = parseFloat(selStateOrRangeObj.scrollTop);

}
function scrollToEditorLoc(x, y) {
    getEditorContainerElement().scrollTo(x, y);
}
function scrollDocRangeIntoView(docRange, opts) {
    // opts as bool = alignToTop
    // opts as obj = behavior (smooth|instant|auto), block (*start*|center|end|nearest), inline (start|center|end|*nearest*)
    const start_opts = opts && opts.start ? opts.start : { behavior: 'auto', block: 'start', inline: 'nearest' };
    const end_opts = opts && opts.end ? opts.end : { behavior: 'auto', block: 'end', inline: 'nearest' };

    const start_elm = getElementAtDocIdx(docRange.index, true);
    const end_elm = getElementAtDocIdx(Math.min(getDocLength()-1,docRange.index + docRange.length), true);

    start_elm.scrollIntoView(start_opts);
    if (docRange.length > 0) {
        // NOTE for big (multiline spans for example) don't include end 
        end_elm.scrollIntoView(end_opts);
    }
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
    updateScrollBarSizeAndPositions();
}

function showAllScrollbars() {
    showEditorScrollbars();
    showTableScrollbars();
    updateScrollBarSizeAndPositions();
}


function scrollToHome() {
    getEditorContainerElement().scrollTop = 0;
}

function scrollToEnd() {
    getEditorContainerElement().scrollTop = getEditorElement().offsetHeight;
}

function updateScrollBarSizeAndPositions() {
    const cur_x_vis = isAnyHorizontalScrollBarVisible();
    const cur_y_vis = isAnyVerticalScrollBarVisible();
    if (cur_x_vis != globals.LastHorizontalScrollBarIsVisible ||
        cur_y_vis != globals.LastVerticalScrollBarIsVisible) {
        onScrollBarVisibilityChanged_ntf(cur_x_vis, cur_y_vis);
    }
    globals.LastVerticalScrollBarIsVisible = cur_y_vis;
    globals.LastHorizontalScrollBarIsVisible = cur_x_vis;
}

// #endregion Actions

// #region Event Handlers


function onEditorContainerScroll(e) {
    updateFindReplaceRangeRects();
    drawOverlay();
}
// #endregion Event Handlers