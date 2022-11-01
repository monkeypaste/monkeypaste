const InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
const BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote']
const AllDocumentTags = [...InlineTags,...BlockTags];

function loadTextContent(itemDataStr, isPasteRequest) {
	quill.enable(true);
	setRootHtml(itemDataStr);

	loadTemplates(isPasteRequest);
}

function getTextContentData() {
	let qhtml = getHtml();
	return qhtml;
}

function getTextContenLineCount() {
	return getLineCount();
}
function getTextContentCharCount() {
	return getText().length;
}

