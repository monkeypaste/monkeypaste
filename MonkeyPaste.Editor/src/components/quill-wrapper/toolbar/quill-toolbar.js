// #region Globals

// #endregion Globals

// #region Life Cycle

function initEditorToolbarQuillOptions(quillOptions) {
	//initFontFamilySelector();
	initFontFamilySelector();
	let sizes = registerFontSizes();

	//quillOptions.modules.toolbar = {
	//	container: [
	//		//[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
	//		[{ size: sizes }], // font sizes
	//		[{ font: fonts.whitelist }],
	//		["bold", "italic", "underline", "strike"], // toggled buttons
	//		["blockquote", "code-block"],

	//		// [{ 'header': 1 }, { 'header': 2 }],               // custom button values
	//		[{ list: "ordered" }, { list: "bullet" }, { list: "check" }],
	//		[{ script: "sub" }, { script: "super" }], // superscript/subscript
	//		[{ indent: "-1" }, { indent: "+1" }], // outdent/indent
	//		[{ direction: "rtl" }], // text direction

	//		// [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
	//		["link", "image"],//, "video", "formula"],
	//		[{ color: [] }, { background: [] }], // dropdown with defaults from theme
	//		[{ align: [] }],
	//		// ['clean'],
	//		//[{ "Table-Input": registerTables() }]
	//	],
	//	//handlers: {
	//	//	"Table-Input": () => {
	//	//		return;
	//	//	}
	//	//}
	//};

	if (UseBetterTable) {
		Quill.register({ "modules/better-table": quillBetterTable }, true);
		//quillOptions.modules.toolbar.container.push([{ "Table-Input": registerTables() }]);
		//quillOptions.modules.toolbar.handlers = {
		//	"Table-Input": () => {
		//		return;
		//	}
		//};
		quillOptions.modules['better-table'] = {
			operationMenu: {
				items: {
					unmergeCells: {
						text: "Unmerge cells"
					}
				},
				color: {
					colors: ["green", "red", "yellow", "blue", "white"],
					text: "Background Colors:"
				}
			}
		};
		quillOptions.modules.keyboard = {
			bindings: quillBetterTable.keyboardBindings
		};
	} else if (UseQuill2) {
		quillOptions.modules.table = true;
	}

	return quillOptions;
}

function initEditorToolbar() {
	// called after options are returned and quill creates toolbar

	getEditorContainerElement().firstChild.id = 'quill-editor';
	getEditorToolbarElement().classList.add('hidden');
	getEditorToolbarElement().classList.add('top-align');

	initTable();
	initLists();
	initLinks();
	initFontColorToolbarItems();

	initFontFamilyPicker();
	//initLinkToolbarButton();
	initTemplateToolbarButton();
	initFindReplaceToolbar();
}
function initLinkToolbarButton() {
	// workaround because link button does show up for some reason...
	let link_button_elm = document.getElementsByClassName('ql-link')[0];
	function onLinkButtonClick(e) {
		let tooltip_elm = document.getElementsByClassName('ql-tooltip')[0];
		tooltip_elm.setAttribute('data-mode', 'link');
		tooltip_elm.classList.remove('hidden');
		tooltip_elm.classList.add('ql-editing');
	}

	link_button_elm.addEventListener('click', onLinkButtonClick);
}

// #endregion Life Cycle

// #region Getters

function getEditorToolbarElement() {
	return document.getElementsByClassName('ql-toolbar')[0];
}
// #endregion Getters

function getEditorToolbarWidth() {
	if (!isEditorToolbarVisible()) {
		return 0;
	}
	return getEditorToolbarElement().getBoundingClientRect().width;
}

function getEditorToolbarHeight() {
	if (!isEditorToolbarVisible()) {
		return 0;
	}
	let eth = getEditorToolbarElement().getBoundingClientRect().height;
	return eth;
}
// #region Setters

// #endregion Setters

// #region State

// #endregion State

function isEditorToolbarVisible() {
	return !getEditorToolbarElement().classList.contains('hidden');
}
// #region Actions
function hideEditorToolbar() {
	getEditorToolbarElement().classList.add("hidden");
	getEditorToolbarElement().style.display = 'none';
	updateAllElements();
}

function showEditorToolbar() {
	getEditorToolbarElement().classList.remove("hidden");
	getEditorToolbarElement().style.display = 'block';
	updateAllElements();
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers






