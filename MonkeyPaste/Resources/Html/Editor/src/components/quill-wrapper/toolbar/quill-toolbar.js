function addToolbarToQuillOptions(quillOptions) {
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
			["link", "image", "video", "formula"],
			[{ color: [] }, { background: [] }], // dropdown with defaults from theme
			[{ align: [] }],
			// ['clean'],                                         // remove formatting button
			// ['templatebutton'],
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

function getQuillToolbarContainerElement() {
	return document.getElementsByClassName("ql-toolbar")[0];
}


function hideEditorToolbar() {
	getQuillToolbarContainerElement().classList.add("hidden");

	//document.getElementById('editor').previousSibling.style.display = 'none';
	updateAllSizeAndPositions();
}

function showEditorToolbar() {
	getQuillToolbarContainerElement().classList.remove("hidden");
	updateAllSizeAndPositions();
}

function isEditorToolbarVisible() {
	return !getQuillToolbarContainerElement().classList.contains('hidden');
}

function getEditorToolbarWidth() {
	if (isReadOnly()) {
		return 0;
	}
	return getQuillToolbarContainerElement().getBoundingClientRect().width;
}

function getEditorToolbarHeight() {
	if (isReadOnly()) {
		return 0;
	}
	var toolbarHeight = parseInt($(".ql-toolbar").outerHeight());
	return toolbarHeight;
}