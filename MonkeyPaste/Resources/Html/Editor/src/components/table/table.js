var DefaultCsvProps = {
    ColSeparator: ',',
    RowSeparator: '\n'
};

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
    DefaultCsvProps.RowSeparator = envNewLine();
}

function getTableCsv(format,csvProps, encodeTemplates = false) {

    let table = document.getElementsByClassName('quill-better-table')[0];
    if (!table) {
        return '';
    }
    let rows = table.querySelectorAll('tr');
    if (!rows || rows.length == 0) {
        return '';
    }
    format = !format ? 'HTML Format' : format;
    csvProps = !csvProps ? DefaultCsvProps : csvProps;
    let was_enabled = quill.isEnabled();
    if (!was_enabled) {
        quill.enable(true);
        quill.update();
	}
    let csv_output = '';

    for (var i = 0; i < rows.length; i++) {
        let row = rows[i];
        let cells = row.querySelectorAll('td');
        let row_str = '';
        for (var j = 0; j < cells.length; j++) {
            if (j > 0) {
                row_str += csvProps.ColSeparator;
			}
            let cell = cells[j];
            if (format == 'HTML Format') {
                row_str += cell.innerHTML;
            } else if (format == 'Text') {
                let cell_range = getElementDocRange(cell);
                
                let cell_text = getText(cell_range, encodeTemplates).trim(); // remove new line ending
                row_str += cell_text;
            }
        }
        row_str += csvProps.RowSeparator;
        csv_output += row_str;
    }
    if (!was_enabled) {
        quill.enable(false);
	}
    return csv_output;
}

function initTableToolbarButton() {
    if (!UseBetterTable) {
        return;
    }

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