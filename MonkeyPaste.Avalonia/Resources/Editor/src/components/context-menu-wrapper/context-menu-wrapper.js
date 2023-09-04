// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getBusySpinnerMenuItem() {
    return [
        {
            icon: 'spinner',
            iconFgColor: 'dimgray',
            iconClassList: ['rotate'],
            label: 'Loading...'
        }
    ];
}

function getContextMenuElement() {
    let cm_elms = Array.from(document.getElementsByClassName('context-menu'));
    if (cm_elms.length == 0) {
        return null;
    }
    return cm_elms[0];
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions
function moveContextMenu(origin) {
    let cm_elm = getContextMenuElement();
    if (!cm_elm) {
        return;
    }

    setElementComputedStyleProp(cm_elm, 'left', `${origin.x}px`);
    setElementComputedStyleProp(cm_elm, 'top', `${origin.y}px`);
}
function calculateMenuOrigin(anchor_rect, menu_rect, anchor_corner, menu_anchor_corner, offset_x,offset_y) {
    let anchor_vert = anchor_corner.split('-')[0];
    let anchor_horiz = anchor_corner.split('-')[1];
    let menu_vert = menu_anchor_corner.split('-')[0];
    let menu_horiz = menu_anchor_corner.split('-')[1];

    let anchor_p = getRectCornerByName(anchor_rect, anchor_corner);
    let origin_x = 0;
    let origin_y = 0;

    if (anchor_horiz != menu_horiz) {
        if (anchor_horiz == 'left') {
            // anchor left menu is right
            origin_x = anchor_rect.left - menu_rect.width;
        } else {
            // anchor right menu left
            origin_x = anchor_rect.right;
        }
    } else {
        if (anchor_horiz == 'left') {
            // anchor left menu is left
            origin_x = anchor_rect.left;
        } else {
            // anchor right menu right
            origin_x = anchor_rect.right - menu_rect.width;
        }
    }
    if (anchor_vert != menu_vert) {
        if (anchor_vert == 'top') {
            // anchor top menu is bottom
            origin_y = anchor_rect.top - menu_rect.height;
        } else {
            // anchor bottom menu is top
            origin_y = anchor_rect.bottom;
        }
    } else {
        if (anchor_vert == 'top') {
            // anchor top menu is top
            origin_y = anchor_rect.top;
        } else {
            // anchor bottom menu bottom
            origin_y = anchor_rect.bottom - menu_rect.height;
        }
    }
    origin_x += offset_x ? offset_x : 0;
    origin_y += offset_y ? offset_y : 0;
    return { x: origin_x, y: origin_y };
}

function createContextMenu(mil, origin) {
    let x = origin ? origin.x : 0;
    let y = origin ? origin.y : 0;
    superCm.destroyMenu();
    superCm.createMenu(mil, { pageX: x, pageY: y });
}
function showContextMenu(anchor_elm, mil, anchor_corner, menu_anchor_corner, offset_x, offset_y) {
    anchor_corner = anchor_corner || 'top-right';
    menu_anchor_corner = menu_anchor_corner || 'top-left';
    let anchor_elm_rect = cleanRect(anchor_elm.getBoundingClientRect());
    createContextMenu(mil, getRectCornerByName(anchor_elm_rect,anchor_corner));

    let cm_elm = getContextMenuElement();
    let cm_rect = cleanRect(cm_elm.getBoundingClientRect());
    let cm_origin = calculateMenuOrigin(anchor_elm_rect, cm_rect, anchor_corner, menu_anchor_corner, offset_x, offset_y);
    moveContextMenu(cm_origin);
    return cm_elm;
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers