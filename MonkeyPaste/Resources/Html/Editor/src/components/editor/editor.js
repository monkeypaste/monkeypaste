var DefaultEditorWidth = 1200;

var IgnoreNextTextChange = false;
var IgnoreNextSelectionChange = false;

var IsLoaded = false;

var EnvName = "";

var IsClipboardDataReady = false;

var IsPastingTemplate = false;
var IsSubSelectionEnabled = false;

function initConverter() {
	reqMsg = {
		envName: 'wpf',
		isReadOnlyEnabled: true,
		usedTextTemplates: {},
		isPasteRequest: false,
		itemEncodedHtmlData: ''
	}

	EnvName = reqMsg.envName;

	loadQuill(reqMsg);

	document.getElementsByClassName("ql-toolbar")[0].classList.add("env-wpf");
	disableReadOnly();
	//hideEditorToolbar();
	window.addEventListener(
		"resize",
		function (event) {
			updateAllSizeAndPositions();
		},
		true
	);

	updateAllSizeAndPositions();

	IsLoaded = true;
	return "CONVERTER LOADED";
}

function convertPlainHtml(plainHtml) {
	//log("Converting This Plain Html:");
	//log(plainHtml);
	//setHtml(plainHtml);
	quill.deleteText(0, quill.getLength());
	quill.clipboard.dangerouslyPasteHTML(plainHtml);
	quill.update();
	return getHtml();
	//

	//
}

function init(initMsg) {
	//if (IsLoaded) {
	//	log('editor already loaded, setting html and  ignoring...');
	//	return;
	//}
	// reqMsgStr is serialized 'MpQuillLoadRequestMessage' object

	//log("init request: " + reqMsgStr);
	//drag/drop notes:
	// quill.root.removeEventListener('dragstart',getEventListeners(quill.root).dragstart[0].listener)
	// quill.root.removeEventListener('drop',getEventListeners(quill.root).drop[0].listener)

	if (initMsg == null) {
		//let sample1 = '<p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,128,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">//rtbvm.HasViewChanged = true;</span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">rtbvm.OnPropertyChanged(</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">nameof</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">(rtbvm.CurrentSize)); </span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> cilv =  </span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">this</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.GetVisualAncestor<</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(43,145,175);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpContentListView</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">>(); </span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">cilv.UpdateAdorner();</span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><br copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> rtbl = cilv.GetVisualDescendents<</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(43,145,175);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">RichTextBox</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">>(); </span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">double</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> totalHeight = rtbl.Sum(x => x.ActualHeight) +</span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(43,145,175);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpMeasurements</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.Instance.ClipTileEditToolbarHeight + </span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(43,145,175);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpMeasurements</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.Instance.ClipTileDetailHeight; </span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> ctcv =  </span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">this</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.GetVisualAncestor<</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(43,145,175);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpClipTileContainerView</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">>(); </span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">ctcv.ExpandBehavior.Resize(totalHeight); </span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,255);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> sv = cilv.ContentListBox.GetScrollViewer(); </span></p><p class="ql-align-left"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas"	style="font-size: 12.6666666666667px; color: rgb(0,0,0);"	copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">sv.InvalidateScrollInfo();</span></p><p class="ql-align-left"	copyitemblockguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><br copyiteminlineguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"></p>';
		
		let sample1 = "<html><body><!--StartFragment--><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>This article can be considered as the fourth instalment in the following sequence of articles:</p><ol style='margin: 10px 0px; padding: 0px 0px 0px 40px; border: 0px; color: rgb(17, 17, 17); font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><a href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'	style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Multiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks</a></li><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><a href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'	style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Basics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework</a></li><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><a href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'	style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Multiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples</a></li></ol><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>If you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.</p><!--EndFragment--></body></html>";

		// reqMsg = {
		//   envName: "web",
		//   isPasteRequest: false,
		//   isReadOnlyEnabled: false,
		//   itemEncodedHtmlData: sample1 + sample2,
		//   usedTextTemplates: []
		// };
		initMsg = {
			envName: 'wpf',
			copyItemId: 0,
			isReadOnlyEnabled: true,
			usedTextTemplates: {},
			isPasteRequest: false,
			itemEncodedHtmlData: sample1
		}
	}  
	EnvName = initMsg.envName;
	CopyItemId = initMsg.copyItemId;

	if (!IsLoaded) {
		loadQuill(initMsg);
	}

	initContent(initMsg.itemEncodedHtmlData);

	if (!IsLoaded) {
		initTemplates(initMsg.usedTextTemplates, initMsg.isPasteRequest);
	}

	initDragDrop();

	initClipboard();

	if (EnvName == "web") {
		//for testing in browser
		document.getElementsByClassName("ql-toolbar")[0].classList.add("env-web");
	} else {
		document.getElementsByClassName("ql-toolbar")[0].classList.add("env-wpf");

		if (initMsg.isReadOnlyEnabled) {
			enableReadOnly();
		} else {
			showEditorToolbar();
			disableReadOnly();
		}
	}

	window.addEventListener(
		"resize",
		function (event) {
			onWindowResize(event);
		},
		true
	);

	updateAllSizeAndPositions();
	window.onscroll = onWindowScroll;

	IsLoaded = true;

	log('Editor loaded');

	// init response is serialized 'MpQuillLoadResponseMessage'
	let initResponseMsg = {
		contentWidth: getContentWidth(),
		contentHeight: getContentHeight(),
		decodedTemplateGuids: getDecodedTemplateGuids()
	}
	let initResponseMsgStr = JSON.stringify(initResponseMsg);
	log('init Response: ');
	log(initResponseMsgStr);

	//setComOutput(initResponseMsgStr);

	return initResponseMsgStr;
}


