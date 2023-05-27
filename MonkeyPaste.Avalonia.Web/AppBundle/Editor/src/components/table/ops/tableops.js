
// #region Globals

// #endregion Globals

// #region Life Cycle

function initTableOps() {
    if (!globals.IS_TABLE_OPS_TOOLBAR_ENABLED) {
        return;
    }
    addClickOrKeyClickEventListener(getTableOpsNavLeftButton(), onTableNavLeftClickOrKey);
    addClickOrKeyClickEventListener(getTableOpsNavRightButton(), onTableNavRightClickOrKey);
}
// #endregion Life Cycle

// #region Getters

function getTableOpsToolbarContainerElement() {
    return document.getElementById('tableOpsToolbar');
}

function getTableOpsNavLeftButton() {
    return document.getElementById('tableOpsNavLeft');
}
function getTableOpsNavRightButton() {
    return document.getElementById('tableOpsNavRight');
}
function getTableOpsContainerElement() {
    return document.getElementById('tableOpsOuterContainer').firstChild;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State
function isShowingTableOpsToolbar() {
    return !getTableOpsToolbarContainerElement().classList.contains('hidden');
}
// #endregion State

// #region Actions
function showTableOpsToolbar() {
    const anim_tb = !isShowingTableOpsToolbar();

    let cell_elm = getTableCellElementAtDocIdx(getDocSelection().index);
    if (!cell_elm) {
        return;
    }
    getTableOpsToolbarContainerElement().classList.remove('hidden');

    if (getTableOpsContainerElement()) {
        //getTableOpsContainerElement().remove();
        return;
    }

    let ops_elm = getBetterTableModule().getOpsMenu(cell_elm).tableOperationMenu.domNode;

    getTableOpsToolbarContainerElement().insertBefore(ops_elm, getTableOpsNavRightButton());

    if (anim_tb) {
        let tttb_bottom = 0;
        if (isShowingPasteToolbar()) {
            //ett.classList.remove('bottom-align');
            tttb_bottom += getPasteToolbarContainerElement().getBoundingClientRect().height;
        }
        if (isShowingEditTemplateToolbar()) {
            //ett.classList.remove('bottom-align');
            tttb_bottom += getEditTemplateToolbarContainerElement().getBoundingClientRect().height;
        }

        getTableOpsToolbarContainerElement().style.bottom = `${tttb_bottom}px`;
    }
    
}

function hideTableOpsToolbar() {
    if (!isShowingTableOpsToolbar()) {
        return;
    }
    getTableOpsToolbarContainerElement().style.bottom = `${-getTableOpsToolbarContainerElement().getBoundingClientRect().height}px`;

    delay(getToolbarTransitionMs())
        .then(() => {
            getTableOpsToolbarContainerElement().classList.add('hidden');
            if (getTableOpsContainerElement()) {
                getTableOpsContainerElement().remove();
            }
        });
}

function updateTableOpsToolbarToSelection() {
    if (!globals.IS_TABLE_OPS_TOOLBAR_ENABLED) {
        return;
    }
    const sel = getDocSelection();
    if (!sel ||
        globals.ContentItemType != 'Text' ||
        isReadOnly()) {
        hideTableOpsToolbar();
        return;
    }
    showTableOpsToolbar();
}
// #endregion Actions

// #region Event Handlers

function onTableOpsToolbarWindowClick(e) {
    //updateTableOpsToolbarToSelection();
}

function onTableNavLeftClickOrKey(e) {
    getTableOpsContainerElement().scrollLeft -= 15;
}
function onTableNavRightClickOrKey(e) {
    getTableOpsContainerElement().scrollLeft += 15;
}
// #endregion Event Handlers

