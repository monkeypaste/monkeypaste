var quill;

var clickCount = 0;
var isCompleted = false;
var isLoaded = false;

var consolelog = '';

var enterKeyBindings = null;

var isShowingTemplateContextMenu = false;
var isShowingTemplateColorPaletteMenu = false;
var isShowingTemplateToolbarMenu = false;
var isShowingEditorToolbar = true;


////////////////////////////////////////////////////////////////////////////////
/*  These var's can be parsed and replaced with values from app */
var envName = '';
var isPastingTemplate = false;
var fontFamilys = null;
var fontSizes = null;
var defaultFontIdx = null;
var indentSize = 5;
var htmlContent = '';//"[{\"templateId\":\"-1\",\"templateName\":\"Template #1\",\"templateColor\":\"#402A32\",\"docIdx\":[\"0\",\"3\",\"8\",\"35\"],\"templateText\":\"Template #1\"},{\"templateId\":\"-2\",\"templateName\":\"Template #2\",\"templateColor\":\"#D78484\",\"docIdx\":[\"2\",\"9\",\"32\"],\"templateText\":\"Template #2\"},{\"templateId\":\"-3\",\"templateName\":\"Template #3\",\"templateColor\":\"#165FA4\",\"docIdx\":\"4\",\"templateText\":\"Template #3\"},{\"templateId\":\"-4\",\"templateName\":\"Template #4\",\"templateColor\":\"#912505\",\"docIdx\":\"38\",\"templateText\":\"Template #4\"}]";        
////////////////////////////////////////////////////////////////////////////////

function setWpfEnv() {
    envName = 'wpf';
}

function init(html, isReadOnly, fontFamilys, fontSizes, defaultFontIdx, indentSize, isFillingTemplates) {
    if (fontFamilys == null) {
        fontFamilys = ['Arial', 'Courier', 'Garamond', 'Tahoma', 'Times New Roman', 'Verdana'];
    }
    if (fontSizes == null) {
        fontSizes = ['8px', '9px', '10px', '12px', '14px', '16px', '20px', '24px', '32px', '42px', '54px', '68px', '84px', '98px'];
    }
    if (defaultFontIdx == null) {
        defaultFontIdx = 3;
    }
    loadQuill(fontFamilys, fontSizes, defaultFontIdx);

    if (isFillingTemplates) {
        showPasteTemplateToolbar();
    }

    if (html == null) {
        html = '';
    }

    setHtml(html);


    if (envName == '') {
        //for testing in browser
    } else {
        if (isReadOnly == null || isReadOnly == true) {
            hideToolbar();
            enableReadOnly();
        } else {
            showToolbar();
            disableReadOnly();
        }

        //hideScrollbars();
        //disableScrolling();
    }

    isLoaded = true;

    //console.log('Quill init called');
    return "GREAT!";
}

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