function loadQuill(reqMsg) {
	Quill.register("modules/htmlEditButton", htmlEditButton);
	Quill.register({ "modules/better-table": quillBetterTable }, true);

	registerTemplateSpan();

	// Append the CSS stylesheet to the page
	var node = document.createElement("style");
	node.innerHTML = registerFontStyles(reqMsg.envName);
	document.body.appendChild(node);

	quill = new Quill("#editor", {
		//debug: true,
		placeholder: "",
		theme: "snow",
		modules: {
			table: false,
			toolbar: registerToolbar(reqMsg.envName),
			htmlEditButton: {
				syntax: true
			},
			"better-table": {
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
			},
			keyboard: {
				bindings: quillBetterTable.keyboardBindings
			}
		}
	});

	quill.root.setAttribute("spellcheck", "false");

	//quill.root.removeEventListener('drag', quill.root.ondrag);
	//quill.root.removeEventListener('dragend', quill.root.ondragend);
	//quill.root.removeEventListener('dragenter', quill.root.ondragenter);
	//quill.root.removeEventListener('dragleave', quill.root.ondragleave);
	//quill.root.removeEventListener('dragover', quill.root.ondragover);

	//quill.root.removeEventListener("dragstart", quill.root.ondragstart, true);
	//quill.root.removeEventListener("drop", quill.root.ondrop, true);

	initTableToolbarButton();

	window.addEventListener("click", onWindowClick);

	quill.on("selection-change", onEditorSelectionChanged);

	quill.on("text-change", onEditorTextChanged);
}

