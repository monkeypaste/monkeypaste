function registerTables() {
    var tableOptions = [];
    var maxRows = 7;
    var maxCols = 7;

    for (let r = 1; r <= maxRows; r++) {
        for (let c = 1; c <= maxCols; c++) {
            tableOptions.push('newtable_' + r + '_' + c);
        }
    }
    return tableOptions;
}

function initTable() {
    initTableToolbarButton();
}


function initTableToolbarButton() {
    let editorDiv = $("#editor");

    var curTableIconSpan = editorDiv.parent().find('span.ql-Table-Input.ql-picker')[0].childNodes[0];
    curTableIconSpan.innerHTML = "<svg style=\"right: 4px;\" viewbox=\"0 0 18 18\"> <rect class=ql-stroke height=12 width=12 x=3 y=3></rect> <rect class=ql-fill height=2 width=3 x=5 y=5></rect> <rect class=ql-fill height=2 width=4 x=9 y=5></rect> <g class=\"ql-fill ql-transparent\"> <rect height=2 width=3 x=5 y=8></rect> <rect height=2 width=4 x=9 y=8></rect> <rect height=2 width=3 x=5 y=11></rect> <rect height=2 width=4 x=9 y=11></rect> </g> </svg>";
    var curTableCellIconSpans = $(curTableIconSpan.parentNode.childNodes[1]).children();
    curTableCellIconSpans.click((function () {
        var curQuillBetterTable = quill.getModule('better-table');
        var curQuillToolbar = quill.getModule('toolbar');
        return function () {
            var curRowIndex = Number(this.dataset.value.substring(9).split('_')[0]);
            var curColIndex = Number(this.dataset.value.substring(9).split('_')[1]);
            curQuillBetterTable.insertTable(curRowIndex, curColIndex);
            // The following two lines have been added, thinking that it would fix the issue 
            // of keeping the icon in blue color.
            // However Quill keeps adding the classes back, so this fix doesn't work.
            $(this).parent().parent().find(".ql-selected").removeClass("ql-selected");
            $(this).parent().parent().find(".ql-active").removeClass("ql-active");
        };
    })());
    curTableCellIconSpans.hover(function () {
        var curRowIndex = Number(this.dataset.value.substring(9).split('_')[0]);
        var curColIndex = Number(this.dataset.value.substring(9).split('_')[1]);
        $(this).parent().children().each((function () {
            var curRowIndex1 = curRowIndex;
            var curColIndex1 = curColIndex;
            return function () {
                var curRowIndex2 = Number(this.dataset.value.substring(9).split('_')[0]);
                var curColIndex2 = Number(this.dataset.value.substring(9).split('_')[1]);
                if (curRowIndex2 <= curRowIndex1 && curColIndex2 <= curColIndex1) {
                    $(this).addClass("ql-picker-item-highlight");
                }
            };
        })());
    }, function () {
        $(this).parent().children().removeClass("ql-picker-item-highlight");
    });
}