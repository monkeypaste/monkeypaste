var IsTextSelectionFancy = false;
var FancyTextSelectionRoundedCornerRadius = 5;

function convertRectsToRoundRectGroups(rects,max_snap_dist, def_corner_radius) {
    //assumes rects are sorted by y and don't overlap
    let cur_rrect_group = [];
    let rrect_groups = [];
	for (var i = 0; i < rects.length; i++) {
        let cur_rect = rects[i];
        if (i == 0) {
            cur_rrect_group = [[def_corner_radius, cur_rect]];
        } else {
            let last_rect = cur_rrect_group[cur_rrect_group.length - 1][1];
            let is_new_group = Math.abs(cur_rect.top - last_rect.bottom) > max_snap_dist;
            if (is_new_group) {
                rrect_groups.push(cur_rrect_group);
                cur_rrect_group = [[def_corner_radius, cur_rect]];
            } else {
                let cur_radius = def_corner_radius;
                // set radius from tl of prev cw going from br to tr of cur
                let last_rect_idx = cur_rrect_group.length - 1;
                if (false) { //if (last_rect_idx > 0) {
                    // prev was interior group


                } else {
                    // prev was top of group

                    // only has unique br and bl edges

                    // handle pbr and ctr
                    let prev_br_to_cur_tr_dist = last_rect.right - cur_rect.right;
                    let prev_br_to_cur_tr_diff = Math.abs(prev_br_to_cur_tr_dist);
                    if (prev_br_to_cur_tr_diff <= max_snap_dist) {
                        // on snap no radius pbr ctr

                        //cur_rrect_group[last_rect_idx][0].br = 0;
                        //cur_radius.tr = 0;
                    } else {
                        // if staggered check if prev and cur rects meet along x-axis
                        let do_cur_prev_intersect = cur_rect.right > last_rect.left;
                        if (do_cur_prev_intersect) {
                            if (prev_br_to_cur_tr_dist < 0) {
                                // pbr is before ctr
                                // add cap to pbr
                                cur_rrect_group[last_rect_idx][0].br = -def_corner_radius;
                            } else {
                                // pbr is after ctr
                                cur_radius.tr = -def_corner_radius;
                            }
						}
                    }

                    // handle pbl and ctl


                    //let prev_bl_to_cur_tl_dist = last_rect.left - cur_rect.left;
                    //let prev_bl_to_cur_tl_diff = Math.abs(prev_bl_to_cur_tl_dist);
                    //if (prev_bl_to_cur_tl_diff <= max_snap_dist) {
                    //    // on snap no radius pbl ctl
                    //    cur_rrect_group[last_rect_idx][0].bl = 0;
                    //    cur_radius.tl = 0;
                    //} else {
                    //    // if staggered check if prev and cur rects meet along x-axis
                    //    let do_cur_prev_intersect = cur_rect.right > last_rect.left;
                    //    if (do_cur_prev_intersect) {
                    //        if (prev_bl_to_cur_tl_dist < 0) {
                    //            // pbl is before ctl
                    //            // add cap to pbl
                    //            cur_rrect_group[last_rect_idx][0].br = -def_corner_radius;
                    //        } else {
                    //            // pbl is after ctl
                    //            cur_radius.tl = -def_corner_radius;
                    //        }
                    //    }
                    //}
                }

                cur_rrect_group.push([cur_radius, cur_rect]);
			}
		}
    }
    if (cur_rrect_group != null && cur_rrect_group.length > 0) {
        rrect_groups.push(cur_rrect_group);
	}
    return rrect_groups;
}



function enableFancyTextSelection() {
    IsTextSelectionFancy = true;
    setTextSelectionBgColor('transparent');
    drawOverlay();
}

function disableFancyTextSelection() {
    IsTextSelectionFancy = false;
    setTextSelectionBgColor('black');
    drawOverlay();
}

function toggleFancyTextSelection() {
    IsTextSelectionFancy ? disableFancyTextSelection() : enableFancyTextSelection();
}

function setTextSelectionBgColor(bgColor) {
    document.body.style.setProperty('--selbgcolor', bgColor);
}

function setTextSelectionFgColor(fgColor) {
    document.body.style.setProperty('--selfgcolor', fgColor);
}

function setCaretColor(caretColor) {
    getEditorElement().style.caretColor = caretColor;
}

function getCaretColor() {
    return getEditorElement().style.caretColor;
}