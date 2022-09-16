var DefaultFontFamily = 'Arial';
var IsFontFamilyPickerOpen = false;

function addFontFamiliesToQuillContainerOptions(container) {
    let fonts = registerFontFamilys();
    container.unshift([{ font: fonts.whitelist }]);

    return container;
}

function initFontFamilyPicker() {
    // needs to be called after quill init

    let ffp_elm = document.getElementsByClassName('ql-font ql-picker')[0];
    ffp_elm.addEventListener('click', (e) => {
        return;
        IsFontFamilyPickerOpen = true;
        let blurred_sel = getSelection(); 
        if (!blurred_sel) {
            blurred_sel = BlurredSelectionRange;
            if (!blurred_sel) {
                // should be caught in onEditorSelChange but who knows anymore
                debugger;
                return;
			}
		}
        let ffp_opts_elm = ffp_elm.getElementsByClassName('ql-picker-options')[0];
        ffp_opts_elm.addEventListener('click', (e2) => {
            let ff = e2.target.getAttribute('data-value');
            if (ff.length == 0) {
                debugger;
                return;
            }
            ffp_elm.classList.remove('ql-expanded');
            IsFontFamilyPickerOpen = false;
            //quill.focus();
            //quill.formatText(blurred_sel.index, blurred_sel.length, 'font', ff);
            refreshFontFamilyPicker(ff);
            return;
        });
        
        return;
    });
}

function registerFontFamilys() {
    let fontFamilys = getFontsByEnv(EnvName);
    var fontNames = fontFamilys.map(x => x.toLowerCase().replaceAll(' ', '-'));

    let fonts = Quill.import('formats/font');

    //var fonts = Quill.import('attributors/class/font');
    fonts.whitelist = fontNames;
    Quill.register(fonts, true);

    return fonts;
}

function registerFontStyles(envName) {
    //// Add fonts to CSS style
    //var fontStyles = "";
    //fontFamilys.forEach(function (font) {
    //    var fontName = getFontName(font);
    //    fontStyles += ".ql-snow .ql-picker.ql-font .ql-picker-label[data-value=" + fontName + "]::before, .ql-snow .ql-picker.ql-font .ql-picker-item[data-value=" + fontName + "]::before {" +
    //        "content: '" + font + "';" +
    //        "font-family: '" + font + "', sans-serif;" +
    //        "}" +
    //        ".ql-font-" + fontName + "{" +
    //        " font-family: '" + font + "', sans-serif;" +
    //        "}";
    //});

    //return fontStyles;

    let fontFamilys = getFontsByEnv(envName);

    let fontNames = fontFamilys.map(x => getFontFamilyCssStr(x)).join(' ');
    return fontNames;

    //// Add fonts to CSS style
    //var fontstyles = "";
    
    //fontFamilys.forEach(function (font) {
    //        var fontName = getFontFamilyDataValue(font);
    //        fontstyles += ".ql-snow .ql-picker.ql-font .ql-picker-label[data-value=" + fontName + "]::before, .ql-snow .ql-picker.ql-font .ql-picker-item[data-value=" + fontName + "]::before {" +
    //            "content: '" + font + "';" +
    //            "font-family: '" + font + "', sans-serif;" +
    //            "}" +
    //            ".ql-font-" + fontName + "{" +
    //            " font-family: '" + font + "', sans-serif;" +
    //            "}";
    //    });

    //return fontstyles;
}