function registerToolbar(envName) {
	let sizes = registerFontSizes();
	let fonts = registerFontFamilys(envName);

	var toolbar = {
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

	return toolbar;
}

function focusEditor() {
	document.getElementById("editor").focus();
}
function hideScrollbars() {
	//document.querySelector('body').style.overflow = 'hidden';
	document.getElementById("editor").style.overflow = "hidden";
}

function showScrollbars() {
	//document.querySelector('body').style.overflow = 'scroll';
	document.getElementById("editor").style.overflow = "auto";
}

function getTotalHeight() {
	var totalHeight =
		getEditorToolbarHeight() + getEditorHeight() + getTemplateToolbarHeight();
	return totalHeight;
}

function updateAllSizeAndPositions() {
	//$(".ql-toolbar").css("position", "fixed");
	$(".ql-toolbar").css("top", 0);

	if (isEditorToolbarVisible()) {
		$("#editor").css("top", $(".ql-toolbar").outerHeight()); 
	} else {
		$("#editor").css("top", 0);
	}

	let wh = window.visualViewport.height;
	let eth = getEditorToolbarHeight();
	let tth = getTemplateToolbarHeight();

	$("#editor").css("height", wh - eth - tth);

	updateEditTemplateToolbarPosition();
	updatePasteTemplateToolbarPosition();

	drawOverlay();

	if (EnvName == "android") {
		//var viewportBottom = window.scrollY + window.innerHeight;
		//let tbh = $(".ql-toolbar").outerHeight();
		//if (y <= 0) {
		//    //keyboard is not visible
		//    $(".ql-toolbar").css("top", y);
		//    $("#editor").css("top", y + tbh);
		//} else {
		//    $(".ql-toolbar").css("top", y - tbh);
		//    $("#editor").css("top", 0);
		//}
		//$("#editor").css("bottom", viewportBottom - tbh);
	}
}

function onWindowClick(e) {
	if (
		e.path.find(
			(x) => x.classList && x.classList.contains("edit-template-toolbar")
		) != null ||
		e.path.find(
			(x) => x.classList && x.classList.contains("paste-template-toolbar")
		) != null ||
		e.path.find(
			(x) => x.classList && x.classList.contains("context-menu-option")
		) != null ||
		e.path.find((x) => x.classList && x.classList.contains("ql-toolbar")) !=
		null
	) {
		//ignore clicks within template toolbars
		return;
	}
	if (
		e.path.find(
			(x) => x.classList && x.classList.contains("ql-template-embed-blot")
		) == null
	) {
		hideAllTemplateContextMenus();
		hideEditTemplateToolbar();
		hidePasteTemplateToolbar();
		clearTemplateFocus();
	}
}

function onWindowScroll(e) {
	if (isReadOnly()) {
		return;
	}

	updateAllSizeAndPositions();
}

function onWindowResize(e) {
	updateAllSizeAndPositions();
	drawOverlay();
}

function onEditorSelectionChanged(range, oldRange, source) {
	//LastSelectedHtml = SelectedHtml;
	//SelectedHtml = getSelectedHtml();
	if (IgnoreNextSelectionChange) {
		IgnoreNextSelectionChange = false;
		return;
	}

	if (range) {
		refreshFontSizePicker();
		refreshFontFamilyPicker();

		if (range.length == 0) {
			var text = getText({ index: range.index, length: 1 });
			let ls = getLineStartDocIdx(range.index);
			let le = getLineEndDocIdx(range.index);
			log("User cursor is at " + range.index + ' idx before "' + text + '" line start: '+ls+' line end: '+le);
		} else {
			var text = getText(range);
			log(
				"User cursor is at " +
				range.index +
				" with length " +
				range.length +
				' and selected text "' +
				text +
				'"'
			);
		}

		refreshTemplatesAfterSelectionChange();
		updateTemplatesAfterSelectionChanged(range, oldRange, source);

		let selChangedObj = { copyItemId: CopyItemId, index: range.index, length: range.length };

		if (typeof notifyEditorSelectionChanged === 'function') {
			notifyEditorSelectionChanged(JSON.stringify(selChangedObj));
		}
		

	} else {
		log("Cursor not in the editor");
	}
	if (!range && !isEditTemplateTextAreaFocused()) {
		if (oldRange) {
			//blur occured
			quill.setSelection(oldRange);
		} else {
			return;
		}
		
	}
}

function onEditorTextChanged(delta, oldDelta, source) {
	updateAllSizeAndPositions();
	if (!IsLoaded) {
		return;
	}
	if (IgnoreNextTextChange) {
		IgnoreNextTextChange = false;
		return;
	}
	let srange = quill.getSelection();
	if (!srange) {
		return;
	}

	updateTemplatesAfterTextChanged(delta, oldDelta, source);

	if (typeof notifyContentLengthChanged === 'function') {
		// send MpQuillContentLengthChangedMessage msg to host to update ContetEnd offset
		let contentLengthChangedMsg = { copyItemId: CopyItemId, length: quill.getLength() };
		notifyContentLengthChanged(JSon.stringify(contentLengthChangedMsg));
	}
}

function selectAll() {
	quill.setSelection({ index: 0, length: qull.getLength()});
}

function isAllSelected() {
	let result = quill.getSelection().length == quill.getLength();
	return result;
}


function setHtml(html) {
	//quill.root.innerHTML = html;
	document.getElementsByClassName("ql-editor")[0].innerHTML = html;
}

function setHtmlFromBase64(base64Html) {
	console.log("base64: " + base64Html);
	let html = atob(base64Html);
	console.log("html: " + html);

	quill.root.innerHTML = html;

	var output = getHtmlBase64();
	return output;
}

function setContents(jsonStr) {
	quill.setContents(JSON.parse(jsonStr));
}

function getText(rangeObj) {
	if (!quill || !quill.root) {
		return '';
	}

	let wasReadOnly = isReadOnly();
	if (wasReadOnly) {
		document.getElementById('editor').firstChild.setAttribute('contenteditable', true);
		quill.update();
	}

	rangeObj = rangeObj == null ? { index: 0, length: quill.getLength() } : rangeObj;

	let text = quill.getText(rangeObj.index, rangeObj.length);
	if (wasReadOnly) {
		document.getElementById('editor').firstChild.setAttribute('contenteditable', false);
		quill.update();
	}
	return text;
}

function setTextInRange(range, text) {
	quill.deleteText(range.index, range.length);
	quill.insertText(range.index, text);

	//quill.setText(text + "\n");
}

function getSelectedText() {
	var selection = quill.getSelection();
	return quill.getText(selection.index, selection.length);
}

function getHtml() {
	if (quill && quill.root) {
		//var val = document.getElementsByClassName("ql-editor")[0].innerHTML;
		clearTemplateFocus();
		var val = quill.root.innerHTML;
		//log('getHtml response');
		//log(val);
		//setComOutput(JSON.stringify(val));

		return val;
	}
	setComOutput('');
	return '';
}

function getEncodedHtml() {
	resetTemplates();
	var result = encodeTemplates();
	return result;
}

function getSelectedHtml(maxLength) {
	maxLength = maxLength == null ? Number.MAX_SAFE_INTEGER : maxLength;

	var selection = quill.getSelection();
	if (selection == null) {
		return "";
	}
	if (!selection.hasOwnProperty("length") || selection.length == 0) {
		selection.length = 1;
	}
	if (selection.length > maxLength) {
		selection.length = maxLength;
	}
	var selectedContent = quill.getContents(selection.index, selection.length);
	var tempContainer = document.createElement("div");
	var tempQuill = new Quill(tempContainer);

	tempQuill.setContents(selectedContent);
	let result = tempContainer.querySelector(".ql-editor").innerHTML;
	tempContainer.remove();
	return result;
}

function getSelectedHtml2() {
	var selection = window.getSelection();
	if (selection.rangeCount > 0) {
		var range = selection.getRangeAt(0);
		var docFrag = range.cloneContents();

		let docFragStr = domSerializer.serializeToString(docFrag);

		const xmlnAttribute = ' xmlns="http://www.w3.org/1999/xhtml"';
		const regEx = new RegExp(xmlnAttribute, "g");
		docFragStr = docFragStr.replace(regEx, "");
		return docFragStr;
	}
	return "";
}

function createLink() {
	var range = quill.getSelection(true);
	if (range) {
		var text = getText(range);
		quill.deleteText(range.index, range.length);
		var ts =
			'<a class="square_btn" href="https://www.google.com">' + text + "</a>";
		quill.clipboard.dangerouslyPasteHTML(range.index, ts);

		log("text:\n" + getText());
		console.table("\nhtml:\n" + getHtml());
	}
}

function getIsClipboardReady() {
	var isReady = IsClipboardDataReady;
	return isReady ? "yes" : "no";
}

function isReadOnly() {
	var isEditable = document.getElementById('editor').firstChild.getAttribute('contenteditable');
	return !isEditable;
}

function enableReadOnly() {
	//deleteJsComAdapter();

	document.getElementById('editor').firstChild.setAttribute('contenteditable', false);
	document.getElementById('editor').firstChild.style.caretColor = 'transparent';

	quill.update();

	hideEditorToolbar();

	scrollToHome();
	hideScrollbars();

	IsSubSelectionEnabled = false;
	drawOverlay();
}

function disableReadOnly(isSilent) {		
	IsSubSelectionEnabled = true;

	if (!isSilent) {
		showEditorToolbar();
		showScrollbars();
	}

	document.getElementById('editor').firstChild.setAttribute('contenteditable', true);
	document.getElementById('editor').firstChild.style.caretColor = 'black';
	//$(".ql-editor").attr("contenteditable", true);
	//$(".ql-editor").css("caret-color", "black");


	//document.body.style.height = disableReadOnlyMsg.editorHeight;
	//$('.ql-editor').css('min-width', getEditorToolbarWidth());
	//$('.ql-editor').css('min-height', disableReadOnlyMsg.editorHeight);
	//document.getElementById('editor').style.minHeight = disableReadOnlyMsg.editorHeight - getEditorToolbarHeight() + 'px';
	//$('.ql-editor').css('width', DefaultEditorWidth);
	//document.body.style.minHeight = disableReadOnlyMsg.editorHeight;

	updateAllSizeAndPositions();

	refreshFontSizePicker();
	refreshFontFamilyPicker();

	drawOverlay();
}

function enableSubSelection() {
	IsSubSelectionEnabled = true;
	if (isReadOnly()) {
		document.getElementById('editor').firstChild.style.caretColor = 'red';
	} else {
		// this SHOULD be set already in disableReadOnly but setting here to ensure state
		document.getElementById('editor').firstChild.style.caretColor = 'black';
	}
	drawOverlay();
}

function disableSubSelection() {
	IsSubSelectionEnabled = false;

	if (isReadOnly()) {
		document.getElementById('editor').firstChild.style.caretColor = 'transparent';

		let selection = quill.getSelection();
		if (selection) {
			setSelection({ index: selection.index, length: 0 });
		}
	}

	drawOverlay();
}

function getSelection() {
	let selection = quill.getSelection();
	return selection;
}

function setSelection(selObj) {
	let index = 0;
	let length = 0;
	if (typeof selObj === 'string' || selObj instanceof String) {
		selObj = JSON.parse(selObj);
	}

	index = selObj.index;
	length = selObj.length;

	quill.setSelection(index, length);
}

function isShowingEditorToolbar() {
	$(".ql-toolbar").css("display") != "none";
}

function hideEditorToolbar() {
	if (EnvName == "web") {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.remove("ql-toolbar-env-web");
	} else {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.remove("ql-toolbar-env-wpf");
	}
	document.getElementsByClassName("ql-toolbar")[0].classList.add("hidden");

	//document.getElementById('editor').previousSibling.style.display = 'none';
	updateAllSizeAndPositions();
}

function showEditorToolbar() {
	document.getElementsByClassName("ql-toolbar")[0].classList.remove("hidden");
	if (EnvName == "web") {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.add("ql-toolbar-env-web");
	} else {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.add("ql-toolbar-env-wpf");
	}
	updateAllSizeAndPositions();
}

function isEditorToolbarVisible() {
	return !document.getElementsByClassName("ql-toolbar")[0].classList.contains('hidden');
}

function getEditorWidth() {
	var editorRect = document.getElementById("editor").getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').wi());
	return editorRect.width;
}

function getEditorHeight() {
	var editorRect = document.getElementById("editor").getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').outerHeight());
	return editorRect.height;
}

function getEditorToolbarWidth() {
	if (isReadOnly()) {
		return 0;
	}
	return document
		.getElementsByClassName("ql-toolbar")[0]
		.getBoundingClientRect().width;
}

function getEditorToolbarHeight() {
	if (isReadOnly()) {
		return 0;
	}
	var toolbarHeight = parseInt($(".ql-toolbar").outerHeight());
	return toolbarHeight;
}

function scrollToHome() {
	document.getElementById("editor").scrollTop = 0;
}

