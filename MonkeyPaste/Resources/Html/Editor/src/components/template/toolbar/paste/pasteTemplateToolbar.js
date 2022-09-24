function showPasteTemplateToolbar() {
    var ptt = document.getElementById('pasteTemplateToolbar');
    ptt.style.display = 'flex';

    document.getElementById('paste-template-custom-select').innerHTML = '<select id="pasteTemplateToolbarMenuSelector"></select >';

    var templateMenuSelector = document.getElementById('pasteTemplateToolbarMenuSelector');
    templateMenuSelector.innerHTML = '';

    var tl = getUsedTemplateDefinitions();
    for (var i = 0; i < tl.length; i++) {
        var t = tl[i];
        var templateItem = '<option class="templateOption" value="' + t['templateGuid'] + '" onchange="focusTemplate(' + t['templateGuid'] + ');">' +
            t['templateName'] + '</option>';
        templateMenuSelector.innerHTML += templateItem;
    }

    document.getElementById('nextTemplateButton').addEventListener('click', function (e) {
        gotoNextTemplate();
    });
    document.getElementById('nextTemplateButton').addEventListener('keydown', function (e) {
        gotoNextTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('click', function (e) {
        gotoPrevTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('keydown', function (e) {
        gotoPrevTemplate();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('click', function (e) {
        clearAllTemplateText();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('keydown', function (e) {
        clearAllTemplateText();
    });
    document.getElementById('pasteTemplateButton').addEventListener('click', function (e) {
        isCompleted = true;
    });
    document.getElementById('pasteTemplateButton').addEventListener('keydown', function (e) {
        isCompleted = true;
    });

    createTemplateSelectorStyling(templateMenuSelector);

    //let tbb = isShowingToolbar ? $(".ql-toolbar").outerHeight() : 0;
    //moveToolbarTop(0);
    //let wb = $(window).height();
    //movePasteTemplateToolbarTop($(window).height() - $("#pasteTemplateToolbar").outerHeight());
    //moveEditorTop($("#pasteTemplateToolbar").outerHeight() + tbb);

    updateAllSizeAndPositions();

    $('#templateTextArea').focus();
}

function isShowingPasteTemplateToolbar() {
    return $("#pasteTemplateToolbar").css("display") != 'none';
}

function hidePasteTemplateToolbar() {
    var ptt = document.getElementById('pasteTemplateToolbar');
    ptt.style.display = 'none';
}

function updatePasteTemplateToolbarPosition() {
    let wh = window.visualViewport.height;
    let ptth = $("#pasteTemplateToolbar").outerHeight();
    $("#pasteTemplateToolbar").css("top", wh - ptth);
}

function gotoNextTemplate() {
    var tl = getUsedTemplateDefinitions();
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i]['templateGuid'] == getFocusTemplateGuid()) {
            curIdx = i;
            break;
        }
    }
    var nextIdx = curIdx + 1;
    if (nextIdx >= tl.length) {
        nextIdx = 0;
    }
    focusTemplate(tl[nextIdx]['templateGuid']);
}

function gotoPrevTemplate() {
    var tl = getUsedTemplateDefinitions();
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i]['templateGuid'] == getFocusTemplateGuid()) {
            curIdx = i;
            break;
        }
    }
    var prevIdx = curIdx - 1;
    if (prevIdx < 0) {
        prevIdx = tl.length - 1;
    }
    focusTemplate(tl[prevIdx]['templateGuid']);
}

function clearAllTemplateText() {
    var tl = getUsedTemplateDefinitions();
    for (var i = 0; i < tl.length; i++) {
        setTemplateText(tl[i]['templateGuid'], '');
    }
}

function setTemplateText(tguid, text) {
    //var stl = document.getElementsByClassName("ql-template-embed-blot");
    var stl = document.getElementsByClassName(TemplateEmbedClass);
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.getAttribute('templateGuid') == tguid) {
            t.innerText = text;
            t.templateText = text;
        }
    }
}

function createTemplateSelectorStyling() {
    var tl = getUsedTemplateDefinitions();
    var x, i, j, selElmnt, a, b, c;
    x = document.getElementsByClassName("paste-template-custom-select");
    for (i = 0; i < x.length; i++) {
        selElmnt = x[i].getElementsByTagName("select")[0];
        if (selElmnt.options.length == 0) {
            continue;
        }
        var st = getTemplateDefByGuid(selElmnt.options[selElmnt.selectedIndex].getAttribute('templateGuid'));
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a2 = document.createElement("SPAN");
        a2.setAttribute("class", "square-box");
        a2.setAttribute("style", "background-color: " + st['templateColor']);
        a.appendChild(a2);
        a.innerHTML += '<span style="margin: 0 10px 0 50px;">' + selElmnt.options[selElmnt.selectedIndex].innerHTML + '</span>';
        x[i].appendChild(a);
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        for (j = 0; j < selElmnt.length; j++) {
            var t = getTemplateDefByGuid(selElmnt.options[j].getAttribute('value'));
            c = document.createElement("DIV");
            c.setAttribute('optionId', t['templateGuid']);
            //c.innerHTML = selElmnt.options[j].innerHTML;
            d = document.createElement("SPAN");
            d.setAttribute("class", "square-box");
            d.setAttribute("style", "background-color: " + t['templateColor']);
            c.appendChild(d);
            c.innerHTML += selElmnt.options[j].innerHTML;
            c.addEventListener("click", onTemplateOptionClick);
            b.appendChild(c);
            if (j == selElmnt.selectedIndex) {
                c.classList.add("class", "same-as-selected");
            }
        }
        x[i].appendChild(b);
        a.addEventListener("click", function (e) {
            e.stopPropagation();
            closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");
        });
    }

    function closeAllSelect(elmnt) {
        var x, y, i, arrNo = [];
        x = document.getElementsByClassName("select-items");
        y = document.getElementsByClassName("select-selected");
        for (i = 0; i < y.length; i++) {
            if (elmnt == y[i]) {
                arrNo.push(i)
            } else {
                y[i].classList.remove("select-arrow-active");
            }
        }
        for (i = 0; i < x.length; i++) {
            if (arrNo.indexOf(i)) {
                x[i].classList.add("select-hide");
            }
        }
    }
    document.addEventListener("click", closeAllSelect);
}

function onTemplateOptionClick(e, target) {
    var targetDiv = e == null ? target : e.currentTarget;
    var tVal = targetDiv.getAttribute('optionId');
    var t = getTemplateDefByGuid(tVal);
    var y, i, k, s, h;
    s = targetDiv.parentNode.parentNode.getElementsByTagName("select")[0];
    h = targetDiv.parentNode.previousSibling;
    for (i = 0; i < s.length; i++) {
        var sVal = s.options[i].value;
        if (sVal == tVal) {
            s.selectedIndex = i;
            h.innerHTML = targetDiv.innerHTML.replaceAll(t['templateName'], '<span style="margin: 0 10px 0 50px;">' + t['templateName'] + '</span>');
            y = targetDiv.parentNode.getElementsByClassName("same-as-selected");
            for (k = 0; k < y.length; k++) {
                y[k].removeAttribute("class");
            }
            targetDiv.setAttribute("class", "same-as-selected");
            break;
        }
    }

    focusTemplate(s.options[s.selectedIndex].value,null, true);
    h.click();
}