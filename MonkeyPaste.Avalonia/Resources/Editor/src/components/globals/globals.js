const globals = {

	// #region ENV
	WindowsEnv: 'Windows',
	IosEnv: 'Ios',
	AndroidEnv: 'Android',
	MacEnv: 'Mac',
	LinuxEnv: 'Linux',
	WebEnv: 'Web',
	UknownEnv: 'Unknown',
	EnvName: "",
	// #endregion 

	// #region MAIN
	IsLoaded: false,
	DomParser: new DOMParser(),
	DomSerializer: new XMLSerializer(),
	IsDebug: true,
	IsTesting: false,
	IsSpellCheckEnabled: false,
	MaxUndoLimit: -1,
	// #endregion 

	// #region EDITOR
	DefaultEditorWidth: 1200,
	InlineTags: ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'],
	BlockTags: ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote', 'pre'],
	
	IgnoreNextSelectionChange: false,
	SuppressTextChangedNtf: false,
	EditorTheme: 'light',
	IsToolbarsLoaded: false,
	// #endregion

	// #region QUILL
	quill: null,

	// #endregion

	// #region ALIGN
	AlignOptionItems: [
		{
			icon: 'align-left',
			classes: 'icon-only'
		},
		{
			icon: 'align-center',
			classes: 'icon-only'
		},
		{
			icon: 'align-right',
			classes: 'icon-only'
		},
		{
			icon: 'align-justify',
			classes: 'icon-only'
		}
	],
	AlignLeftOptIdx: 0,
	AlignCenterOptIdx: 1,
	AlignRightOptIdx: 2,
	AlignJustifyOptIdx: 3,
	// #endregion 

	// #region ANNOTATIONS
	RootAnnotations: [],
	SelectedAnnotationGuid:  null,
	HoverAnnotationGuid: null,
	// #endregion 

	// #region APPEND
	IsAppendInsertMode:  false,
	IsAppendLineMode:  false,

	IsAppendManualMode:  false,
	IsAppendPreMode:  false,
	IsAppendPaused:  false,

	FixedAppendIdx:  -1,
	// #endregion

	// #region CLIPBOARD

	PLACEHOLDER_DATAOBJECT_TEXT:'3acaaed7-862d-47f5-8614-3259d40fce4d',
	// #endregion

	// #region COLORS

	// #region CONSTANTS
	ContentColors: [
		"#F8A0AE",
		"#FCA845",
		"#D79D3C",
		"#F7F590",
		"#BDFE28",
		"#32FF4C",
		"#36FFAD",
		"#60FFE3",
		"#95CCF3",
		"#638DE3",
		"#9DB0FF",
		"#DD7EE6",
		"#E167A4",
		"#FFFFFF",
		"#F34544",
		"#FB6C28",
		"#A87B52",
		"#FCF04E",
		"#8FFE73",
		"#44C721",
		"#20C3B2",
		"#2EEEF9",
		"#2BA7ED",
		"#167FC1",
		"#947FDC",
		"#BA8DC8",
		"#FC4AD2",
		"#DFDFDF",
		"#E57466",
		"#FDAA82",
		"#D6B685",
		"#EFFEB9",
		"#D9E7AA",
		"#C1D687",
		"#AACEA0",
		"#DAFDE9",
		"#D7F4F8",
		"#C9CFE9",
		"#D8CBE9",
		"#B9A9E7",
		"#EEE9ED",
		"#BBBBBB",
		"#D39FA1",
		"#BD8D67",
		"#A2907A",
		"#C6C17F",
		"#ACB726",
		"#7FB663",
		"#A0C9C5",
		"#AEC1D0",
		"#99B2C6",
		"#96A3D0",
		"#B4A8C0",
		"#CBB2C8",
		"#C384A3",
		"#000000",
		"#BF3532",
		"#B15637",
		"#7B5548",
		"#E0C82A",
		"#8C9D2D",
		"#5CAA3A",
		"#209F94",
		"#286792",
		"#1E33A0",
		"#3459AA",
		"#6D5AB3",
		"#AA5AB3",
		"#CD3C75",
		"000000"],
	CssColorLookup: {
		"aliceblue": "#f0f8ff", "antiquewhite": "#faebd7", "aqua": "#00ffff", "aquamarine": "#7fffd4", "azure": "#f0ffff",
		"beige": "#f5f5dc", "bisque": "#ffe4c4", "black": "#000000", "blanchedalmond": "#ffebcd", "blue": "#0000ff", "blueviolet": "#8a2be2", "brown": "#a52a2a", "burlywood": "#deb887",
		"cadetblue": "#5f9ea0", "chartreuse": "#7fff00", "chocolate": "#d2691e", "coral": "#ff7f50", "cornflowerblue": "#6495ed", "cornsilk": "#fff8dc", "crimson": "#dc143c", "cyan": "#00ffff",
		"darkblue": "#00008b", "darkcyan": "#008b8b", "darkgoldenrod": "#b8860b", "darkgray": "#a9a9a9", "darkgreen": "#006400", "darkkhaki": "#bdb76b", "darkmagenta": "#8b008b", "darkolivegreen": "#556b2f",
		"darkorange": "#ff8c00", "darkorchid": "#9932cc", "darkred": "#8b0000", "darksalmon": "#e9967a", "darkseagreen": "#8fbc8f", "darkslateblue": "#483d8b", "darkslategray": "#2f4f4f", "darkturquoise": "#00ced1",
		"darkviolet": "#9400d3", "deeppink": "#ff1493", "deepskyblue": "#00bfff", "dimgray": "#696969", "dodgerblue": "#1e90ff",
		"firebrick": "#b22222", "floralwhite": "#fffaf0", "forestgreen": "#228b22", "fuchsia": "#ff00ff",
		"gainsboro": "#dcdcdc", "ghostwhite": "#f8f8ff", "gold": "#ffd700", "goldenrod": "#daa520", "gray": "#808080", "green": "#008000", "greenyellow": "#adff2f",
		"honeydew": "#f0fff0", "hotpink": "#ff69b4",
		"indianred ": "#cd5c5c", "indigo": "#4b0082", "ivory": "#fffff0", "khaki": "#f0e68c",
		"lavender": "#e6e6fa", "lavenderblush": "#fff0f5", "lawngreen": "#7cfc00", "lemonchiffon": "#fffacd", "lightblue": "#add8e6", "lightcoral": "#f08080", "lightcyan": "#e0ffff", "lightgoldenrodyellow": "#fafad2",
		"lightgrey": "#d3d3d3", "lightgreen": "#90ee90", "lightpink": "#ffb6c1", "lightsalmon": "#ffa07a", "lightseagreen": "#20b2aa", "lightskyblue": "#87cefa", "lightslategray": "#778899", "lightsteelblue": "#b0c4de",
		"lightyellow": "#ffffe0", "lime": "#00ff00", "limegreen": "#32cd32", "linen": "#faf0e6",
		"magenta": "#ff00ff", "maroon": "#800000", "mediumaquamarine": "#66cdaa", "mediumblue": "#0000cd", "mediumorchid": "#ba55d3", "mediumpurple": "#9370d8", "mediumseagreen": "#3cb371", "mediumslateblue": "#7b68ee",
		"mediumspringgreen": "#00fa9a", "mediumturquoise": "#48d1cc", "mediumvioletred": "#c71585", "midnightblue": "#191970", "mintcream": "#f5fffa", "mistyrose": "#ffe4e1", "moccasin": "#ffe4b5",
		"navajowhite": "#ffdead", "navy": "#000080",
		"oldlace": "#fdf5e6", "olive": "#808000", "olivedrab": "#6b8e23", "orange": "#ffa500", "orangered": "#ff4500", "orchid": "#da70d6",
		"palegoldenrod": "#eee8aa", "palegreen": "#98fb98", "paleturquoise": "#afeeee", "palevioletred": "#d87093", "papayawhip": "#ffefd5", "peachpuff": "#ffdab9", "peru": "#cd853f", "pink": "#ffc0cb", "plum": "#dda0dd", "powderblue": "#b0e0e6", "purple": "#800080",
		"rebeccapurple": "#663399", "red": "#ff0000", "rosybrown": "#bc8f8f", "royalblue": "#4169e1",
		"saddlebrown": "#8b4513", "salmon": "#fa8072", "sandybrown": "#f4a460", "seagreen": "#2e8b57", "seashell": "#fff5ee", "sienna": "#a0522d", "silver": "#c0c0c0", "skyblue": "#87ceeb", "slateblue": "#6a5acd", "slategray": "#708090", "snow": "#fffafa", "springgreen": "#00ff7f", "steelblue": "#4682b4",
		"tan": "#d2b48c", "teal": "#008080", "thistle": "#d8bfd8", "tomato": "#ff6347", "turquoise": "#40e0d0",
		"violet": "#ee82ee",
		"wheat": "#f5deb3", "white": "#ffffff", "whitesmoke": "#f5f5f5",
		"yellow": "#ffff00", "yellowgreen": "#9acd32", "transparent": "#00000000"
	},
	// #endregion

	// #region PALETTE
	ColorPaletteAnchorElement:  null,
	ColorPaletteAnchorResultCallback:  null,

	COLOR_PALETTE_ROW_COUNT:  5,
	COLOR_PALETTE_COL_COUNT:  14,

	IsCustomColorPaletteOpen:  false,
	// #endregion

	// #endregion

	// #region CONVERTER
	IsConverterLoaded: false,

	// #endregion

	// #region DATA TRANSFER

	LOCAL_HOST_URL:  'https://localhost',
	URL_DATA_FORMAT:  "uniformresourcelocator",
	URI_LIST_FORMAT:  'text/uri-list',
	HTML_FORMAT:  'text/html',
	HTML_FRAGMENT_FORMAT:  'html format',
	TEXT_FORMAT:  'text/plain',

	// #endregion

	// #region HTML FRAGMENT

	TableDeltaAttrbs: [
		'table-col',
		'table-cell-line',
		'row',
		'rowspan',
		'colspan'
	],

	// #endregion

	// #region DND

	// #region DEBOUNCER
	LastDebouncedMouseLoc: null,
	LastDragOverDateTime: null,
	// #endregion

	// #region DRAG
	MIN_DRAG_DIST:  10,
	WasNoSelectBeforeDragStart:  false,
	CurDragTargetElm:  null,
	DragItemElms:  [],
	// #endregion

	// #region DROP
	DropIdx:  -1,

	AllowedEffects:  ['copy', 'copyLink', 'copyMove', 'link', 'linkMove', 'move', 'all'],

	AllowedDropTypes:  ['text/plain', 'text/html', 'application/json', 'files'],

	CurDropTargetElm:  null,

	DropItemElms: [],
	DropMoveLineColor: 'red',
	DropCopyLineColor: 'green',
	// #endregion
	// #endregion

	// #region SELECTION

	DefaultSelectionBgColor: 'lightblue',
	DefaultSelectionFgColor: 'black',
	DefaultCaretColor: 'black',
	SubSelCaretColor: 'red',
	DragSelBgColor: 'salmon',
	DragFormatedSelFgColor: 'orange',
	DragCopySelBgColor: 'green',
	LastSelRange: null,
	CurSelRange: { index: 0, length: 0 },
	SelectionOnMouseDown: null,
	// #endregion

	// #region FIND/REPLACE

	CurFindReplaceDocRanges:  null,
	CurFindReplaceDocRangeIdx:  -1,

	CurFindReplaceDocRangeRectIdxLookup:  null,

	CurFindReplaceDocRangesRects:  null,

	DefaultFindReplaceInputState:  {
		searchText: '',
		replaceText: '',
		isReplace: false,
		isCaseSensitive: false,
		isWholeWordMatch: false,
		useRegEx: false
	},

	LastFindReplaceInputState:  null,

	IsFindReplaceInactive:  true,

	Searches:  null,
	// #endregion

	// #region FOCUS
	IsHostFocused: false,
	// #endregion

	// #region FONT

	// #region FONT COLOR
	FontColorOverrideAttrb: null,
	FontBgColorOverrideAttrb: null,
	// #endregion

	// #region FONT FAMILY
	DefaultFontFamily: 'Arial',
	IsFontFamilyPickerOpen: false,
	EnvFonts: null,
	winFonts: [
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
		'Helvetica',
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
	],
	macFonts: [
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
	],
	// #endregion

	// #region FONT SIZE
	DefaultFontSizes:  ['8px', '9px', '10px', '12px', '14px', '16px', '20px', '24px', '32px', '42px', '54px', '68px', '84px', '98px'],
	DefaultFontSize:  '12px',
	IsFontSizePickerOpen:  false,
	// #endregion

	// #endregion

	// #region INPUT

	// #region KEYBOARD
	DecreaseFocusLevelKey:  'Escape',
	IncreaseFocusLevelKey:  ' ',


	NavigationKeys:  [
		"ArrowLeft",
		"ArrowUp",
		"ArrowRight",
		"ArrowDown",
		//"Shift",
		//"Alt",
		//"Control",
		//"Meta",
		"Home",
		"End",
		"PageUp",
		"PageDown",
		//globals.DecreaseFocusLevelKey,
		//globals.IncreaseFocusLevelKey
	],

	IsMetaDown:  false, //duplicate (mac)
	IsCtrlDown:  false, //duplicate
	IsShiftDown:  false, //split 
	IsAltDown:  false, // w/ formatting (as html)? ONLY formating? dunno
	// #endregion

	// #region MOUSE

	WindowMouseDownLoc:  null,
	WindowMouseLoc:  null,

	WasSupressRightMouseDownSentToHost:  false,
	WasInternalContextMenuAbleToShow:  false,
	// #endregion
	// #endregion

	// #region CONTENT

	CONTENT_CLASS_PREFIX: 'content',
	ContentClassAttrb: null,
	ContentHandle: null,
	ContentItemType: 'Text',

	// #region TEXT
	// #endregion

	// #region FILE LIST
	FileListClassAttrb: null,
	FileListItems: [],
	// #endregion

	// #region IMAGE
	ContentImageWidth: -1,
	ContentImageHeight: -1,
	// #endregion

	// #endregion

	// #region LINKS
	RequiredNavigateUriModKeys:  [
		//'Control'
	],

	LinkTypes:  [
		'fileorfolder',
		'uri',
		'email',
		'phonenumber',
		'currency',
		'hexcolor',
		'streetaddress',
		'delete-item'
	],

	LinkTypeAttrb:  null,
	// #endregion

	// #region LISTS
	ENCODED_LIST_ITEM_OPEN_TOKEN:  "{li{",
	ENCODED_LIST_ITEM_CLOSE_TOKEN:  "}li}",
	ListOptionItems: [
		{
			icon: 'list-ordered',
			classes: 'icon-only'
		},
		{
			icon: 'list-bullet',
			classes: 'icon-only'
		},
		{
			icon: 'list-checkable',
			classes: 'icon-only'
		}
	],
	ListOrderedOptIdx: 0,
	ListBulletOptIdx: 1,
	ListCheckableOptIdx: 2,
	// #region CHECKABLE LISTS
	CheckableListItemAttributor: null,
	// #endregion

	// #endregion

	// #region MESSAGES
	PendingGetResponses: [],
	// #endregion

	// #region OVERLAY
	IS_OVERLAY_CARET_ENABLED:  true,


	CaretBlinkTickMs:  500,
	CaretBlinkOffColor:  null,
	CaretBlinkTimerInterval:  null,

	// #endregion

	// #region PASTE

	// #region PASTE BUTTON POPUP
	PastePopupMenuOptions:  [
		{
			label: 'Block',
			icon: 'append-outline'
		},
		{
			label: 'Inline',
			icon: 'insert-outline'
		},
		{
			separator: true
		},
		{
			label: 'Before',
			icon: 'arrow-left'
		},
		{
			label: 'After',
			icon: 'arrow-right'
		},
		{
			separator: true
		},
		{
			label: 'Manual',
			icon: 'text-insert-caret-outline'
		},
		{
			separator: true
		},
		{
			label: 'Done',
			icon: 'sign-out'
		},
		{
			label: 'Stack Mode',
			icon: 'append-outline'
		}
	],


	AppendLineOptIdx:  0,
	AppendInsertOptIdx:  1,
	AppendPreIdx:  3,
	AppendPostIdx:  4,
	ManualOptIdx:  6,
	DoneOptIdx:  8,
	StartOptIdx:  9,

	// #endregion

	// #region PASTE TOOLBAR

	MIN_TOOLBAR_HEIGHT:  65,
	// #endregion

	// #endregion

	// #region SCROLL

	SuppressNextEditorScrollChangedNotification:  false,

	LastScrollBarXIsVisible:  false,
	LastScrollBarYIsVisible:  false,
	// #region AUTO SCROLL

	AutoScrolledOffset:  null,

	AutoScrollVelX:  0,
	AutoScrollVelY:  0,

	AutoScrollAccumlator:  5,
	AutoScrollBaseVelocity:  25,

	MIN_AUTO_SCROLL_DIST:  30,

	AutoScrollInterval:  null,
	// #endregion
	// #endregion

	// #region SHORTCUTS
	SHORTCUT_STR_TOKEN:  '$$',

	SHORTCUT_TYPES:  [
		'ToggleAppendInsertMode',
		'ToggleAppendLineMode',
		'ToggleAppendPreMode',
		'ToggleAppendPaused',
		'ToggleAppendManualMode'
	],

	ShortcutKeysLookup:  {},
	// #endregion

	// #region TABLES
	TABLE_WRAPPER_CLASS_NAME:  'quill-better-table-wrapper',
	TABLE_COL_TOOLS_CLASS_NAME:  'qlbt-col-tool',
	TABLE_OPS_MENU_CLASS_NAME:  'qlbt-operation-menu',

	ALLOW_TABLE_OPS_MENU:  true,
	IS_TABLE_OPS_TOOLBAR_ENABLED:  false,

	IsTableOpsMenuEnabled:  true,
	IsTableInteractionEnabled:  true,

	DefaultCsvProps:  {
		ColSeparator: ',',
		RowSeparator: '\n'
	},

	IsTableDragSelecting:  false,
	// #region CREATE TABLES

	MAX_TABLE_ROWS:  7,
	MAX_TABLE_COLS:  7,
	// #endregion
	// #endregion

	// #region TEMPLATES

	MIN_TEMPLATE_DRAG_DIST: 5,
	ENCODED_TEMPLATE_OPEN_TOKEN: "{t{",
	ENCODED_TEMPLATE_CLOSE_TOKEN: "}t}",
	IS_SMART_TEMPLATE_NAV_ENABLED: false,
	availableTemplates: null,
	IsMovingTemplate: false,
	IsTemplatePaddingAfterTextChange: false,
	TemplateBeforeEdit: null,
	TemplateTypesMenuOptions: [
		{
			label: 'Dynamic',
			icon: 'text'
		},
		{
			label: 'Static',
			icon: 'snowflake'
		},
		/* {
			 label: 'Content',
			 icon: 'fa-solid fa-clipboard'
		 },
		 {
			 label: 'Analyzer',
			 icon: 'fa-solid fa-scale-balanced'
		 },
		 {
			 label: 'Action',
			 icon: 'fa-solid fa-bolt-lightning'
		 },
		{
			label: 'Contact',
			icon: 'contact'
		},*/
		{
			label: 'DateTime',
			icon: 'datetime'
		}
	],

	// #region TEMPLATE BLOT
	TemplateEmbedClass:  'template-blot',

	Template_AT_INSERT_Class:  'template-blot-at-insert',

	TemplateEmbedHtmlAttributes:  [
		'background-color',
		'wasVisited',
		'docIdx'
		//'color'

		//'templateGuid',
		//'templateInstanceGuid',
		//'isFocus',
		//'templateName',
		//'templateColor',
		//'templateText',
		//'templateType',
		//'templateData',
		//'templateDeltaFormat',
		//'templateHtmlFormat',
		//'wasVisited'
	],
	// #endregion

	// #region TEMPLATE CONTACTS 

	ContactFieldTypes: [
		'FirstName',
		'LastName',
		'FullName',
		'PhoneNumber',
		'Address',
		'Email',
	],
	AvailableContacts: [],
	// #endregion

	// #region TEMPLATE DATETIME
	CUSTOM_TEMPLATE_LABEL_VAL:  'Custom',

	
	// #endregion

	// #region TOOLTIP

TOOLTIP_HOVER_ATTRB_NAME:  "hover-tooltip",
TOOLTIP_TOOLBAR_ATTRB_NAME:  "toolbar-tooltip",
	// #endregion

	// #region WINDOW
EDITOR_WINDOW_NAME:  'MpEditorWindow',

IsWindowResizeUpdateEnabled:  true,
	// #endregion

	// #endregion 

	// #region SVG 
	SVG_CLASS_PREFIX: 'svg-key-',
	SVG_NO_DEFAULT_CLASS: 'svg-no-defaults',
	SVG_INNER_CLASS_ATTR: 'svg-classes',
	SvgElements: {
		'empty': "<svg xmlns='http://www.w3.org/2000/svg' height='48' width='48' viewBox='0 0 48 48'></svg>",
		'findreplace': "<svg xmlns='http://www.w3.org/2000/svg' height='48' width='48' viewBox='0 0 48 48'><path d='M10.2 19.4q-.85 0-1.3-.6-.45-.6-.2-1.35 1.4-4.25 5.075-6.85Q17.45 8 22 8q3.25 0 6.075 1.5T33 13.55V9.5q0-.65.425-1.075Q33.85 8 34.5 8q.65 0 1.075.425Q36 8.85 36 9.5v8.4q0 .65-.425 1.075-.425.425-1.075.425h-8.4q-.65 0-1.075-.425-.425-.425-.425-1.075 0-.65.425-1.075.425-.425 1.075-.425h5.2q-1.6-2.4-4.025-3.9Q24.85 11 22 11q-3.6 0-6.45 2.05t-4 5.35q-.15.4-.55.7-.4.3-.8.3Zm28.6 21.5-8.15-8.1q-1.85 1.55-4.05 2.35-2.2.8-4.6.8-3.25 0-6.125-1.35T11 30.7v3.8q0 .65-.425 1.075Q10.15 36 9.5 36q-.65 0-1.075-.425Q8 35.15 8 34.5v-8.4q0-.65.425-1.075Q8.85 24.6 9.5 24.6h8.4q.65 0 1.075.425.425.425.425 1.075 0 .65-.425 1.075-.425.425-1.075.425h-5.45q1.65 2.4 4.15 3.875 2.5 1.475 5.4 1.475 3.5 0 6.3-2.025t3.95-5.325q.15-.4.55-.7.4-.3.8-.3.85 0 1.3.6.45.6.2 1.35-.4 1.15-.975 2.15T32.8 30.65l8.15 8.15q.45.45.45 1.05 0 .6-.45 1.05-.45.45-1.075.45T38.8 40.9Z'/></svg>",
		'createtable': "<svg version='1.1'  xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 445 445' xml:space='preserve'><path d='M0,37.215v55v15v300.57h445v-300.57v-15v-55H0z M276.667,277.595H168.333v-70.19h108.334V277.595z M306.667,207.405H415	v70.19H306.667V207.405z M276.667,307.595v70.19H168.333v-70.19H276.667z M30,207.405h108.333v70.19H30V207.405z M168.333,177.405	v-70.19h108.334v70.19H168.333z M138.333,107.215v70.19H30v-70.19H138.333z M30,307.595h108.333v70.19H30V307.595z M306.667,377.785	v-70.19H415v70.19H306.667z M415,177.405H306.667v-70.19H415V177.405z'/><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>",
		'createtemplate': "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 640 512'><path d='M399.3 509.7c-58.2-8.8-108.2-72.8-137.6-119.7c-20-31.9-25.1-70.3-19.6-107.7L266.3 118c1.4-9.8 5.1-19.2 12.9-25.2c20.2-15.6 72.4-41.5 185.1-24.5s155.2 57.4 170 78.3c5.7 8 6.5 18.1 5.1 27.9L615.2 338.8c-5.5 37.3-21.5 72.6-49.8 97.2c-41.7 36.1-108 82.5-166.1 73.7zm17.1-277.7c.1-.5 .2-1.1 .3-1.6c3.2-21.8-11.6-42-33.1-45.3s-41.5 11.8-44.7 33.5c-.1 .5-.1 1.1-.2 1.6c-.6 5.4 5.2 8.4 10.3 6.7c9-3 18.8-3.9 28.7-2.4s19.1 5.3 26.8 10.8c4.4 3.1 10.8 2 11.8-3.3zm112.6 22.2c4.4 3.1 10.8 2 11.8-3.3c.1-.5 .2-1.1 .3-1.6c3.2-21.8-11.6-42-33.1-45.3s-41.5 11.8-44.7 33.5c-.1 .5-.1 1.1-.2 1.6c-.6 5.4 5.2 8.4 10.3 6.7c9-3 18.8-3.9 28.7-2.4s19.1 5.3 26.8 10.8zm-11.5 85.2c-28.8 12.8-61.4 17.8-94.9 12.8s-63.2-19.5-87-40.3c-6.3-5.5-16.2-1.7-15.2 6.7c5.9 48.5 43 89.1 93 96.7s97.2-20.2 116.8-64.9c3.4-7.7-5-14.3-12.6-10.9zM240.7 446.9c-58.2 8.8-124.5-37.5-166.1-73.7c-28.3-24.5-44.3-59.8-49.8-97.2L.6 111.8C-.8 102 0 91.9 5.7 83.9C20.5 63 63 22.7 175.7 5.6s164.9 8.9 185.1 24.5c.9 .7 1.7 1.4 2.4 2.1c-52.8 4.8-85.1 21-103.6 35.3c-17 13.1-23 32-25 45.9L215.3 244.7c-2.6 .1-5.2 .4-7.9 .8c-35.2 5.3-61.8 32.7-68.2 66.3c-1.6 8.2 8.3 12.2 14.8 7c15.6-12.4 34.1-21.3 54.7-25.4c-3 38.4 4 78.7 25.9 113.6c6.9 11 15 23.1 24.2 35.4c-5.9 2.1-11.9 3.6-18 4.5zM174.1 157c-1-5.3-7.4-6.4-11.8-3.3c-7.7 5.5-16.8 9.3-26.8 10.8s-19.8 .6-28.7-2.4c-5.1-1.7-10.9 1.3-10.3 6.7c.1 .5 .1 1.1 .2 1.6c3.2 21.8 23.2 36.8 44.7 33.5s36.3-23.5 33.1-45.3c-.1-.5-.2-1.1-.3-1.6z'/></svg>",
		'fontbg': "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1920 1920' xmlns:xlink='http://www.w3.org/1999/xlink'><rect x='0' y='0' width='1920' height='1920' style='fill: none;stroke: rgb(0,0,0); stroke-width: 50;'></rect><g> <path d='m742.81 1024.051 185.984-512h62.336l186.112 512H742.81Zm605.056 469.888 120.32-43.776-387.328-1066.112H839.194L451.866 1450.163l120.32 43.776 124.16-341.888h527.36l124.16 341.888ZM1792 1728.051c0 35.2-28.672 64-64 64H192c-35.328 0-64-28.8-64-64v-1536c0-35.2 28.672-64 64-64h1536c35.328 0 64 28.8 64 64v1536Zm-64-1728H192c-105.856 0-192 86.144-192 192v1536c0 105.856 86.144 192 192 192h1536c105.856 0 192-86.144 192-192v-1536c0-105.856-86.144-192-192-192Z' fill-rule='evenodd'></path> </g></svg>",
		'fontfg': "<svg width='1024px' height='1024px' viewBox='0 0 1024 1024' xmlns='http://www.w3.org/2000/svg' class='icon'><path d='M904 816H120c-4.4 0-8 3.6-8 8v80c0 4.4 3.6 8 8 8h784c4.4 0 8-3.6 8-8v-80c0-4.4-3.6-8-8-8zm-650.3-80h85c4.2 0 8-2.7 9.3-6.8l53.7-166h219.2l53.2 166c1.3 4 5 6.8 9.3 6.8h89.1c1.1 0 2.2-.2 3.2-.5a9.7 9.7 0 0 0 6-12.4L573.6 118.6a9.9 9.9 0 0 0-9.2-6.6H462.1c-4.2 0-7.9 2.6-9.2 6.6L244.5 723.1c-.4 1-.5 2.1-.5 3.2-.1 5.3 4.3 9.7 9.7 9.7zm255.9-516.1h4.1l83.8 263.8H424.9l84.7-263.8z'/></svg>",
		'text': "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 278 278' xml:space='preserve'><path d='M254.833,0h-98.5h-33h-99.5C15.549,0,8.5,6.716,8.5,15v33c0,8.284,6.716,15,15,15s15-6.716,15-15V30h69v218H89.833	c-8.284,0-15,6.716-15,15s6.716,15,15,15h99c8.284,0,15-6.716,15-15s-6.716-15-15-15H170.5V30h69v18c0,8.284,6.716,15,15,15	s15-6.716,15-15V15C269.5,6.716,263.117,0,254.833,0z'/><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>",
		'snowflake': "<svg width='512px' height='512px' viewBox='0 0 512 512' xmlns='http://www.w3.org/2000/svg'><path d='M461,349l-34-19.64a89.53,89.53,0,0,1,20.94-16,22,22,0,0,0-21.28-38.51,133.62,133.62,0,0,0-38.55,32.1L300,256l88.09-50.86a133.46,133.46,0,0,0,38.55,32.1,22,22,0,1,0,21.28-38.51,89.74,89.74,0,0,1-20.94-16l34-19.64A22,22,0,1,0,439,125l-34,19.63a89.74,89.74,0,0,1-3.42-26.15A22,22,0,0,0,380,96h-.41a22,22,0,0,0-22,21.59A133.61,133.61,0,0,0,366.09,167L278,217.89V116.18a133.5,133.5,0,0,0,47.07-17.33,22,22,0,0,0-22.71-37.69A89.56,89.56,0,0,1,278,71.27V38a22,22,0,0,0-44,0V71.27a89.56,89.56,0,0,1-24.36-10.11,22,22,0,1,0-22.71,37.69A133.5,133.5,0,0,0,234,116.18V217.89L145.91,167a133.61,133.61,0,0,0,8.52-49.43,22,22,0,0,0-22-21.59H132a22,22,0,0,0-21.59,22.41A89.74,89.74,0,0,1,107,144.58L73,125a22,22,0,1,0-22,38.1l34,19.64a89.74,89.74,0,0,1-20.94,16,22,22,0,1,0,21.28,38.51,133.62,133.62,0,0,0,38.55-32.1L212,256l-88.09,50.86a133.62,133.62,0,0,0-38.55-32.1,22,22,0,1,0-21.28,38.51,89.74,89.74,0,0,1,20.94,16L51,349a22,22,0,1,0,22,38.1l34-19.63a89.74,89.74,0,0,1,3.42,26.15A22,22,0,0,0,132,416h.41a22,22,0,0,0,22-21.59A133.61,133.61,0,0,0,145.91,345L234,294.11V395.82a133.5,133.5,0,0,0-47.07,17.33,22,22,0,1,0,22.71,37.69A89.56,89.56,0,0,1,234,440.73V474a22,22,0,0,0,44,0V440.73a89.56,89.56,0,0,1,24.36,10.11,22,22,0,0,0,22.71-37.69A133.5,133.5,0,0,0,278,395.82V294.11L366.09,345a133.61,133.61,0,0,0-8.52,49.43,22,22,0,0,0,22,21.59H380a22,22,0,0,0,21.59-22.41A89.74,89.74,0,0,1,405,367.42l34,19.63A22,22,0,1,0,461,349Z'/></svg>",
		'contact': "<svg xmlns='http://www.w3.org/2000/svg' width='100px' height='100px' viewBox='0 0 100 100' xml:space='preserve'><g>	<path d='M74,29H26c-3.3,0-6,2.7-6,6v29c0,3.3,2.7,6,6,6h48c3.3,0,6-2.7,6-6V35C80,31.7,77.3,29,74,29z M48.6,63H40		h-8.6c-1.9,0-3.4-2.1-3.4-4.1c0.1-3,3.2-4.8,6.5-6.3c2.3-1,2.6-1.9,2.6-2.9c0-1-0.6-1.9-1.4-2.6c-1.3-1.2-2.1-3-2.1-5		c0-3.8,2.3-7,6.3-7s6.3,3.2,6.3,7c0,2-0.7,3.8-2.1,5c-0.8,0.7-1.4,1.6-1.4,2.6c0,1,0.3,1.9,2.6,2.8c3.3,1.4,6.4,3.4,6.5,6.4		C52,60.9,50.5,63,48.6,63z M72,56c0,1.1-0.9,2-2,2h-9c-1.1,0-2-0.9-2-2v-3c0-1.1,0.9-2,2-2h9c1.1,0,2,0.9,2,2V56z M72,45		c0,1.1-0.9,2-2,2H55c-1.1,0-2-0.9-2-2v-3c0-1.1,0.9-2,2-2h15c1.1,0,2,0.9,2,2V45z'/></g></svg>",
		'datetime': "<svg width='48px' height='48px' viewBox='0 0 48 48' fill='none' xmlns='http://www.w3.org/2000/svg'><path d='M12 5C12 4.44772 12.4477 4 13 4C13.5523 4 14 4.44772 14 5V11C14 11.5523 13.5523 12 13 12C12.4477 12 12 11.5523 12 11V5Z' fill='#333333'/><path d='M28 5C28 4.44772 28.4477 4 29 4C29.5523 4 30 4.44772 30 5V11C30 11.5523 29.5523 12 29 12C28.4477 12 28 11.5523 28 11V5Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M14 23H12L12 25H14V23ZM12 21C10.8954 21 10 21.8954 10 23V25C10 26.1046 10.8954 27 12 27H14C15.1046 27 16 26.1046 16 25V23C16 21.8954 15.1046 21 14 21H12Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M22 23H20L20 25H22V23ZM20 21C18.8954 21 18 21.8954 18 23V25C18 26.1046 18.8954 27 20 27H22C23.1046 27 24 26.1046 24 25V23C24 21.8954 23.1046 21 22 21H20Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M30 23H28L28 25H30V23ZM28 21C26.8954 21 26 21.8954 26 23V25C26 26.1046 26.8954 27 28 27H30C31.1046 27 32 26.1046 32 25V23C32 21.8954 31.1046 21 30 21H28Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M14 31H12L12 33H14V31ZM12 29C10.8954 29 10 29.8954 10 31V33C10 34.1046 10.8954 35 12 35H14C15.1046 35 16 34.1046 16 33V31C16 29.8954 15.1046 29 14 29H12Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M22 31H20L20 33H22V31ZM20 29C18.8954 29 18 29.8954 18 31V33C18 34.1046 18.8954 35 20 35H22C23.1046 35 24 34.1046 24 33V31C24 29.8954 23.1046 29 22 29H20Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M9 10H33C34.1046 10 35 10.8954 35 12V28C35.6906 28 36.3608 28.0875 37 28.252V12C37 9.79086 35.2091 8 33 8H9C6.79086 8 5 9.79086 5 12V36C5 38.2091 6.79086 40 9 40H28.0703C27.7122 39.381 27.4347 38.7095 27.252 38H9C7.89543 38 7 37.1046 7 36V12C7 10.8954 7.89543 10 9 10Z' fill='#333333'/><path d='M5 12C5 9.79086 6.79086 8 9 8H33C35.2091 8 37 9.79086 37 12V19H5V12Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M33 10H9C7.89543 10 7 10.8954 7 12V17H35V12C35 10.8954 34.1046 10 33 10ZM9 8C6.79086 8 5 9.79086 5 12V19H37V12C37 9.79086 35.2091 8 33 8H9Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M36 19H6V17H36V19Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M35 42C38.3137 42 41 39.3137 41 36C41 32.6863 38.3137 30 35 30C31.6863 30 29 32.6863 29 36C29 39.3137 31.6863 42 35 42ZM35 44C39.4183 44 43 40.4183 43 36C43 31.5817 39.4183 28 35 28C30.5817 28 27 31.5817 27 36C27 40.4183 30.5817 44 35 44Z' fill='#333333'/><path fill-rule='evenodd' clip-rule='evenodd' d='M35 31.1787C35.5523 31.1787 36 31.6264 36 32.1787V36.4768L38.4515 38.1785C38.9052 38.4934 39.0177 39.1165 38.7027 39.5702C38.3878 40.0239 37.7647 40.1364 37.311 39.8215L34 37.5232V32.1787C34 31.6264 34.4477 31.1787 35 31.1787Z' fill='#333333'/></svg>",
		'delete': "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px'	 width='48px' height='48px' viewBox='0 0 348.333 348.334' xml:space='preserve'><g>	<path d='M336.559,68.611L231.016,174.165l105.543,105.549c15.699,15.705,15.699,41.145,0,56.85		c-7.844,7.844-18.128,11.769-28.407,11.769c-10.296,0-20.581-3.919-28.419-11.769L174.167,231.003L68.609,336.563		c-7.843,7.844-18.128,11.769-28.416,11.769c-10.285,0-20.563-3.919-28.413-11.769c-15.699-15.698-15.699-41.139,0-56.85		l105.54-105.549L11.774,68.611c-15.699-15.699-15.699-41.145,0-56.844c15.696-15.687,41.127-15.687,56.829,0l105.563,105.554		L279.721,11.767c15.705-15.687,41.139-15.687,56.832,0C352.258,27.466,352.258,52.912,336.559,68.611z'/></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>",
		'list-ordered': "<svg viewBox='0 0 18 18'> <line class='ql-stroke' x1='7' x2='15' y1='4' y2='4'></line> <line class='ql-stroke' x1='7' x2='15' y1='9' y2='9'></line> <line class='ql-stroke' x1='7' x2='15' y1='14' y2='14'></line> <line class='ql-stroke ql-thin' x1='2.5' x2='4.5' y1='5.5' y2='5.5'></line> <path class='ql-fill' d='M3.5,6A0.5,0.5,0,0,1,3,5.5V3.085l-0.276.138A0.5,0.5,0,0,1,2.053,3c-0.124-.247-0.023-0.324.224-0.447l1-.5A0.5,0.5,0,0,1,4,2.5v3A0.5,0.5,0,0,1,3.5,6Z'></path> <path class='ql-stroke ql-thin' d='M4.5,10.5h-2c0-.234,1.85-1.076,1.85-2.234A0.959,0.959,0,0,0,2.5,8.156'></path> <path class='ql-stroke ql-thin' d='M2.5,14.846a0.959,0.959,0,0,0,1.85-.109A0.7,0.7,0,0,0,3.75,14a0.688,0.688,0,0,0,.6-0.736,0.959,0.959,0,0,0-1.85-.109'></path> </svg>",
		'list-bullet': `<svg viewBox="0 0 18 18"> <line class="ql-stroke" x1="6" x2="15" y1="4" y2="4"></line> <line class="ql-stroke" x1="6" x2="15" y1="9" y2="9"></line> <line class="ql-stroke" x1="6" x2="15" y1="14" y2="14"></line> <line class="ql-stroke" x1="3" x2="3" y1="4" y2="4"></line> <line class="ql-stroke" x1="3" x2="3" y1="9" y2="9"></line> <line class="ql-stroke" x1="3" x2="3" y1="14" y2="14"></line> </svg>`,
		'list-checkable': "<svg viewBox='0 0 18 18'> <line class='ql-stroke' x1='9' x2='15' y1='4' y2='4'></line> <polyline class='ql-stroke' points='3 4 4 5 6 3'></polyline> <line class='ql-stroke' x1='9' x2='15' y1='14' y2='14'></line> <polyline class='ql-stroke' points='3 14 4 15 6 13'></polyline> <line class='ql-stroke' x1='9' x2='15' y1='9' y2='9'></line> <polyline class='ql-stroke' points='3 9 4 10 6 8'></polyline> </svg>",
		'insert-outline': `<svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 512 512" style="enable-background:new 0 0 512 512;" xml:space="preserve"><g><g><path d="M490.667,0H21.333C9.552,0,0,9.551,0,21.333v469.333C0,502.449,9.552,512,21.333,512h469.333			c11.782,0,21.333-9.551,21.333-21.333V21.333C512,9.551,502.45,0,490.667,0z M469.334,469.333L469.334,469.333H42.667V42.667			h426.667V469.333z"/></g></g><g><g><path d="M405.334,362.667H106.667c-11.782,0-21.333,9.551-21.333,21.333c-0.001,11.782,9.551,21.333,21.333,21.333h298.667			c11.782,0,21.333-9.551,21.333-21.333C426.667,372.218,417.117,362.667,405.334,362.667z"/></g></g><g><g><path d="M277.335,277.334H106.667c-11.782,0-21.333,9.551-21.333,21.333C85.333,310.449,94.885,320,106.667,320h170.667			c11.782,0,21.333-9.551,21.333-21.333C298.668,286.885,289.117,277.334,277.335,277.334z"/></g></g><g><g><path d="M277.335,192.001H106.667c-11.782,0-21.333,9.551-21.333,21.333c-0.001,11.781,9.551,21.333,21.333,21.333h170.667			c11.782,0,21.333-9.551,21.333-21.333C298.668,201.552,289.117,192.001,277.335,192.001z"/></g></g><g><g><path d="M405.334,106.667H106.667c-11.782,0-21.333,9.551-21.333,21.333c-0.001,11.782,9.551,21.333,21.333,21.333h298.667			c11.782,0,21.333-9.551,21.333-21.333S417.117,106.667,405.334,106.667z"/></g></g><g><g><path d="M392.838,256l27.582-27.582c8.331-8.331,8.331-21.838-0.001-30.17c-8.331-8.331-21.839-8.331-30.17,0l-42.667,42.667			c-8.331,8.331-8.331,21.839,0,30.17l42.667,42.667c8.331,8.331,21.839,8.331,30.17,0c8.331-8.331,8.331-21.839,0-30.17			L392.838,256z"/></g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>`,
		'insert-solid': `<svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px"	 viewBox="0 0 512 512" style="enable-background:new 0 0 512 512;" xml:space="preserve"><g>	<g>		<path d="M490.667,0H21.333C9.536,0,0,9.557,0,21.333v469.333C0,502.443,9.536,512,21.333,512h469.333			c11.797,0,21.333-9.557,21.333-21.333V21.333C512,9.557,502.464,0,490.667,0z M106.667,106.667h298.667			c11.797,0,21.333,9.557,21.333,21.333s-9.536,21.333-21.333,21.333H106.667c-11.797,0-21.333-9.557-21.333-21.333			S94.869,106.667,106.667,106.667z M106.667,192h170.667c11.797,0,21.333,9.557,21.333,21.333s-9.536,21.333-21.333,21.333H106.667			c-11.797,0-21.333-9.557-21.333-21.333S94.869,192,106.667,192z M106.667,277.333h170.667c11.797,0,21.333,9.557,21.333,21.333			S289.131,320,277.333,320H106.667c-11.797,0-21.333-9.557-21.333-21.333S94.869,277.333,106.667,277.333z M405.333,405.333			H106.667c-11.797,0-21.333-9.557-21.333-21.333s9.536-21.333,21.333-21.333h298.667c11.797,0,21.333,9.557,21.333,21.333			S417.131,405.333,405.333,405.333z M420.416,283.584c8.341,8.341,8.341,21.824,0,30.165c-4.16,4.16-9.621,6.251-15.083,6.251			c-5.461,0-10.923-2.091-15.083-6.251l-42.667-42.667c-8.341-8.341-8.341-21.824,0-30.165l42.667-42.667			c8.341-8.341,21.824-8.341,30.165,0s8.341,21.824,0,30.165L392.832,256L420.416,283.584z"/>	</g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>`,
		'append-outline': `<svg width="20px" height="20px" viewBox="0 0 20 20" version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink">    <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">        <g fill="#212121" fill-rule="nonzero">            <path d="M14,12 C15.0543909,12 15.9181678,12.81585 15.9945144,13.8507339 L16,14 L16,16 C16,17.0543909 15.18415,17.9181678 14.1492661,17.9945144 L14,18 L6,18 C4.94563773,18 4.08183483,17.18415 4.00548573,16.1492661 L4,16 L4,14 C4,12.9456091 4.81587733,12.0818322 5.85073759,12.0054856 L6,12 L14,12 Z M14,13 L6,13 C5.48716857,13 5.06449347,13.386027 5.0067278,13.8833761 L5,14 L5,16 C5,16.51285 5.38604429,16.9355092 5.88337975,16.9932725 L6,17 L14,17 C14.51285,17 14.9355092,16.613973 14.9932725,16.1166239 L15,16 L15,14 C15,13.48715 14.613973,13.0644908 14.1166239,13.0067275 L14,13 Z M17.5,9.5 C17.7761,9.5 18,9.72386 18,10 C18,10.2761 17.7761,10.5 17.5,10.5 L2.5,10.5 C2.22386,10.5 2,10.2761 2,10 C2,9.72386 2.22386,9.5 2.5,9.5 L17.5,9.5 Z M14,2 C15.1046,2 16,2.89543 16,4 L16,6 C16,7.10457 15.1046,8 14,8 L6,8 C4.89543,8 4,7.10457 4,6 L4,4 C4,2.89543 4.89543,2 6,2 L14,2 Z M14,3 L6,3 C5.44772,3 5,3.44772 5,4 L5,6 C5,6.55228 5.44772,7 6,7 L14,7 C14.5523,7 15,6.55228 15,6 L15,4 C15,3.44772 14.5523,3 14,3 Z"></path>        </g>    </g></svg>`,
		'append-solid': `<svg width="20px"     height="20px"     viewBox="0 0 20 20"     version="1.1"     xmlns="http://www.w3.org/2000/svg"     xmlns:xlink="http://www.w3.org/1999/xlink">	<g stroke="none"	   stroke-width="1"	   fill="none"	   fill-rule="evenodd">		<g fill="#212121"		   fill-rule="nonzero">			<path d="M14,12 C15.0543909,12 15.9181678,12.81585 15.9945144,13.8507339 L16,14 L16,16 C16,17.0543909 15.18415,17.9181678 14.1492661,17.9945144 L14,18 L6,18 C4.94563773,18 4.08183483,17.18415 4.00548573,16.1492661 L4,16 L4,14 C4,12.9456091 4.81587733,12.0818322 5.85073759,12.0054856 L6,12 L14,12 Z M17.5,9.5 C17.7761,9.5 18,9.72386 18,10 C18,10.2454222 17.8230914,10.4496 17.5898645,10.4919429 L17.5,10.5 L2.5,10.5 C2.22386,10.5 2,10.2761 2,10 C2,9.75454222 2.17687704,9.5503921 2.41012499,9.50805575 L2.5,9.5 L17.5,9.5 Z M14,2 C15.1046,2 16,2.89543 16,4 L16,6 C16,7.10457 15.1046,8 14,8 L6,8 C4.89543,8 4,7.10457 4,6 L4,4 C4,2.89543 4.89543,2 6,2 L14,2 Z"</path>		</g>	</g></svg>`,
		'text-insert-caret-outline': `<svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px"	 viewBox="0 0 512.002 512.002" style="enable-background:new 0 0 512.002 512.002;" xml:space="preserve"><g>	<g>		<path d="M309.334,266.667c5.867,0,10.667-4.8,10.667-10.667c0-5.867-4.8-10.667-10.667-10.667h-42.667V66.774			c0-25.493,18.773-45.44,42.667-45.44c5.867,0,10.667-4.8,10.667-10.667c0-5.867-4.8-10.667-10.667-10.667			c-21.653-0.107-41.813,11.093-53.333,29.333c-11.52-18.24-31.68-29.44-53.333-29.333c-5.867,0-10.667,4.8-10.667,10.667			c0,5.867,4.8,10.667,10.667,10.667c23.893,0,42.667,19.947,42.667,45.44v178.56h-42.667c-5.867,0-10.667,4.8-10.667,10.667			c0,5.867,4.8,10.667,10.667,10.667h42.667v180.16c0,24.96-18.347,43.84-42.667,43.84c-5.867,0-10.667,4.8-10.667,10.667			c0,5.867,4.8,10.667,10.667,10.667c21.547,0.107,41.6-10.667,53.333-28.693c11.733,18.027,31.787,28.8,53.333,28.693			c5.867,0,10.667-4.8,10.667-10.667c0-5.867-4.8-10.667-10.667-10.667c-24.32,0-42.667-18.88-42.667-43.84v-180.16H309.334z"/>	</g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>`,
		'text-insert-caret-solid': `<svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px"	 viewBox="0 0 512.002 512.002" style="enable-background:new 0 0 512.002 512.002;" xml:space="preserve" style="background-color: black;"><g>	<g>		<path style="fill: rgb(255,255,255)" d="M309.334,266.667c5.867,0,10.667-4.8,10.667-10.667c0-5.867-4.8-10.667-10.667-10.667h-42.667V66.774			c0-25.493,18.773-45.44,42.667-45.44c5.867,0,10.667-4.8,10.667-10.667c0-5.867-4.8-10.667-10.667-10.667			c-21.653-0.107-41.813,11.093-53.333,29.333c-11.52-18.24-31.68-29.44-53.333-29.333c-5.867,0-10.667,4.8-10.667,10.667			c0,5.867,4.8,10.667,10.667,10.667c23.893,0,42.667,19.947,42.667,45.44v178.56h-42.667c-5.867,0-10.667,4.8-10.667,10.667			c0,5.867,4.8,10.667,10.667,10.667h42.667v180.16c0,24.96-18.347,43.84-42.667,43.84c-5.867,0-10.667,4.8-10.667,10.667			c0,5.867,4.8,10.667,10.667,10.667c21.547,0.107,41.6-10.667,53.333-28.693c11.733,18.027,31.787,28.8,53.333,28.693			c5.867,0,10.667-4.8,10.667-10.667c0-5.867-4.8-10.667-10.667-10.667c-24.32,0-42.667-18.88-42.667-43.84v-180.16H309.334z"/>	</g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g><g></g></svg>`,
		'radio-off': `<svg width="24px"     height="24px"     viewBox="0 0 24 24"     xmlns="http://www.w3.org/2000/svg">	<g data-name="Layer 2">		<g data-name="radio-button-off">			<rect width="24"			      height="24"			      opacity="0"/>			<path d="M12 22a10 10 0 1 1 10-10 10 10 0 0 1-10 10zm0-18a8 8 0 1 0 8 8 8 8 0 0 0-8-8z"/>		</g>	</g></svg>`,
		'radio-on': `<svg width="24px"     height="24px"     viewBox="0 0 24 24"     xmlns="http://www.w3.org/2000/svg">	<g data-name="Layer 2">		<g data-name="radio-button-on">			<rect width="24"			      height="24"			      opacity="0"/>			<path d="M12 2a10 10 0 1 0 10 10A10 10 0 0 0 12 2zm0 18a8 8 0 1 1 8-8 8 8 0 0 1-8 8z"/>			<path d="M12 7a5 5 0 1 0 5 5 5 5 0 0 0-5-5z"/>		</g>	</g></svg>`,
		'align-justify': `<svg width="24px"     height="24px"     viewBox="0 0 24 24"     xmlns="http://www.w3.org/2000/svg"     fill="none"     stroke="currentColor"     stroke-width="2"     stroke-linecap="round"     stroke-linejoin="round">	<line x1="21"	      y1="10"	      x2="3"	      y2="10"></line>	<line x1="21"	      y1="6"	      x2="3"	      y2="6"></line>	<line x1="21"	      y1="14"	      x2="3"	      y2="14"></line>	<line x1="21"	      y1="18"	      x2="3"	      y2="18"></line></svg>`,
		'align-left': `<svg width="24px"     height="24px"     viewBox="0 0 24 24"     xmlns="http://www.w3.org/2000/svg"     enable-background="new 0 0 24 24">	<path d="M3,7h18c0.6,0,1-0.4,1-1s-0.4-1-1-1H3C2.4,5,2,5.4,2,6S2.4,7,3,7z M3,11h14c0.6,0,1-0.4,1-1s-0.4-1-1-1H3c-0.6,0-1,0.4-1,1S2.4,11,3,11z M21,13H3c-0.6,0-1,0.4-1,1s0.4,1,1,1h18c0.6,0,1-0.4,1-1S21.6,13,21,13z M17,17H3c-0.6,0-1,0.4-1,1s0.4,1,1,1h14c0.6,0,1-0.4,1-1S17.6,17,17,17z"/></svg>`,
		'align-right': `<svg width="24px"     height="24px"     viewBox="0 0 24 24"     xmlns="http://www.w3.org/2000/svg"     enable-background="new 0 0 24 24">	<path d="M3,7h18c0.6,0,1-0.4,1-1s-0.4-1-1-1H3C2.4,5,2,5.4,2,6S2.4,7,3,7z M21,9H7c-0.6,0-1,0.4-1,1s0.4,1,1,1h14c0.6,0,1-0.4,1-1S21.6,9,21,9z M21,13H3c-0.6,0-1,0.4-1,1s0.4,1,1,1h18c0.6,0,1-0.4,1-1S21.6,13,21,13z M21,17H7c-0.6,0-1,0.4-1,1s0.4,1,1,1h14c0.6,0,1-0.4,1-1S21.6,17,21,17z"/></svg>`,
		'align-center': `<svg width="24px"     height="24px"     viewBox="0 0 24 24"     xmlns="http://www.w3.org/2000/svg"     enable-background="new 0 0 24 24">	<path d="M7,9c-0.6,0-1,0.4-1,1s0.4,1,1,1h10c0.6,0,1-0.4,1-1s-0.4-1-1-1H7z M3,7h18c0.6,0,1-0.4,1-1s-0.4-1-1-1H3C2.4,5,2,5.4,2,6S2.4,7,3,7z M17,17H7c-0.6,0-1,0.4-1,1s0.4,1,1,1h10c0.6,0,1-0.4,1-1S17.6,17,17,17z M21,13H3c-0.6,0-1,0.4-1,1s0.4,1,1,1h18c0.6,0,1-0.4,1-1S21.6,13,21,13z"/></svg>`,
		'arrow-left': `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" data-fa-i2svg=""><path fill="currentColor" d="M447.1 256C447.1 273.7 433.7 288 416 288H109.3l105.4 105.4c12.5 12.5 12.5 32.75 0 45.25C208.4 444.9 200.2 448 192 448s-16.38-3.125-22.62-9.375l-160-160c-12.5-12.5-12.5-32.75 0-45.25l160-160c12.5-12.5 32.75-12.5 45.25 0s12.5 32.75 0 45.25L109.3 224H416C433.7 224 447.1 238.3 447.1 256z"></path></svg>`,
		'arrow-right': `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" data-fa-i2svg=""><path fill="currentColor" d="M438.6 278.6l-160 160C272.4 444.9 264.2 448 256 448s-16.38-3.125-22.62-9.375c-12.5-12.5-12.5-32.75 0-45.25L338.8 288H32C14.33 288 .0016 273.7 .0016 256S14.33 224 32 224h306.8l-105.4-105.4c-12.5-12.5-12.5-32.75 0-45.25s32.75-12.5 45.25 0l160 160C451.1 245.9 451.1 266.1 438.6 278.6z"></path></svg>`,
		'edit-template': `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" data-fa-i2svg=""><path fill="currentColor" d="M490.3 40.4C512.2 62.27 512.2 97.73 490.3 119.6L460.3 149.7L362.3 51.72L392.4 21.66C414.3-.2135 449.7-.2135 471.6 21.66L490.3 40.4zM172.4 241.7L339.7 74.34L437.7 172.3L270.3 339.6C264.2 345.8 256.7 350.4 248.4 353.2L159.6 382.8C150.1 385.6 141.5 383.4 135 376.1C128.6 370.5 126.4 361 129.2 352.4L158.8 263.6C161.6 255.3 166.2 247.8 172.4 241.7V241.7zM192 63.1C209.7 63.1 224 78.33 224 95.1C224 113.7 209.7 127.1 192 127.1H96C78.33 127.1 64 142.3 64 159.1V416C64 433.7 78.33 448 96 448H352C369.7 448 384 433.7 384 416V319.1C384 302.3 398.3 287.1 416 287.1C433.7 287.1 448 302.3 448 319.1V416C448 469 405 512 352 512H96C42.98 512 0 469 0 416V159.1C0 106.1 42.98 63.1 96 63.1H192z"></path></svg>`,
		'paste': `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" data-fa-i2svg=""><path fill="currentColor" d="M320 96V80C320 53.49 298.5 32 272 32H215.4C204.3 12.89 183.6 0 160 0S115.7 12.89 104.6 32H48C21.49 32 0 53.49 0 80v320C0 426.5 21.49 448 48 448l144 .0013L192 176C192 131.8 227.8 96 272 96H320zM160 88C146.8 88 136 77.25 136 64S146.8 40 160 40S184 50.75 184 64S173.3 88 160 88zM416 128v96h96L416 128zM384 224L384 128h-112C245.5 128 224 149.5 224 176v288c0 26.51 21.49 48 48 48h192c26.51 0 48-21.49 48-48V256h-95.99C398.4 256 384 241.6 384 224z"></path></svg>`,
		'plus': `<svg class="svg-inline--fa fa-plus option-icon" style="color: lime;" aria-hidden="true" focusable="false" data-prefix="fas" data-icon="plus" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" data-fa-i2svg=""><path fill="currentColor" d="M432 256c0 17.69-14.33 32.01-32 32.01H256v144c0 17.69-14.33 31.99-32 31.99s-32-14.3-32-31.99v-144H48c-17.67 0-32-14.32-32-32.01s14.33-31.99 32-31.99H192v-144c0-17.69 14.33-32.01 32-32.01s32 14.32 32 32.01v144h144C417.7 224 432 238.3 432 256z"></path></svg>`,
		'spinner': `<svg class="svg-inline--fa fa-spinner" aria-hidden="true" focusable="false" data-prefix="fas" data-icon="spinner" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" data-fa-i2svg=""><path fill="currentColor" d="M304 48C304 74.51 282.5 96 256 96C229.5 96 208 74.51 208 48C208 21.49 229.5 0 256 0C282.5 0 304 21.49 304 48zM304 464C304 490.5 282.5 512 256 512C229.5 512 208 490.5 208 464C208 437.5 229.5 416 256 416C282.5 416 304 437.5 304 464zM0 256C0 229.5 21.49 208 48 208C74.51 208 96 229.5 96 256C96 282.5 74.51 304 48 304C21.49 304 0 282.5 0 256zM512 256C512 282.5 490.5 304 464 304C437.5 304 416 282.5 416 256C416 229.5 437.5 208 464 208C490.5 208 512 229.5 512 256zM74.98 437C56.23 418.3 56.23 387.9 74.98 369.1C93.73 350.4 124.1 350.4 142.9 369.1C161.6 387.9 161.6 418.3 142.9 437C124.1 455.8 93.73 455.8 74.98 437V437zM142.9 142.9C124.1 161.6 93.73 161.6 74.98 142.9C56.24 124.1 56.24 93.73 74.98 74.98C93.73 56.23 124.1 56.23 142.9 74.98C161.6 93.73 161.6 124.1 142.9 142.9zM369.1 369.1C387.9 350.4 418.3 350.4 437 369.1C455.8 387.9 455.8 418.3 437 437C418.3 455.8 387.9 455.8 369.1 437C350.4 418.3 350.4 387.9 369.1 369.1V369.1z"></path></svg>`,
		'link': `<svg viewBox="0 0 18 18"> <line x1="7" x2="11" y1="7" y2="11"></line> <path class="ql-stroke" d="M8.9,4.577a3.476,3.476,0,0,1,.36,4.679A3.476,3.476,0,0,1,4.577,8.9C3.185,7.5,2.035,6.4,4.217,4.217S7.5,3.185,8.9,4.577Z"></path> <path class="ql-stroke" d="M13.423,9.1a3.476,3.476,0,0,0-4.679-.36,3.476,3.476,0,0,0,.36,4.679c1.392,1.392,2.5,2.542,4.679.36S14.815,10.5,13.423,9.1Z"></path> </svg>`,
		'play': `<svg fill="#000000"     viewBox="0 0 32 32"     version="1.1"     xmlns="http://www.w3.org/2000/svg"> <g stroke-width="0"></g> <g stroke-linecap="round"    stroke-linejoin="round"></g> <g>  <path d="M5.92 24.096q0 1.088 0.928 1.728 0.512 0.288 1.088 0.288 0.448 0 0.896-0.224l16.16-8.064q0.48-0.256 0.8-0.736t0.288-1.088-0.288-1.056-0.8-0.736l-16.16-8.064q-0.448-0.224-0.896-0.224-0.544 0-1.088 0.288-0.928 0.608-0.928 1.728v16.16z"></path> </g></svg>`,
		'stop': `<svg fill="#000000" viewBox="0 0 32 32" version="1.1" xmlns="http://www.w3.org/2000/svg"><g stroke-width="0"></g><g stroke-linecap="round" stroke-linejoin="round"></g><g><path d="M5.92 24.096q0 0.832 0.576 1.408t1.44 0.608h16.128q0.832 0 1.44-0.608t0.576-1.408v-16.16q0-0.832-0.576-1.44t-1.44-0.576h-16.128q-0.832 0-1.44 0.576t-0.576 1.44v16.16z"></path></g></svg>`,
		'pause': `<svg fill="#000000"      viewBox="0 0 32 32"      version="1.1"      xmlns="http://www.w3.org/2000/svg"> 	<g stroke-width="0"></g> 	<g stroke-linecap="round" 	   stroke-linejoin="round"></g> 	<g> 		<path d="M5.92 24.096q0 0.832 0.576 1.408t1.44 0.608h4.032q0.832 0 1.44-0.608t0.576-1.408v-16.16q0-0.832-0.576-1.44t-1.44-0.576h-4.032q-0.832 0-1.44 0.576t-0.576 1.44v16.16zM18.016 24.096q0 0.832 0.608 1.408t1.408 0.608h4.032q0.832 0 1.44-0.608t0.576-1.408v-16.16q0-0.832-0.576-1.44t-1.44-0.576h-4.032q-0.832 0-1.408 0.576t-0.608 1.44v16.16z"></path> 	</g> </svg>`,
		'sign-out': `<svg fill="#000000" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><g stroke-width="0"></g><g stroke-linecap="round" stroke-linejoin="round"></g><g><polyline points="6 15 3 12 6 9" style="fill: none; stroke: #000000; stroke-linecap: round; stroke-linejoin: round; stroke-width: 2;"></polyline><line x1="3" y1="12" x2="17" y2="12" style="fill: none; stroke: #000000; stroke-linecap: round; stroke-linejoin: round; stroke-width: 2;"></line><path d="M10,8V5a1,1,0,0,1,1-1h9a1,1,0,0,1,1,1V19a1,1,0,0,1-1,1H11a1,1,0,0,1-1-1V16" style="fill: none; stroke: #000000; stroke-linecap: round; stroke-linejoin: round; stroke-width: 2;"></path></g></svg>`,
		'check': `<svg viewBox="0 0 24 24"      fill="none"      xmlns="http://www.w3.org/2000/svg"> 	<g stroke-width="0"></g> 	<g stroke-linecap="round" 	   stroke-linejoin="round"></g> 	<g> 		<g> 			<path d="M4 12L8.94975 16.9497L19.5572 6.34326" 			      stroke="#3bfd21" 			      stroke-width="2" 			      stroke-linecap="round" 			      stroke-linejoin="round"></path> 		</g> 	</g> </svg>`
	}
	// #endregion 
}

function initGlobals() {
	globals.ENCODED_TEMPLATE_REGEXP = new RegExp(globals.ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + globals.ENCODED_TEMPLATE_CLOSE_TOKEN, "");
	globals.ENCODED_LIST_ITEM_REGEXP = new RegExp(globals.ENCODED_LIST_ITEM_OPEN_TOKEN + ".*?" + globals.ENCODED_LIST_ITEM_CLOSE_TOKEN, "");

	globals.MinFileListOptIdx = globals.AppendPreIdx;

	globals.TemplateDateTimeFormatOptionLabels = [
		'yyyy-MM-dd',
		'MM-dd-yyyy',
		'dd MMMM yyyy',
		'E, MMM d, yy',
		'HH:mm:ss',
		'h:mm a',
		'MM/dd/yyyy HH:mm:ss',
		'E, dd MMM yyyy HH:mm:ss',
		globals.CUSTOM_TEMPLATE_LABEL_VAL
	];
}