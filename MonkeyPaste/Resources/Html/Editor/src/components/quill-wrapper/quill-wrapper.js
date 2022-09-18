var quill;

function initQuill() {
	let quillOptions = createQuillOptions();
	quill = new Quill("#editor", quillOptions);

	registerTemplateBlots();

	initTable();
	initFontFamilyPicker();
	quill.root.setAttribute("spellcheck", "false");

	getEditorContainerElement().firstChild.id = 'quill-editor';
}

function createQuillOptions() {
	// create quill options
	let quillOptions = {
		//debug: true,
		placeholder: "",
		allowReadOnlyEdits: true,
		theme: "snow",
		modules: {
			table: false,
			//htmlEditButton: {
			//	syntax: true
			//}
		}
	}	

	quillOptions = addToolbarToQuillOptions(quillOptions);
	return quillOptions;
}
