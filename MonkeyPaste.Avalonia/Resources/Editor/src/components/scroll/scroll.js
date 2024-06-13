
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
    let scroll_elms = [getEditorContainerElement(), getFindReplaceContainerElement()];

    if (isFontSizeDropDownOpen()) {
        scroll_elms.push(getFontSizeDropDownElement());
    }
    if (isFontFamilyDropDownOpen()) {
        scroll_elms.push(getFontFamilyDropDownElement());
    }

    let table_elms = getTableElements();
    for (var i = 0; i < table_elms.length; i++) {
        scroll_elms.push(table_elms[i]);
    }
    return scroll_elms;
}

function getDefaultScrollStartOpts() {

}
function getDefaultScrollEndOpts() {

}

function getElementsWithVisibleScrollbars() {
    if (!isSubSelectionEnabled()) {
        return [];
    }
    return getScrollableElements().filter(x => isScrollbarVisible(x));
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isScrollbarVisible(scroll_elm) {
    if (isVerticalScrollbarVisible(scroll_elm) || isHorizontalScrollbarVisible(scroll_elm)) {
        return true;
    }
    return false;
}
function isVerticalScrollbarVisible(scroll_elm) {
    if (!scroll_elm || scroll_elm.scrollHeight === undefined || scroll_elm.clientHeight == undefined) {
        return false;
    }
    let overflow_prop = getElementComputedStyleProp(scroll_elm, 'overflow-y');
    if (overflow_prop == 'hidden') {
        return false;
    }
    if (scroll_elm.scrollHeight > scroll_elm.clientHeight) {
        return true;
    }
    return false;
}
function isHorizontalScrollbarVisible(scroll_elm) {
    if (!scroll_elm || scroll_elm.scrollWidth === undefined || scroll_elm.clientWidth == undefined) {
        return false;
    }
    let overflow_prop = getElementComputedStyleProp(scroll_elm, 'overflow-x');
    if (overflow_prop == 'hidden') {
        return false;
    }
    if (scroll_elm.scrollWidth > scroll_elm.clientWidth) {
        return true;
    }
    return false;
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

function unwrapContentScroll() {
    disableWrap();
}
function wrapContentScroll() {
    enableWrap();
}
function scrollToHome() {
    getEditorContainerElement().scrollTop = 0;
}

function scrollToEnd() {
    log('scroll to end, new top: ' + getEditorElement().offsetHeight);
    getEditorContainerElement().scrollTop = getEditorElement().offsetHeight;
}

function updateScrollBarSizeAndPositions() {
    let vis_sb_elms = getElementsWithVisibleScrollbars();

    if (diff(vis_sb_elms, globals.LastScrollBarElms).length > 0) {
        for (var i = 0; i < vis_sb_elms.length; i++) {
            let elm = vis_sb_elms[i];
            elm.addEventListener('pointerenter', onPointerEnterScrollElm);
            elm.addEventListener('pointerleave', onPointerLeaveScrollElm);
            if (globals.WindowMouseLoc) {
                if (isPointInRect(cleanRect(elm.getBoundingClientRect()), globals.WindowMouseLoc)) {
                    // when scrollbar goes visible and pointer is already within, trigger enter
                    onPointerEnterScrollElm({ currentTarget: elm });
                }
            }
		}        

    }
    globals.LastScrollBarElms = vis_sb_elms;
}


// #endregion Actions

// #region Event Handlers

function onPointerEnterScrollElm(e) {
    let has_x = isHorizontalScrollbarVisible(e.currentTarget);
    let has_y = isVerticalScrollbarVisible(e.currentTarget);
    onScrollBarVisibilityChanged_ntf(has_x, has_y);
}
function onPointerLeaveScrollElm(e) {
    onScrollBarVisibilityChanged_ntf(false,false);
}
function onEditorContainerScroll(e) {
    updateFindReplaceRangeRects();
    drawOverlay();
}
// #endregion Event Handlers