function getFontFamilyCssStr(ff) {
    let fontFamilyDropDownCssTemplateStr = "" +
        ".ql-snow .ql-picker.ql-font .ql-picker-label[data-value='times-new-roman']::before, " +
        ".ql-snow .ql-picker.ql-font .ql-picker-item[data-value='times-new-roman']::before {" +
        "content: 'Times New Roman';" +
        "font-family: 'Times New Roman', sans-serif; }";

    //Set the font-family content used for the HTML content.
    let fontFamilyContentTemplateStr = "" +
        ".ql-font-times-new-roman {" +
        "font-family: 'Times New Roman', sans-serif;" +
        "}";

    return fontFamilyDropDownCssTemplateStr
        .replaceAll('times-new-roman', ff.toLowerCase().replaceAll(' ', '-'))
        .replaceAll('Times New Roman', ff) +
        ' ' +
        fontFamilyContentTemplateStr
            .replaceAll('times-new-roman', ff.toLowerCase().replaceAll(' ', '-'))
            .replaceAll('Times New Roman', ff);
}

function refreshFontFamilyPicker(forceFamily = null) {
    if (IsFontFamilyPickerOpen) {
        return;
    }
    let curFontFamily = forceFamily;
    if (curFontFamily == null) {
        //use selection leaf and iterate up tree until font family is defined or return empty string
        curFontFamily = findSelectionFontFamily();
        if (curFontFamily == null || curFontFamily == '') {
            // debugger;
        }
	}
    let curFontFamily_dataValue = getFontFamilyDataValue(curFontFamily);

    let fontFamilyFound = false;

    //set font family picker to found font family (may need to use default if none found)
    let font_family_picker_elm = document.getElementsByClassName('ql-font ql-picker')[0];
    let font_family_picker_label_elm = font_family_picker_elm.getElementsByClassName('ql-picker-label')[0];
    let font_family_picker_options_elm = font_family_picker_elm.getElementsByClassName('ql-picker-options')[0];

    font_family_picker_label_elm.setAttribute('data-value', curFontFamily_dataValue);

    //iterate through font picker items and clear selection and if there's match set as selected
    Array.from(font_family_picker_options_elm.children)
        .forEach((fontFamilySpan) => {
            fontFamilySpan.classList.remove('ql-selected');
            if (fontFamilySpan.getAttribute('data-value') == curFontFamily_dataValue) {
                fontFamilySpan.classList.add('ql-selected');
                fontFamilyFound = true;
            }
        });

    if (!fontFamilyFound) {
        let familyElm = font_family_picker_options_elm.firstChild.cloneNode();

        familyElm.setAttribute('data-value', curFontFamily);
        familyElm.classList.add('ql-selected');
        font_family_picker_label_elm.innerHTML += familyElm.outerHTML;
    }
}

