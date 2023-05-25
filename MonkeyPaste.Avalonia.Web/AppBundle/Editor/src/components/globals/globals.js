const globals = {
	// MAIN
	IsLoaded: false,
	DomParser: new DOMParser(),
	DomSerializer: new XMLSerializer(),
	IsDebug: true,
	IsTesting: false,
	IsSpellCheckEnabled: false,
	MaxUndoLimit: -1,

	// ENV
	WindowsEnv: 'Windows',
	IosEnv: 'Ios',
	AndroidEnv: 'Android',
	MacEnv: 'Mac',
	LinuxEnv: 'Linux',
	WebEnv: 'Web',
	UknownEnv: 'Unknown',
	EnvName: "",

	// EDITOR
	DefaultEditorWidth: 1200,
	IgnoreNextSelectionChange: false,
	SuppressTextChangedNtf: false,
	EditorTheme: 'light',

	// CONTENT
	CONTENT_CLASS_PREFIX: 'content',
	ContentClassAttrb: null,
	ContentHandle: null,
	ContentItemType: 'Text',
	InlineTags: ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'],
	BlockTags: ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote', 'pre'],

	// TEMPLATES
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
		 },*/
		{
			label: 'Contact',
			icon: 'contact'
		},
		{
			label: 'DateTime',
			icon: 'datetime'
		}
	],
	// TEMPLATE CONTACTS

	ContactFieldTypes: [
		'FirstName',
		'LastName',
		'FullName',
		'PhoneNumber',
		'Address',
		'Email',
	],
	AvailableContacts: [],
}

function initGlobals() {
	globals.ENCODED_TEMPLATE_REGEXP = new RegExp(globals.ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + globals.ENCODED_TEMPLATE_CLOSE_TOKEN, "");
}