function initTextContent(itemDataStr) {
	try {
		if (hasJsonStructure(itemDataStr)) {
			let delta = JSON.parse(itemDataStr);
			quill.setContents(delta.ops, 'silent');
			return;
		}
		// issues: template's are given parent spans and overflow outside of template
		setTextInRange(getContentRange(),'','silent');
		insertHtml(0, itemDataStr,'silent');

		//issues: plain html will not format correctly
		//setHtml(itemDataStr);
		
	} catch (ex) {
		//malformed or unsupported content was 
		onException_ntf('setHtml', ex);
		let item_doc_node = domParser.parseFromString(itemDataStr, 'text/html');
		log('malformed content: ')
		log(itemDataStr);
		log('using plain text: ');
		//let item_pt = item_doc_node.body.innerText;
		//log(item_pt);
		//initContent(item_pt);
	}
}