function findSelectionFontFamily() {
    let curFormat = quill.getFormat();
    let curFontFamily = curFormat != null && curFormat.hasOwnProperty('font') ? curFormat.font : null;// DefaultFontFamily;
    if (curFontFamily) {
        return curFontFamily;
	}
    var selection = quill.getSelection();
    if (!selection) {
        // selection outside editor, shouldn't happen since EditorSelectionChange handler forces oldSelection but check timing if occurs
        debugger;
        return '';
    } 
    let [leaf, offset] = quill.getLeaf(selection.index);
    if (leaf.parent && leaf.parent.domNode) {
                    let parentBlot = leaf.parent;
                    while (parentBlot) {
                        let fontFamilyParts = parentBlot.domNode.style.fontFamily.split(',');
                        if (fontFamilyParts.length > 0 && fontFamilyParts[0].length > 0) {
                            curFontFamily = fontFamilyParts[0].trim().replace(/"/g, '');
                            break;
                        }
                        parentBlot = parentBlot.parent;
        }                
   //     let parent_elm = leaf.parent.domNode;
   //     while (parent_elm) {
   //         if (parent_elm.style['fontFamily'] != null && parent_elm.style['fontFamily'] != '') {
   //             let fontFamilyParts = parent_elm.style.fontFamily.split(','); //getElementFontFamily(parent_elm).split(',');
   //             if (fontFamilyParts.length > 0 && fontFamilyParts[0].length > 0) {
   //                 let found_font_family = fontFamilyParts[0].trim().replace(/"/g, '');
   //                 return found_font_family;
   //             }
			//}
   //         parent_elm = parent_elm.parentNode;
   //     }
    }
    return '';
}

function getElementFontFamily(elm) {
    let elmStyles = window.getComputedStyle(elm);
    let ff = elmStyles.getPropertyValue('font-family');
    return ff;
}


function getFontFamilyDataValue(fontFamily) {
    // Generate code-friendly font names
    if (fontFamily) {
        return fontFamily.toLowerCase().replace(/\s/g, "-");
    }
    return '';
}

function getFontsByEnv(env) {
    env = env == null ? 'wpf' : env;
    if (env == null || env == 'wpf' || env == 'web') {
        return winFonts;
    } else if (env == 'mac') {
        return macFonts;
    }
}

const winFonts = [
    'Arial',
    'Bahnschrift',
    'Calibri',
    'Cambria',
    'Cambria Math',
    'Candara',
    'Comic Sans MS',
    'Consolas',
    'Constantia',
    'Corbel',
    'Courier New',
    'Ebrima',
    'Franklin Gothic',
    'Gabriola',
    'Gadugi',
    'Georgia',
    'Impact',
    'Ink Free',
    'Javanese Text',
    'Leelawadee UI',
    'Lucida Console',
    'Lucida Sans Unicode',
    'Malgun Gothic',
    'Microsoft Himalaya',
    'Microsoft JhengHei',
    'Microsoft JhengHei UI',
    'Microsoft New Tai Lue',
    'Microsoft PhagsPa',
    'Microsoft Sans Serif',
    'Microsoft Tai Le',
    'Microsoft YaHei',
    'Microsoft YaHei UI',
    'Microsoft Yi Baiti',
    'MingLiU-ExtB',
    'PMingLiU-ExtB',
    'MingLiU_HKSCS-ExtB',
    'Mongolian Baiti',
    'MS Gothic',
    'MS UI Gothic',
    'MS PGothic',
    'MV Boli',
    'Myanmar Text',
    'Nirmala UI',
    'Palatino Linotype',
    'Segoe MDL2 Assets',
    'Segoe Print',
    'Segoe Script',
    'Segoe UI',
    'Segoe UI Emoji',
    'Segoe UI Historic',
    'Segoe UI Symbol',
    'SimSun',
    'NSimSun',
    'SimSun-ExtB',
    'Sitka Small',
    'Sitka Text',
    'Sitka Subheading',
    'Sitka Heading',
    'Sitka Display',
    'Sitka Banner',
    'Sylfaen',
    'Symbol',
    'Tahoma',
    'Times New Roman',
    'Trebuchet MS',
    'Verdana',
    'Webdings',
    'Wingdings',
    'Yu Gothic',
    'Yu Gothic UI',
    'HoloLens MDL2 Assets',
    'Agency FB',
    'Algerian',
    'Book Antiqua',
    'Arial Rounded MT',
    'Baskerville Old Face',
    'Bauhaus 93',
    'Bell MT',
    'Bernard MT',
    'Bodoni MT',
    'Bodoni MT Poster',
    'Bookman Old Style',
    'Bradley Hand ITC',
    'Britannic',
    'Berlin Sans FB',
    'Broadway',
    'Brush Script MT',
    'Californian FB',
    'Calisto MT',
    'Castellar',
    'Century Schoolbook',
    'Centaur',
    'Century',
    'Chiller',
    'Colonna MT',
    'Cooper',
    'Copperplate Gothic',
    'Curlz MT',
    'Dubai',
    'Elephant',
    'Engravers MT',
    'Eras ITC',
    'Felix Titling',
    'Forte',
    'Franklin Gothic Book',
    'Freestyle Script',
    'French Script MT',
    'Footlight MT',
    'Garamond',
    'Gigi',
    'Gill Sans MT',
    'Gill Sans',
    'Gloucester MT',
    'Century Gothic',
    'Goudy Old Style',
    'Goudy Stout',
    'Harlow Solid',
    'Harrington',
    'Haettenschweiler',
    'High Tower Text',
    'Imprint MT Shadow',
    'Informal Roman',
    'Blackadder ITC',
    'Edwardian Script ITC',
    'Kristen ITC',
    'Jokerman',
    'Juice ITC',
    'Kunstler Script',
    'Wide Latin',
    'Lucida Bright',
    'Lucida Calligraphy',
    'Lucida Fax',
    'Lucida Handwriting',
    'Lucida Sans',
    'Lucida Sans Typewriter',
    'Magneto',
    'Maiandra GD',
    'Matura MT Script Capitals',
    'Mistral',
    'Modern No. 20',
    'Monotype Corsiva',
    'Niagara Engraved',
    'Niagara Solid',
    'OCR A',
    'Old English Text MT',
    'Onyx',
    'Palace Script MT',
    'Papyrus',
    'Parchment',
    'Perpetua',
    'Perpetua Titling MT',
    'Playbill',
    'Poor Richard',
    'Pristina',
    'Rage',
    'Ravie',
    'Rockwell',
    'Script MT',
    'Showcard Gothic',
    'Snap ITC',
    'Stencil',
    'Tw Cen MT',
    'Tempus Sans ITC',
    'Viner Hand ITC',
    'Vivaldi',
    'Vladimir Script',
    'Wingdings 2',
    'Wingdings 3',
    'Opus Chords Std',
    'Opus Figured Bass Extras Std',
    'Opus Figured Bass Std',
    'Opus Function Symbols Std',
    'Opus Metronome Std',
    'Opus Note Names Std',
    'Opus Ornaments Std',
    'Opus Percussion Std',
    'Opus PlainChords Std',
    'Opus Roman Chords Std',
    'Opus Special Extra Std',
    'Opus Special Std',
    'Opus Std',
    'Opus Text Std',
    'Opus Big Time Std',
    'Opus Chords Sans Std',
    'Arial Unicode MS',
    'DengXian',
    'FangSong',
    'KaiTi',
    'SimHei',
    'Leelawadee',
    'Microsoft Uighur',
    'MS Mincho',
    'MS Outlook',
    'Bookshelf Symbol 7',
    'MS Reference Sans Serif',
    'MS Reference Specialty',
    'Marlett',
    'Global User Interface',
    'Global Monospace',
    'Global Sans Serif',
    'Global Serif'
];

const macFonts = [
    'American Typewriter',
    'Andale Mono',
    'Arial',
    'Arial Black',
    'Arial Narrow',
    'Arial Rounded MT Bold',
    'Arial Unicode MS',
    'Avenir',
    'Avenir Next',
    'Avenir Next Condensed',
    'Baskerville',
    'Big Caslon',
    'Bodoni 72',
    'Bodoni 72 Oldstyle',
    'Bodoni 72 Smallcaps',
    'Bradley Hand',
    'Brush Script MT',
    'Chalkboard',
    'Chalkboard SE',
    'Chalkduster',
    'Charter',
    'Cochin',
    'Comic Sans MS',
    'Copperplate',
    'Courier',
    'Courier New',
    'Didot',
    'DIN Alternate',
    'DIN Condensed',
    'Futura',
    'Geneva',
    'Georgia',
    'Gill Sans',
    'Helvetica',
    'Helvetica Neue',
    'Herculanum',
    'Hoefler Text',
    'Impact',
    'Lucida Grande',
    'Luminari',
    'Marker Felt',
    'Menlo',
    'Microsoft Sans Serif',
    'Monaco',
    'Noteworthy',
    'Optima',
    'Palatino',
    'Papyrus',
    'Phosphate',
    'Rockwell',
    'Savoye LET',
    'SignPainter',
    'Skia',
    'Snell Roundhand',
    'Tahoma',
    'Times',
    'Times New Roman',
    'Trattatello',
    'Trebuchet MS',
    'Verdana',
    'Zapfino'
];