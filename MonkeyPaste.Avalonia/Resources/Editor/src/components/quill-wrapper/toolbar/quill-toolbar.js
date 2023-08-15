// #region Globals

// #endregion Globals

// #region Life Cycle

function initEditorToolbarQuillOptions(quillOptions, toolbarId) {
	initFontFamilySelector();
	let sizes = registerFontSizes();
	quillOptions.modules = {
		toolbar: toolbarId
	};

	//if (UseBetterTable) {
		if (quillBetterTable === undefined || Quill === undefined) {
			/// host load error case
			debugger;
		}

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
					colors: globals.ContentColors,//["green", "red", "yellow", "blue", "white"],
					text: "Background Color:"
				}
			}
		};
		quillOptions.modules.keyboard = {
			bindings: quillBetterTable.keyboardBindings
		};
	//} else if (UseQuill2) {
	//	quillOptions.modules.table = true;
	//}

	return quillOptions;
}

function initEditorToolbar() {
	// called after options are returned and quill creates toolbar

	getEditorToolbarElement().classList.add('hidden');
	getEditorToolbarElement().classList.add('top-align');

	initTable();
	initLists();
	initLinks();
	initBold();
	initItalic();
	initAlignEditorToolbarButton();
	initFontColorToolbarItems();

	initFontFamilyPicker();
	initFontSizes();
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

function getThemableToolbarSvgElements() {
	return [
		document.getElementById('createTemplateToolbarButton').firstChild,
		document.getElementById('alignEditorToolbarButton').firstChild,
		document.getElementById('createTableToolbarButtonLabel').firstChild,
		document.getElementById('findReplaceToolbarButton').firstChild
	];
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

	getThemableToolbarSvgElements().forEach(x => applyShapeStyles(x));
	updateAllElements();
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers






