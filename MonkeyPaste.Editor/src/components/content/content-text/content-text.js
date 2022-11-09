const InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
const BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe', 'blockquote']
const AllDocumentTags = [...InlineTags,...BlockTags];

function loadTextContent(itemDataStr, isPasteRequest) {
	quill.enable(true);
	setRootHtml(itemDataStr);
	//setRootHtml('');
	//insertHtml(0, itemDataStr);

	loadTemplates(isPasteRequest);
}


function adjustQueryRangesForEmptyContent(ranges) {
	// HACK embed's are empty string in getText.
	// this counts all template instance leading to the range
	// and adjusts for that count 
	for (var i = 0; i < ranges.length; i++) {
		let cur_doc_idx = ranges[i].index;
		let pre_template_count = getTemplateCountBeforeDocIdx(cur_doc_idx);

		ranges[i].index += pre_template_count;
	}
	return ranges;
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

