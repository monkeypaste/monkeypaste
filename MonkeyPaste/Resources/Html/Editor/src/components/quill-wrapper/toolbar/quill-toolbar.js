
var UseBetterTable = true;

function addToolbarToQuillOptions(useBetterTable, quillOptions) {
	UseBetterTable = useBetterTable;

	var node = document.createElement("style");
	node.innerHTML = registerFontStyles(EnvName);
	document.body.appendChild(node);

	let fonts = registerFontFamilys();
	let sizes = registerFontSizes();
	//container.unshift([{ size: sizes }]);
	let toolbar = {
		container: [
			//[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
			[{ size: sizes }], // font sizes
			[{ font: fonts.whitelist }],
			["bold", "italic", "underline", "strike"], // toggled buttons
			["blockquote", "code-block"],

			// [{ 'header': 1 }, { 'header': 2 }],               // custom button values
			[{ list: "ordered" }, { list: "bullet" }, { list: "check" }],
			[{ script: "sub" }, { script: "super" }], // superscript/subscript
			[{ indent: "-1" }, { indent: "+1" }], // outdent/indent
			[{ direction: "rtl" }], // text direction

			// [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
			["link", "image"],//, "video", "formula"],
			[{ color: [] }, { background: [] }], // dropdown with defaults from theme
			[{ align: [] }],
			// ['clean'],
			[{ "Table-Input": registerTables() }]
		],
		handlers: {
			"Table-Input": () => {
				return;
			}
		}
	};

	//initFonts();
	//toolbar.container = addFontFamiliesToQuillContainerOptions(toolbar.container);
	//toolbar.container = addFontSizesToQuillContainerOptions(toolbar.container);

	quillOptions.modules.toolbar = toolbar;
	if (UseBetterTable) {
		Quill.register({ "modules/better-table": quillBetterTable }, true);

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
	}

	return quillOptions;
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

function getQuillToolbarContainerElement() {
	return document.getElementsByClassName("ql-toolbar")[0];
}


function hideEditorToolbar() {
	getQuillToolbarContainerElement().classList.add("hidden");
	getQuillToolbarContainerElement().style.display = 'none';
	updateAllSizeAndPositions();
}

function showEditorToolbar() {
	getQuillToolbarContainerElement().classList.remove("hidden");
	getQuillToolbarContainerElement().style.display = 'block';
	updateAllSizeAndPositions();
}

function isEditorToolbarVisible() {
	return !getQuillToolbarContainerElement().classList.contains('hidden');
}

function getEditorToolbarWidth() {
	if (!isEditorToolbarVisible()) {
		return 0;
	}
	return getQuillToolbarContainerElement().getBoundingClientRect().width;
}

function getEditorToolbarHeight() {
	if (!isEditorToolbarVisible()) {
		return 0;
	}
	var toolbarHeight = parseInt($(".ql-toolbar").outerHeight());
	return toolbarHeight;
}