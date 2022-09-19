function initTextContent(itemDataStr) {
	try {
		// NOTE pretty sure pasteHtml has issues but using setHtml when <code> tag present for example doesn't render 
		setTextInRange(getContentRange(),'');
		insertHtml(0, itemDataStr);

		//setHtml(itemDataStr);
		
	} catch (ex) {
		//malformed or unsupported content was 
		onException_ntf('setHtml', ex);
		let item_doc_node = domParser.parseFromString(itemDataStr, 'text/html');
		log('malformed content: ')
		log(itemDataStr);
		log('using plain text: ');
		let item_pt = item_doc_node.body.innerText;
		log(item_pt);
		initContent(item_pt);
	}
}