function registerToolbar(fontFamilys, fontSizes) {
    var fonts = registerFonts(fontFamilys);

    var toolbar = {
        container: [
            //[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
            [{ 'size': fontSizes }],               // font sizes
            [{ 'font': fonts.whitelist }],
            ['bold', 'italic', 'underline'/*, 'strike'*/],        // toggled buttons
            //['blockquote', 'code-block'],

            // [{ 'header': 1 }, { 'header': 2 }],               // custom button values
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            // [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
            [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
            //[{ 'direction': 'rtl' }],                         // text direction

            // [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
            ['link'],// 'image', 'video', 'formula'],
            [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
            [{ 'align': [] }],
            // ['clean'],                                         // remove formatting button
            // ['templatebutton'],
            [{ 'Table-Input': registerTables() }]
        ],
        handlers: {
            'Table-Input': () => { return; }
        }
    };

    return toolbar;
}

function registerFonts(fontFamilys, fontSizes) {
    // Specify Quill fonts
    var fontNames = fontFamilys.map(font => getFontName(font));
    var fonts = Quill.import('attributors/class/font');
    fonts.whitelist = fontNames;
    Quill.register(fonts, true);

    //font sizes
    var size = Quill.import('attributors/style/size');
    size.whitelist = fontSizes;
    Quill.register(size, true);

    return fonts;
}

function registerFontStyles(fontFamilys) {
    // Add fonts to CSS style
    var fontStyles = "";
    fontFamilys.forEach(function (font) {
        var fontName = getFontName(font);
        fontStyles += ".ql-snow .ql-picker.ql-font .ql-picker-label[data-value=" + fontName + "]::before, .ql-snow .ql-picker.ql-font .ql-picker-item[data-value=" + fontName + "]::before {" +
            "content: '" + font + "';" +
            "font-family: '" + font + "', sans-serif;" +
            "}" +
            ".ql-font-" + fontName + "{" +
            " font-family: '" + font + "', sans-serif;" +
            "}";
    });

    return fontStyles;
}

function loadQuill(fontFamilys, fontSizes, defaultFontIdx) {
    if (isLoaded) {
        return;
    }

    Quill.register("modules/htmlEditButton", htmlEditButton);
    Quill.register({ 'modules/better-table': quillBetterTable }, true);

    registerTemplateSpan(Quill);

    // Append the CSS stylesheet to the page
    var node = document.createElement('style');
    node.innerHTML = registerFontStyles(fontFamilys);
    document.body.appendChild(node);

    var curQuillDiv = $("#editor");

    quill = new Quill(curQuillDiv[0], {
        placeholder: '',
        theme: 'snow',
        modules: {
            table: false,
            toolbar: registerToolbar(fontFamilys, fontSizes),
            htmlEditButton: {
                syntax: true,
            },
            'better-table': {
                operationMenu: {
                    items: {
                        unmergeCells: {
                            text: 'Unmerge cells'
                        }
                    },
                    color: {
                        colors: ['green', 'red', 'yellow', 'blue', 'white'],
                        text: 'Background Colors:'
                    }
                }
            },
            keyboard: {
                bindings: quillBetterTable.keyboardBindings
            }
        }
    });

    var curTableIconSpan = curQuillDiv.parent().find('span.ql-Table-Input.ql-picker')[0].childNodes[0];
    curTableIconSpan.innerHTML = "<svg style=\"right: 4px;\" viewbox=\"0 0 18 18\"> <rect class=ql-stroke height=12 width=12 x=3 y=3></rect> <rect class=ql-fill height=2 width=3 x=5 y=5></rect> <rect class=ql-fill height=2 width=4 x=9 y=5></rect> <g class=\"ql-fill ql-transparent\"> <rect height=2 width=3 x=5 y=8></rect> <rect height=2 width=4 x=9 y=8></rect> <rect height=2 width=3 x=5 y=11></rect> <rect height=2 width=4 x=9 y=11></rect> </g> </svg>";
    var curTableCellIconSpans = $(curTableIconSpan.parentNode.childNodes[1]).children();
    curTableCellIconSpans.click((function () {
        var curQuillBetterTable = quill.getModule('better-table');
        var curQuillToolbar = quill.getModule('toolbar');
        return function () {
            var curRowIndex = Number(this.dataset.value.substring(9).split('_')[0]);
            var curColIndex = Number(this.dataset.value.substring(9).split('_')[1]);
            curQuillBetterTable.insertTable(curRowIndex, curColIndex);
            // The following two lines have been added, thinking that it would fix the issue of keeping the icon in blue color.  However Quill keeps adding the classes back, so this fix doesn't work.
            $(this).parent().parent().find(".ql-selected").removeClass("ql-selected"); $(this).parent().parent().find(".ql-active").removeClass("ql-active");
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

    // Toolbar Template Button
    const templateToolbarButton = new QuillToolbarButton({
        icon: '<img id="templateToolbarButton" style="height:20px" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAaVBMVEX/////wQf/vwD/24f/3Iz///3/vQD/ykL/yTj/yT3/0WH/0F7/0Fv/y0T/y0f/yTr/35T/wxT/8tP/6Lj/5az/7sj/9uX/46P/1G7/677/57P/2oL/xSj/8M//1nj//fb//O//4Zz/zVBsBjAmAAAEzklEQVR4nO2di3qiMBBGIUpa1168tFZ70933f8hl0KqVWCuZyfzhm/MAfJ7vhAmwCy0KwzAMwzAMwzAMwzAMwzAMWKq3QSzDyXymrXGe6ta7eLwvl8/aKmGqW1fy4Pz0SdsmAJ9g4zhaaAudwirYOIJl5Bas8QNtqWMEBLEURQRrRZiFKiRYK4KMGzHB0j1ouzXICdYR59p2haxg6d619YQF64gfPRcs3ae24KOsYOmmyoLCBUvtDSOBYOk176TEl2hjqHhdw1WwvuetOXcsN9ET5Cno/PJ1tvh4Gp9xdEM1QZ6C/m29O+Dmj4cyZCroV0fHfAkpahlyFfw+KEOKSoZcBU9//bh9WB1Dtn3w9MCbdkQVQ7Ztov3j71pH1jBk2+h9++n2UyuigiHfpZrftA7+AWDIeKnm162jL/QNOS+2Aw1n6oasF9uB83Cubch7uxS4qF4qz9JqxHq71L5/Xyvvh+z3g/40YjthUkOBG96TM7F9FiY1ZF6i25/vjhVXuvcWUv/4sl+o60/d+0OJglvF6XC2WS+eP53uPb7gUzV34TlNGkOxgpdJY5jiuaiqoWLBNIZJHvxqGuoKJjBUXaIpDDWHTBJD7YLihuoFxQ3VCwob6i9RacMbAEFJwwpCUNIQQ1DOEKSgoCHCkGkQMoQpKGUIsU3skDHEKShjiFRQxhCpoIQh0JBp4DcEE2Q3RCvIbwg1ZBp4DfEKMhtibRM7WA3vAQU5DRGXaMlqCFmQ0RC0IKMhqiCXYQW6REs2Q9iCXIa4BZkMgQvyGCIX5DCE3SZ2xBtiF4w3RC8YbwgvGGkIvNHviTPELxhpmEHBOMMcCkYZPmQh2H77q2cFS/dQ9bugu+laMIshE/Ntk1wEOxccB//POBzdC75lInjfVfC574LFNIuTMOIDSn+zSNh9yBRV3wsWrzkkjCgYeh8cj6iPmOWwSKMKBl4lhsPdd73YbsA/DeMKFsUEfZW6m6iCRTEAN4y4ksnDMLog+iqNLxh+XRqG2CHTgLxbcBSE3vFZChbBTzFhELnRH0A9EbkK1mirhGHYJvYEv/umDc+Q+eId70zkLFggbhi8BWsmYIqMQ+aLJZQie0EC6aG3GwkIFkX4W6EaMA8ZPEWBc/ALjIUqVpBAqChYkLhTV3QjwYKEdkXhgoRuRfGChGZF0SFzQG+iCm30bbQqJiqop5hgyBzQWKgJCxLpKyYtSKTeNJJsE99JWzF5QSJlRYWCRLqKiYfMgVQT1d0qCaaqqLRE0ym6R0XBFAtVWVC+ouoS3SK7aSgOmQOSFQEKEnIVIQoSUhVBChIyExWmICFREaggwa+ovg+ewr1Q4QS5K4It0S2cmwbUkDnAVxGyIMFVEbQgwVMRcMgc4JiowAWJ+IrQBYlYRfCCRNxCzUAwriL8Et3SfdPIoiDRtWImBYluFbMpSHSpmFFB4vqJmlVB4tqKmRUkrlPMriBxzULNUvCaihku0S2/3TQyLUj8rmK2BYnfVMy4IHG5YuaClydq9oKXKvZA8GfFXgj+tFB7Inj+hSLf+btqcEyCf7nXL7V/FyOz91ZGX660fxUvL+VxR+fdpDcrdM9q7Lx3jv7gtPv32j8/oprNJ8PB5HXWTz3DMAzDMAzDMAzDMAzDMHrEf9VjaGNa1FYRAAAAAElFTkSuQmCC"></img>'//`<svg viewBox="0 0 18 18"> <path class="ql-stroke" d="M5,3V9a4.012,4.012,0,0,0,4,4H9a4.012,4.012,0,0,0,4-4V3"></path></svg>`
    })

    templateToolbarButton.onClick = function (e) {
        var templateButton = document.getElementById('templateToolbarButton');
        var tl = getTemplates();
        if (tl.length > 0) {
            showTemplateToolbarContextMenu(templateButton);
        } else {
            createTemplate();
        }
    }
    templateToolbarButton.attach(quill)

    document.addEventListener('click', function (e) {
        //console.log(e.target);
        //if(getFocusTemplateElement() == null) {
        //    hideTemplateToolbarContextMenu();
        //    hideTemplateContextMenu();
        //}
        if (clickCount > 0) {
            if (isShowingTemplateToolbarMenu) {
                hideTemplateToolbarContextMenu();
            }
            if (isShowingTemplateContextMenu) {
                hideTemplateContextMenu();
            }
            if (isShowingTemplateColorPaletteMenu) {
                hideTemplateColorPaletteMenu();
            }
        } else {
            clickCount++;
            if (!wasLastClickOnTemplate) {
                clearTemplateSelection();
                //hidePasteTemplateToolbar();
            } //else if (!isShowingPasteTemplateToolbar) {
                //showPasteTemplateToolbar();
            //}
            wasLastClickOnTemplate = false;
        }
        return;
    });

    const Parchment = Quill.import('parchment');

    let lastClickEvent = null;

    this.quill.root.addEventListener('click', (e) => {
        lastClickEvent = e;

        //let image = Parchment.find(ev.target);

        //if(image instanceof ImageBlot) {
        //    this.quill.setSelection(image.offset(this.quill.scroll), 1, 'user');
        //}

        if (isRenamingTemplate()) {
            if (e.target.getAttribute('templateId') == null ||
                e.target.getAttribute('contenteditable') == false) {
                endSetTemplateName();
            }

            //let tl = getTemplatesFromRange(range);
            //let isRenaming = false;
            //tl.forEach(function (t) {
            //    let tid = t.getAttribute('templateId');
            //    let iid = t.getAttribute('instanceId');
            //    let te = getTemplateElement(tid, iid);
            //    let isEditable = te.getAttribute('contenteditable');
            //    if(isEditable != null || isEditable == false) {
            //        isRenaming = true;
            //        return;
            //    }
            //})
            //if(tl == null || !isRenaming) {
            //    endSetTemplateName();
            //}
            //clearTemplateFocus();
            //quill.setSelection(oldRange);
        }
    });

    quill.on('selection-change', function (range, oldRange, source) {
        //if(isRenamingTemplate()) {

        //    // Given DOM node, find corresponding Blot.
        //    // Bubbling is useful when searching for a Embed Blot with its corresponding
        //    // DOM node's descendant nodes.
        //    let s = Parchment.find(domNode: Node, bubble: boolean = false): Blot;

        //    find(lastClickEvent.target);

        //    if(s instanceof TemplateSpanBlot) {
        //        // TODO make sure s has contenteditable or its a different template
        //        return;
        //    } else {
        //        endSetTemplateName();
        //    }

        //    //clearTemplateFocus();
        //    //quill.setSelection(oldRange);
        //}      


        if (range) {
            if (range.length == 0) {
                console.log('User cursor is on', range.index);               

            } else {
                var text = quill.getText(range.index, range.length);
                console.log('User has highlighted', text);
            }

            let isTemplateSelected = false;
            var tl = getTemplates();
            tl.forEach(function (t) {
                if (Array.isArray(t.docIdx)) {
                    t.docIdx.forEach(function (tDocIdx) {
                        if (range.length == 0) {
                            if (tDocIdx == range.index) {
                                isTemplateSelected = true;
                                return;
                            }
                        } //else if (parseInt(tDocIdx) >= range.index && parseInt(tDocIdx) <= range.index + range.length) {
                        //    isTemplateSelected = true;
                        //    return;
                        //}
                    });
                } else {
                    if (range.length == 0) {
                        if (parseInt(t.docIdx) == range.index) {
                            isTemplateSelected = true;
                            return;
                        }
                    }// else if (parseInt(t.docIdx) >= range.index && parseInt(t.docIdx) <= range.index + range.length) {
                    //    isTemplateSelected = true;
                    //    return;
                    //}
                }
            });
            
            if (!isTemplateSelected) {
                selectedTemplateId = 0;
                hideEditTemplateToolbar();
            }
        } else {
            console.log('Cursor not in the editor');
        }
    });

    quill.on('text-change', function (delta, oldDelta, source) {
        var retainVal = 0;
        var textDelta = 0;
        var wasAddTemplate = false;
        delta.ops.forEach(function (op) {
            if (op.insert != null && op.insert.templatespan != null) {
                //handle shifting in create template
                wasAddTemplate = true;
                return;
            }
            if (op.retain != null) {
                retainVal = op.retain;
            }
            if (op.insert != null && op.insert.templatespan == null) {
                textDelta += op.insert.length;
            }
            if (op.delete != null) {
                textDelta -= parseInt(op.delete);
            }
        });
        if (!wasAddTemplate && textDelta != 0 && retainVal >= 0) {
            shiftTemplates(retainVal, textDelta);
            //console.log(getTemplates());
        }
    });
}

function getFontName(font) {
    // Generate code-friendly font names
    return font.toLowerCase().replace(/\s/g, "-");
}

function setText(text) {
    quill.setText(text + '\n');
}

function setHtml(html) {
    //var delta = quill.clipboard.convert(html);
    //quill.setContents(delta, 'silent');
    quill.root.innerHTML = html;
}

function getContentsAsJson() {
    return JSON.stringify(quill.getContents());
}

function setContents(jsonStr) {
    quill.setContents(JSON.parse(jsonStr));
}

function getText() {
    var text = quill.getText(0, quill.getLength() - 1);
    return text;
}

function getSelectedText() {
    var selection = quill.getSelection();
    return quill.getText(selection.index, selection.length);
}

function getHtml() {
    //document.getElementsByClassName
    //var val = document.getElementsByClassName("ql-editor")[0].innerHTML;
    clearTemplateSelection();
    clearTemplateFocus();
    var val = quill.root.innerHTML;
    return unescape(val);
}

function getSelectedHtml() {
    var selection = quill.getSelection();
    var selectedContent = quill.getContents(selection.index, selection.length);
    var tempContainer = document.createElement('div')
    var tempQuill = new Quill(tempContainer);
    tempQuill.setContents(selectedContent);
    let result = tempContainer.querySelector('.ql-editor').innerHTML;
    tempContainer.remove();
    return result;
}

function isEditorLoaded() {
    return isLoaded;
}

function hideToolbar() {
    isShowingEditorToolbar = false;
    $(".ql-toolbar").css("display", "none");
    moveEditorTop(0);
}

function showToolbar() {
    isShowingEditorToolbar = true;
    $(".ql-toolbar").css("display", "inline-block");
    let tbh = $(".ql-toolbar").outerHeight();
    moveEditorTop(tbh);
}

function getEditorWidth() {
    var editorRect = document.getElementById('editor').getBoundingClientRect();
    //var editorHeight = parseInt($('.ql-editor').wi());
    return editorRect.width;
}

function getEditorHeight() {
    var editorRect = document.getElementById('editor').getBoundingClientRect();
    //var editorHeight = parseInt($('.ql-editor').outerHeight());
    return editorRect.height;
}

function getContentWidth() {
    var bounds = quill.getBounds(0, quill.getLength());
    return bounds.width;
}

function getContentHeight() {
    var bounds = quill.getBounds(0, quill.getLength());
    return bounds.height;
}

function getToolbarHeight() {
    var toolbarHeight = parseInt($('.ql-toolbar').outerHeight());
    return toolbarHeight;
}

function getTemplateToolbarHeight() {
    if (!isShowingEditTemplateToolbar && !isShowingPasteTemplateToolbar) {
        return 0;
    }
    if (isShowingEditTemplateToolbar) {
        return parseInt($("#editTemplateToolbar").outerHeight());
    } else if (isShowingPasteTemplateToolbar) {
        return parseInt($("#pasteTemplateToolbar").outerHeight());
    }
    return 0;    
}

function getTotalHeight() {
    var totalHeight = getToolbarHeight() + getEditorHeight() + getTemplateToolbarHeight();
    return totalHeight;
}

function isReadOnly() {
    var isEditable = $('.ql-editor').attr('contenteditable');
    return !isEditable;
}

function enableReadOnly() {
    $('.ql-editor').attr('contenteditable', false);
    $('.ql-editor').css('caret-color', 'transparent');

    hideToolbar();

    //hideScrollbars();
    disableScrolling();
}

function disableReadOnly() {
    $('.ql-editor').attr('contenteditable', true);
    $('.ql-editor').css('caret-color', 'black');

    document.getElementById('editor').style.height = getContentHeight();

    showToolbar();

    showScrollbars();
}

function hideScrollbars() {
    //hides scrollbars without disabling scrolling (for use when scrolling to search match)
    document.querySelector('body').style.overflow = 'hidden';
    //let isNew = false;
    //var style = document.getElementsByName('style');
    //if (style == null) {
    //    isNew = true;
    //    style = document.createElement('style');
    //}

    //style.type = 'text/css';
    //style.innerHTML = '::-webkit-scrollbar{display:none}';
    //if (isNew) {
    //    document.getElementsByTagName('body')[0].appendChild(style);
    //}
}

function showScrollbars() {
    //hides scrollbars without disabling scrolling (for use when scrolling to search match)
    
    document.querySelector('body').style.overflow = 'auto';

    //if (window.innerHeight < )
    //let isNew = false;
    //var style = document.getElementsByName('style');
    //if (style == null) {
    //    isNew = true;
    //    style = document.createElement('style');
    //}

    //style.type = 'text/css';
    //var overflowStyle = 'auto';
    //if (window)
    //style.innerHTML = '::-webkit-scrollbar{display: block; width: 10em; overflow: auto; height: 2em;}';
    //if (isNew) {
    //    document.getElementsByTagName('body')[0].appendChild(style);
    //}
}

function disableScrolling() {
    document.querySelector('body').style.overflow = 'hidden';
}

function enableScrolling() {
    document.querySelector('body').style.overflow = 'scroll';
}

function getTextWithEmbedTokens() {
    var text = getText().split('');
    var otl = getTemplatesByDocOrder()
    var outText = '';
    var offset = 0;
    otl.forEach(function (ot) {
        offset += parseInt(ot.docIdx);
        var embedStr = getTemplateEmbedStr(ot);
        //text.splice(offset, 1);
        for (var i = 0; i < embedStr.length; i++) {
            text.splice(offset + i, 0, embedStr[i]);
        }
        offset += (embedStr.length);
    });
    return text.join('');
}

function getTemplatesByDocOrder() {
    var til = getTemplateInstances();
    til.sort((a, b) => (parseInt(a.docIdx) > parseInt(b.docIdx)) ? 1 : -1);
    return til;
}

function getUniqueTemplateInstanceId(tId) {
    let newInstanceId = 1;
    let isDup = true;
    while (isDup) {
        isDup = false;
        getTemplateInstances(tId).forEach(function (ti) {
            if (ti.instanceId == newInstanceId) {
                isDup = true;
            }
        });
        if (!isDup) {
            return newInstanceId;
        }
        newInstanceId++;
    }
    return newInstanceId;
}

function getTemplateInstances(tId, iId) {
    var til = [];
    getTemplates().forEach(function (t) {
        if (tId != null) {
            if (t.templateId != tId) {
                return;
            }
            if (iId != null && t.instanceId != iId) {
                return;
            }
        }

        if (Array.isArray(t.docIdx)) {
            t.docIdx.forEach(function (tDocIdx) {
                var ti = new Object();
                var ti = Object.assign(ti, t);
                ti.docIdx = tDocIdx;
                til.push(ti);
            });
        } else {
            til.push(t);
        }
    });
    if (tId != null && iId != null && til.length == 1) {
        return til[0];
    }
    return til;
}

function getTemplateOffset(_t, idx) {
    var _tDocIdx = Array.isArray(_t.docIdx) ? _t.docIdx[idx] : _t.docIdx;
    var text = getText();
    var tl = getTemplates();
    var offset = 0;
    tl.forEach(function (t) {
        var embedStr = '{{' + t.templateId + '}}';
        if (Array.isArray(t.docIdx)) {
            t.docIdx.forEach(function (tDocIdx) {
                if (tDocIdx < _tDocIdx) {
                    offset += getTemplateEmbedStr(t).length;
                }
            });
        } else {
            if (t.docIdx < _tDocIdx) {
                offset += getTemplateEmbedStr(t).length;
            }
        }
    });
    return offset;
}

function getTemplateEmbedStr(t) {
    var templateStr = '{{' + t.templateId + '}}';
    return templateStr;
}

function getTemplatesJson() {
    var val = JSON.stringify(getTemplates());
    return val;
}

function decodeHtml(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function isBright(hex, brightThreshold = 150) {
    var c = hexToRgb(hex.toLowerCase());
    var grayVal = Math.sqrt(
        c.R * c.R * .299 +
        c.G * c.G * .587 +
        c.B * c.B * .114);
    return grayVal > brightThreshold;
}

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        R: parseInt(result[1], 16),
        G: parseInt(result[2], 16),
        B: parseInt(result[3], 16)
    } : null;
}

function createLink() {
    var range = quill.getSelection(true);
    if (range) {
        var text = quill.getText(range.index, range.length);
        quill.deleteText(range.index, range.length);
        var ts = '<a class="square_btn" href="https://www.google.com">' + text + '</a>';
        quill.clipboard.dangerouslyPasteHTML(range.index, ts);

        console.log('text:\n' + getText());
        console.table('\nhtml:\n' + getHtml());
    }
}

function moveToolbarTop(y) {
    $(".ql-toolbar").css("position", "absolute");
    $(".ql-toolbar").css("top", y);

    //var viewportBottom = window.scrollY + window.innerHeight;
    //let tbh = $(".ql-toolbar").outerHeight();
    //if (y <= 0) {
    //    //keyboard is not visible
    //    $(".ql-toolbar").css("top", y);
    //    $("#editor").css("top", y + tbh);
    //} else {
    //    $(".ql-toolbar").css("top", y - tbh);
    //    $("#editor").css("top", 0);
    //}
    //$("#editor").css("bottom", viewportBottom - tbh);
}

function movePasteTemplateToolbarTop(y) {
    $("#pasteTemplateToolbar").css("position", "absolute");
    $("#pasteTemplateToolbar").css("top", y);
}

function moveEditorTop(y) {
    $("#editor").css("position", "absolute");
    $("#editor").css("top", y);
}

function getTemplateIconStr(isEnabled) {
    if (isEnabled) {
        //black
        return 'data:image/png;base64,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';
        //yellow
        //return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAaVBMVEX/////wQf/vwD/24f/3Iz///3/vQD/ykL/yTj/yT3/0WH/0F7/0Fv/y0T/y0f/yTr/35T/wxT/8tP/6Lj/5az/7sj/9uX/46P/1G7/677/57P/2oL/xSj/8M//1nj//fb//O//4Zz/zVBsBjAmAAAEzklEQVR4nO2di3qiMBBGIUpa1168tFZ70933f8hl0KqVWCuZyfzhm/MAfJ7vhAmwCy0KwzAMwzAMwzAMwzAMwzAMWKq3QSzDyXymrXGe6ta7eLwvl8/aKmGqW1fy4Pz0SdsmAJ9g4zhaaAudwirYOIJl5Bas8QNtqWMEBLEURQRrRZiFKiRYK4KMGzHB0j1ouzXICdYR59p2haxg6d619YQF64gfPRcs3ae24KOsYOmmyoLCBUvtDSOBYOk176TEl2hjqHhdw1WwvuetOXcsN9ET5Cno/PJ1tvh4Gp9xdEM1QZ6C/m29O+Dmj4cyZCroV0fHfAkpahlyFfw+KEOKSoZcBU9//bh9WB1Dtn3w9MCbdkQVQ7Ztov3j71pH1jBk2+h9++n2UyuigiHfpZrftA7+AWDIeKnm162jL/QNOS+2Aw1n6oasF9uB83Cubch7uxS4qF4qz9JqxHq71L5/Xyvvh+z3g/40YjthUkOBG96TM7F9FiY1ZF6i25/vjhVXuvcWUv/4sl+o60/d+0OJglvF6XC2WS+eP53uPb7gUzV34TlNGkOxgpdJY5jiuaiqoWLBNIZJHvxqGuoKJjBUXaIpDDWHTBJD7YLihuoFxQ3VCwob6i9RacMbAEFJwwpCUNIQQ1DOEKSgoCHCkGkQMoQpKGUIsU3skDHEKShjiFRQxhCpoIQh0JBp4DcEE2Q3RCvIbwg1ZBp4DfEKMhtibRM7WA3vAQU5DRGXaMlqCFmQ0RC0IKMhqiCXYQW6REs2Q9iCXIa4BZkMgQvyGCIX5DCE3SZ2xBtiF4w3RC8YbwgvGGkIvNHviTPELxhpmEHBOMMcCkYZPmQh2H77q2cFS/dQ9bugu+laMIshE/Ntk1wEOxccB//POBzdC75lInjfVfC574LFNIuTMOIDSn+zSNh9yBRV3wsWrzkkjCgYeh8cj6iPmOWwSKMKBl4lhsPdd73YbsA/DeMKFsUEfZW6m6iCRTEAN4y4ksnDMLog+iqNLxh+XRqG2CHTgLxbcBSE3vFZChbBTzFhELnRH0A9EbkK1mirhGHYJvYEv/umDc+Q+eId70zkLFggbhi8BWsmYIqMQ+aLJZQie0EC6aG3GwkIFkX4W6EaMA8ZPEWBc/ALjIUqVpBAqChYkLhTV3QjwYKEdkXhgoRuRfGChGZF0SFzQG+iCm30bbQqJiqop5hgyBzQWKgJCxLpKyYtSKTeNJJsE99JWzF5QSJlRYWCRLqKiYfMgVQT1d0qCaaqqLRE0ym6R0XBFAtVWVC+ouoS3SK7aSgOmQOSFQEKEnIVIQoSUhVBChIyExWmICFREaggwa+ovg+ewr1Q4QS5K4It0S2cmwbUkDnAVxGyIMFVEbQgwVMRcMgc4JiowAWJ+IrQBYlYRfCCRNxCzUAwriL8Et3SfdPIoiDRtWImBYluFbMpSHSpmFFB4vqJmlVB4tqKmRUkrlPMriBxzULNUvCaihku0S2/3TQyLUj8rmK2BYnfVMy4IHG5YuaClydq9oKXKvZA8GfFXgj+tFB7Inj+hSLf+btqcEyCf7nXL7V/FyOz91ZGX660fxUvL+VxR+fdpDcrdM9q7Lx3jv7gtPv32j8/oprNJ8PB5HXWTz3DMAzDMAzDMAzDMAzDMHrEf9VjaGNa1FYRAAAAAElFTkSuQmCC";
    } else {
        //gray
        return 'data:image/png;base64,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';
    }
}

