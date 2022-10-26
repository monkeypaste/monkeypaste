var InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
var BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote']

function loadTextContent(itemDataStr, isPasteRequest) {
	//try {
		quill.enable(true);
		setRootHtml(itemDataStr);

		loadTemplates(isPasteRequest);
	//} catch (ex) {
	//	//malformed or unsupported content was 
	//	onException_ntf('setHtml', ex);
	//	let item_doc_node = DomParser.parseFromString(itemDataStr, 'text/html');
	//	log('malformed content: ')
	//	log(itemDataStr);
	//	log('using plain text: ');
	//	//let item_pt = item_doc_node.body.innerText;
	//	//log(item_pt);
	//	//loadContent(item_pt);
	//}
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

