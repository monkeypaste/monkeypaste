
function registerFontFamilys(envName) {
    let fontFamilys = getFontsByEnv(envName);
    var fontNames = fontFamilys.map(x => x.toLowerCase().replaceAll(' ', '-'));

    let fonts = Quill.import('formats/font');

    //var fonts = Quill.import('attributors/class/font');
    fonts.whitelist = fontNames;
    Quill.register(fonts, true);

    return fonts;
}

function registerFontSizes() {
    let fontSizes = ['8px', '9px', '10px', '12px', '14px', '16px', '20px', '24px', '32px', '42px', '54px', '68px', '84px', '98px'];

    var size = Quill.import('attributors/style/size');
    size.whitelist = fontSizes;
    Quill.register(size, true);

    return fontSizes;
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

function registerFontStyles(envName) {
    let fontFamilys = getFontsByEnv(envName);

    let fontCssStr = fontFamilys.map(x => getFontFamilyCssStr(x)).join(' ');

    return fontCssStr;

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
}


function getFontName(font) {
    // Generate code-friendly font names
    //return font.toLowerCase().replace(/\s/g, "-");
    return font.toLowerCase().replaceAll(' ', '-');
}

function refreshFontSizePicker() {
    let curFormat = quill.getFormat();
    let curFontSize = curFormat != null && curFormat.hasOwnProperty('size') && curFormat.size.length > 0 ? parseInt(curFormat.size)+'px' : defaultFontSize;
    let fontSizeFound = false;

    document
        .getElementsByClassName('ql-size ql-picker')[0]
        .getElementsByClassName('ql-picker-label')[0]
        .setAttribute('data-value', curFontSize);

    Array
        .from(
            document
                .getElementsByClassName('ql-size ql-picker')[0]
                .getElementsByClassName('ql-picker-options')[0]
                .children)
        .forEach((fontSizeSpan) => {

            fontSizeSpan.classList.remove('ql-selected');
            if (fontSizeSpan.getAttribute('data-value').toLowerCase() == curFontSize.toLowerCase()) {
                fontSizeSpan.classList.add('ql-selected');
                fontSizeFound = true;
            }
        });

    if (!fontSizeFound) {
        let sizeElm = document
            .getElementsByClassName('ql-size ql-picker')[0]
            .getElementsByClassName('ql-picker-options')[0].firstChild.cloneNode();

        sizeElm.setAttribute('data-value', curFontSize);
        sizeElm.classList.add('ql-selected');
        document
            .getElementsByClassName('ql-size ql-picker')[0]
            .getElementsByClassName('ql-picker-options')[0]
            .innerHTML += sizeElm.outerHTML;
    }
}

function refreshFontFamilyPicker() {
    let curFormat = quill.getFormat();
    let curFontFamily = curFormat != null && curFormat.hasOwnProperty('font') ? curFormat.font : defaultFontFamily;
    let fontFamilyFound = false;

    document
        .getElementsByClassName('ql-font ql-picker')[0]
        .getElementsByClassName('ql-picker-label')[0]
        .setAttribute('data-value', curFontFamily);

    Array
        .from(document.getElementsByClassName('ql-font ql-picker')[0].getElementsByClassName('ql-picker-options')[0].children)
        .forEach((fontFamilySpan) => {

            fontFamilySpan.classList.remove('ql-selected');
            if (fontFamilySpan.getAttribute('data-value').toLowerCase() == curFontFamily.toLowerCase()) {
                fontFamilySpan.classList.add('ql-selected');
                fontFamilyFound = true;
            }
        });

    if (!fontFamilyFound) {
        let familyElm = document
            .getElementsByClassName('ql-font ql-picker')[0]
            .getElementsByClassName('ql-picker-options')[0].firstChild.cloneNode();

        familyElm.setAttribute('data-value', curFontFamily);
        familyElm.classList.add('ql-selected');
        document
            .getElementsByClassName('ql-font ql-picker')[0]
            .getElementsByClassName('ql-picker-options')[0]
            .innerHTML += familyElm.outerHTML;
    }
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