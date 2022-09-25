function loadTextContent(itemDataStr, isPasteRequest) {
	try {
		disableFileList();
		showEditor();

		//if (hasJsonStructure(itemDataStr)) {
		//	let delta = JSON.parse(itemDataStr);
		//	quill.setContents(delta.ops, 'silent');
		//	return;
		//}

		// issues: template's are given parent spans and overflow outside of template
		//setTextInRange(getContentRange(),'','silent');
		//insertHtml(0, itemDataStr,'silent');

		//issues: plain html will not format correctly
		setHtml(itemDataStr);

		loadTemplates();
		if (isPasteRequest && HasTemplates) {
			showPasteTemplateToolbar();
		}		
	} catch (ex) {
		//malformed or unsupported content was 
		onException_ntf('setHtml', ex);
		let item_doc_node = DomParser.parseFromString(itemDataStr, 'text/html');
		log('malformed content: ')
		log(itemDataStr);
		log('using plain text: ');
		//let item_pt = item_doc_node.body.innerText;
		//log(item_pt);
		//loadContent(item_pt);
	}
}