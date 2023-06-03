﻿
// #region Life Cycle

function initPlainHtmlConverter() {
	getEditorContainerElement().classList.add('html-converter');

	globals.quill = initQuill();
	initClipboard();
	getEditorElement().classList.add('ql-editor-converter');


	globals.IsConverterLoaded = true;
	globals.IsLoaded = true;

	onInitComplete_ntf();
}

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isPlainHtmlConverter() {
	return getEditorContainerElement().classList.contains('html-converter');
}
// #endregion State

// #region Actions
function convertPlainHtml(dataStr, formatType, bgOpacity = 0.0) {
	if (!globals.IsConverterLoaded) {
		log('convertPlainHtml error! converter not initialized, returning null');
		return null;
	}
	const DO_VALIDATE = true;

	log("Converting This Plain Html:");
	log(dataStr);

	setRootHtml('');
	updateQuill();

	let qhtml = '';
	let formatted_delta = '';
	let iconBase64 = '';
	let base_line_text = '';

	if (formatType == 'text') {
		base_line_text = dataStr;
		let encoded_pt = encodeHtmlSpecialEntitiesFromPlainText(dataStr);
		insertText(0, encoded_pt, 'silent');
	} else {
		if (DO_VALIDATE) {
			base_line_text = globals.DomParser.parseFromString(dataStr, 'text/html').body.innerText;
		}
		
		let htmlStr = dataStr;// fixPlainHtmlColorContrast(dataStr, bgOpacity);
		//htmlStr = swapPreForDivTags(htmlStr);
		//htmlStr = encodeHtmlSpecialEntitiesFromHtmlDoc(htmlStr);
		insertHtml(0,htmlStr,'user');
	}
	updateQuill();
	if (isTableInDocument()) {
		// delta-to-html doesn't convert tables
		qhtml = getHtml2();
		// better table throws exception setting html so don't test table

	} else {
		qhtml = getHtml2();
		if (!isRunningOnHost()) {
			// just for testing html conversion
			//setRootHtml(qhtml);
			//setRootHtml('');
			//insertHtml(0, qhtml, 'user');
		}
	}
	if (qhtml == '') {
		// fallback and use delta2html, i think its a problem when there's only 1 block and content was plain text
		qhtml = getHtml();
	}

	if (DO_VALIDATE) {
		const converted_text = trimQuillTrailingLineEndFromText(globals.DomParser.parseFromString(qhtml, 'text/html').body.innerText);
		const diff = getStringDifference(base_line_text, converted_text);
		if (converted_text.length == base_line_text.length) {
			log('conversion validate: PASSED');
		} else {
			log('conversion validate: FAILED');
			log('first diff: ' + diff);
			log('base length: ' + base_line_text.length);
			log('converted length: ' + converted_text.length);
			log('base text:')
			log(base_line_text);
			log('converted text:');
			log(converted_text);
			onException_ntf('conversion error');
		}
	}
	formatted_delta = convertHtmlToDelta(qhtml);
	//else if (formatType == 'rtf2html') {
	//	formatted_delta = convertHtmlToDelta(dataStr);
	//	setContents(formatted_delta);
	//	qhtml = getHtml(false);
	//	//qhtml = forceHtmlBgOpacity(qhtml, bgOpacity);

	//	//formatted_delta = convertHtmlToDelta(qhtml);
	//	//setRootHtml(qhtml);
	//}
	//else if (formatType == 'html') {
	//	//iconBase64 = locateFaviconBase64(dataStr);

	//	// NOTE this maybe only necessary on windows
	//	//qhtml = fixHtmlBug1(qhtml);
	//	//qhtml = removeUnicode(qhtml);
	//	//qhtml = fixUnicode(qhtml);

	//	setRootHtml(dataStr);
	//	//dataStr = encodeHtmlSpecialEntitiesFromHtmlDoc(dataStr);
	//	//insertHtml(0, dataStr, 'user', false);
	//	formatted_delta = forceDeltaBgOpacity(getDelta(), bgOpacity);
	//	setContents(formatted_delta);
	//	if (isTableInDocument()) {
	//		// delta-to-html doesn't convert tables 
	//		qhtml = getHtml2();
	//		// better table throws exception setting html so don't test table

	//	} else {
	//		qhtml = getHtml();
	//		if (!isRunningOnHost()) {
	//			// just for testing html conversion
	//			setRootHtml(qhtml);
	//		}
			
	//	}
	//}
	
	log('');
	log('RichHtml: ');
	log(qhtml);
	return {
		html: qhtml,
		delta: formatted_delta,
		//icon: iconBase64
	};
}

function locateFaviconBase64(htmlStr) {
	htmlStr = `<!DOCTYPE HTML><html lang="en-US"><head>  <script>    const className = navigator.userAgent.match(/mobile|android|iphone|ipad/i);    document.querySelector('html').className = className ? 'ua-mobile' : 'ua-desktop';  </script>  <meta name="viewport" content="width=device-width, initial-scale=1" />  <meta name="theme-color" content="#ffd966" />  <title>Transmat - Seamless cross web interactions</title>  <link rel=icon href='data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><text y=".9em" font-size="90">🌀</text></svg>'>  <link rel="stylesheet" href="styles.css" /></head><body>  <header>    <div class="container">      <div class="intro">        <h1>Transmat</h1>        <p>          <strong>Seamless cross web interactions</strong><br />          Enable users to transfer data to external apps, and open your webapp to receive external data.        </p>        <p class="buttons">          <a class="button" href="https://GitHub.com/google/transmat">View on GitHub</a>          <a class="github-button" href="https://github.com/google/transmat" data-size="large"                data-show-count="true" aria-label="Star google/transmat on GitHub">              Star          </a>        </p>      </div>      <div class="intro-animation">        <div class="animation">          <div class="browser b1">            <div class="controls">              <span class="c1"></span>              <span class="c2"></span>              <span class="c3"></span>            </div>            <div class="viewport">            </div>          </div>          <div class="browser b2">            <div class="controls">              <span class="c1"></span>              <span class="c2"></span>              <span class="c3"></span>            </div>            <div class="viewport">            </div>          </div>          <div class="drag">            <div class="cursor"></div>            <div class="source"></div>          </div>        </div>      </div>    </div>  </header>  <section class="readme">    <div class="container">      <p>        <img class="payloads" src="payloads.png" alt="Multiple payloads in a single transmit" />        <strong>        Transmat is a small library around the        <a href="https://developer.mozilla.org/en-US/docs/Web/API/DataTransfer">DataTransfer API</a>        that eases transmitting and receiving data in your web app using drag-drop and copy-paste        interactions.</strong></p>      <p>The DataTransfer API has the ability to transfer multiple string data payloads to any other        application on the user's device. This technique is        <a href="https://caniuse.com/mdn-api_datatransfer_setdata">compatible with all modern desktop          browsers</a> (everything after IE11) and can be used today. It will bring your PWA to parity          with native applications.</p>      <h3>Transmat is something for you, if you...</h3>      <ul>        <li>...are looking for a cheap way to integrate with external (WYSIWYG) applications.</li>        <li>...want to provide user the ability to share their data with other applications, even those who you don't know about.</li>        <li>...want external applications to be able to deeply integrate with your web app.</li>        <li>...want to make your app better fit in existing workflows of your user.</li>      </ul>    </div>  </section>  <section class="demo show-on-mobile">    <div class="container">      <h2>Demo</h2>      <p>Transmat only works on desktop browsers. Watch the demo video below.</p>      <div class="video">        <iframe width="560" height="315"             src="https://www.youtube.com/embed/EYMgUhn_Zdo?autoplay=1&controls=0&fs=0&loop=1&modestbranding=1"             title="YouTube video player" frameborder="0"             allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"             allowfullscreen>        </iframe>      </div>    </div>  </section>  <section class="demo show-on-desktop">    <div class="container">      <p class="intro">        <strong>Drag or copy the element below</strong> to a new        <a href="https://google.github.io/transmat" target="_blank">browser window</a>,        a <a href="https://codepen.io/pen" target="_blank">text editor</a> or        <a href="https://doc.new" target="_blank">WYSIWYG editor</a>.      </p>      <div class="targets">        <p class="transmitter" tabindex="0" draggable="true">          <span>            <strong>Drag or copy me!</strong><br />            <em>Containing HTML, Text, URL and JSON data.</em>          </span>        </p>        <p class="receiver" tabindex="0">          <span>Drop or paste here!</span>        </p>      </div>      <div class="results">        <pre class="language-html"><code></code></pre>      </div>    </div>  </section>  <section class="readme">    <div class="container">      <h2>Getting started</h2>      <ol>        <li>          <p>Install the library from <a href="https://npmjs.com/transmat">NPM</a> (version 0.2.1).            <pre class="language-js"><code>npm install transmat</code></pre></p>          <p>Alternatively, you can load the <a href="https://unpkg.com/transmat/lib/index.umd.js">index.umd.js</a> UMD build from a JavaScript CDN like UNPKG.             The library is exported to the transmat object namespace.            <pre class="language-html"><code>&lt;script src="https://unpkg.com/transmat/lib/index.umd.js"&gt;&lt;/script&gt;&lt;script&gt;  // Use transmat.addListeners, transmat.Transmat, etc...&lt;/script&gt;</code></pre></p>        </li>        <li>Add a draggable and focusable element to your webpage.          <pre class="language-html"><code>&lt;div id="myElement" draggable="true" tabindex="0"&gt;  Transmitter and receiver&lt;/div&gt;</code></pre>        </li>        <li><strong>Listen for transmit events on the element.</strong><br />            Create a new Transmat instance for the event, and provide a object with mime-type / data.          <pre class="language-js"><code>import {Transmat, addListeners} from 'transmat';addListeners(myElement, 'transmit', event => {  const transmat = new Transmat(event);  transmat.setData({    'text/plain': 'Hello world!',    'text/html': '&lt;h1&gt;Hello world!&lt;/h1&gt;',    'text/uri-list': 'http://example.com',    'application/json': {foo:'bar'}  });});</code></pre>        </li>        <li>          <strong>Listen for receiving events on the element.</strong><br />          Create a Transmat instance for the event, and, in this case, only accept application/json payload.          <pre class="language-js"><code>import {Transmat, addListeners} from 'transmat';addListeners(myElement, 'receive', event => {  const transmat = new Transmat(event);  if (transmat.hasType('application/json') && transmit.accept()) {    const payload = transmat.getData('application/json');    console.log(JSON.parse(payload));  }});</code></pre>        </li>      </ol>      <a class="webdev" href="https://web.dev/datatransfer">        <img src="https://web.dev/images/lockup.svg" alt="web.dev" width="120" />        <div class="content">          <h2>Read the DataTransfer article on web.dev</h2>          <p>Explaining the used transferring technique in detail, listing some concerns and opportunities for your applications. <u>Learn more.</u></p>        </div>      </a>      <hr />      <h2>Observing transfer events</h2>      <p>You can make use of the included TransmatObserver class to respond to drag activity. An        example is at the this webpage, where the drop targets respond to the drag events.</p>        <pre class="language-js"><code>import {TransmatObserver, Transmat} from 'transmat';const obs = new TransmatObserver(entries => {  for (const entry of entries) {    const transmat = new Transmat(entry.event);    if(transmat.hasMimeType(myCustomMimeType)) {      entry.target.classList.toggle('drag-over', entry.isTarget);      entry.target.classList.toggle('drag-active', entry.isActive);    }  }});obs.observe(myElement);</code></pre>      <hr />      <h2>Minimal drag image</h2>      <p>By default, HTML5 Drag and Drop API drag image is showing the rendered source element.        When applying Transmat on top of existing drag-drop interactions you might want something        more subtle instead.</p>      <p>Transmat comes with a small function that replaces this with a minimal alternative that        still gives the feeling of dragging an object.        <span class="minimal-drag-image" draggable="true" tabindex="0">Drag me for an example</span>, and        notice the checkered rectangle.</p>        <pre class="language-js"><code>import {Transmat, addListeners} from 'transmat';import {setMinimalDragImage} from 'transmat/lib/data_transfer';addListeners(myElement, 'transmit', event => {  const transmat = new Transmat(event);  setMinimalDragImage(transmat.dataTransfer);});</code></pre>      <hr />      <h2>Connecting the web, with JSON-LD</h2>      <p><img class="json-ld-logo" src="json-ld.png" alt="JSON-LD" />        While custom payloads are useful for communication between applications you have in your        control, it also limits the ability to transfer data to external apps.        <a href="https://json-ld.org">JSON-LD (Linked Data)</a> is a great universal standard for this;      </p>      <ul>        <li>Easy to generate from JavaScript,</li>        <li>Many predefined types at <a href="https://schema.org/Thing">Schema.org</a>,</li>        <li>It can contain custom schema definitions.</li>      </ul>      <p>JSON-LD might sound scarier than it is. Here’s an example of what this looks like for a <a href="https://schema.org/Person">Person</a>:</p>        <pre class="language-js"><code>const person = {  '@context': 'https://schema.org',  '@type': 'Person',  name: 'Rory Gilmore',  image: 'https://example.com/rory.jpg',  address: {    '@type': 'PostalAddress',    addressCountry: 'USA',    addressRegion: 'Connecticut',    addressLocality: 'Stars Hollow'  },};transmat.setData('application/ld+json', person);</code></pre>      <p>Transmat comes with several <a href="https://github.com/google/transmat/blob/main/src/json_ld.ts">JSON-LD utilities</a> to ease common interactions with the data.</p>      <p>      By using JSON-LD data, you will <strong>support a connected and open web</strong>. Think of      all the possibilities when you could transfer elements to other applications to continue your work.      That would be great, right? <strong>This starts with you! 🙌</strong></p>      <hr />      <h2>Learn more</h2>      <p>More documentation and examples at the <a href="https://GitHub.com/google/transmat">GitHub repository</a>.</p><p class="social">    <a class="twitter-share-button"          href="https://twitter.com/intent/tweet?text=Seamless interactions across the web with the DataTransfer API.&url=https://google.github.io/transmat"          data-size="large">      Share on Twitter    </a>    <a class="github-button" href="https://github.com/google/transmat" data-size="large"          data-show-count="true" aria-label="Star google/transmat on GitHub">      Star    </a></p>      <p><em>This is not an officially supported Google product.</em></p>    </div>  </section>  <script async src="prismjs.js"></script>  <script async src="dist/index.js"></script>  <script async defer src="https://buttons.github.io/buttons.js"></script>  <script async src="https://www.googletagmanager.com/gtag/js?id=G-WVGTJBXS6K"></script>  <script>    window.dataLayer = window.dataLayer || [];    function gtag(){dataLayer.push(arguments);}    gtag('js', new Date());    gtag('config', 'G-WVGTJBXS6K');  </script>  <script>window.twttr = (function(d, s, id) {    var js, fjs = d.getElementsByTagName(s)[0],      t = window.twttr || {};    if (d.getElementById(id)) return t;    js = d.createElement(s);    js.id = id;    js.src = "https://platform.twitter.com/widgets.js";    fjs.parentNode.insertBefore(js, fjs);    t._e = [];    t.ready = function(f) {      t._e.push(f);    };    return t;  }(document, "script", "twitter-wjs"));</script></body></html>`;

	const BASE64_PREFIX = 'data:image/png;base64,';
	const SVG_PREFIX = 'data:image/svg+xml,';


	let html_doc = globals.DomParser.parseFromString(htmlStr, 'text/html');
	let favicon_links = Array.from(html_doc.head.querySelectorAll('link[rel="icon"],link[rel="shortcut icon"]'));
	let results = [];
	for (var i = 0; i < favicon_links.length; i++) {
		let favicon_link = favicon_links[i];
		if (favicon_link.getAttribute('href').startsWith(SVG_PREFIX)) {
			let svg_html = favicon_link.getAttribute('href').split(SVG_PREFIX)[1];

			//var favicon_xml = (new XMLSerializer).serializeToString(svg_html);

			//var ctx = getOverlayContext();
			var img = new Image;
			img.onload = function (e) {
				let w = e.currentTarget.width;
				let h = e.currentTarget.height;

				let cnv = document.createElement('canvas');				
				cnv.width = w;
				cnv.height = h;

				let ctx = cnv.getContext('2d');

				ctx.drawImage(e.currentTarget, 0, 0, w, h);
				let favicon_base64 = cnv.toDataURL().split(BASE64_PREFIX)[1]; 
			};
			//img.src = "data:image/svg+xml;base64," + btoa(svg_html);
			img.src = favicon_link.getAttribute('href');
		}
		
	}

}

function loadImageAsPNG(url, height, width) {
	return new Promise((resolve, reject) => {
		let sourceImage = new Image();

		sourceImage.onload = () => {
			let png = new Image();
			let cnv = document.createElement('canvas'); 
			cnv.height = height;
			cnv.width = width;

			let ctx = cnv.getContext('2d');

			ctx.drawImage(sourceImage, 0, 0, height, width);
			png.src = cnv.toDataURL(); // defaults to image/png
			resolve(png);
		}
		image.onerror = reject;

		image.src = url;
	});
}

function swapPreForDivTags(htmlStr) {
	let html_doc = globals.DomParser.parseFromString(htmlStr, 'text/html');
	let elms = html_doc.querySelectorAll('pre');
	for (var i = 0; i < elms.length; i++) {
		let node = elms[i];
		let div_elm = html_doc.createElement('p');
		div_elm.classList = node.classList;
		div_elm.style = node.style;
		div_elm.innerHTML = node.innerHTML;
		let parent_elm = node.parentElement;
		parent_elm.replaceChild(div_elm, node);
	}
	return html_doc.body.innerHTML;
}

function fixPlainHtmlColorContrast(htmlStr, opacity) {
	const bg_color = findElementBackgroundColor(getEditorElement());
	const is_bg_bright = isBright(bg_color);
	const fallback_fg = is_bg_bright ? cleanColor('black') : cleanColor('white');

	let html_doc = globals.DomParser.parseFromString(htmlStr, 'text/html');
	let elms = html_doc.querySelectorAll(globals.InlineTags.join(", ") + ',' + globals.BlockTags.join(','));
	for (var i = 0; i < elms.length; i++) {
		try {
			if (!isNullOrUndefined(elms[i].style.backgroundColor)) {
				let bg_rgba = cleanColor(elms[i].style.backgroundColor);
				bg_rgba.a = opacity;
				let newBg = rgbaToCssColor(bg_rgba);
				elms[i].style.backgroundColor = newBg;
			}
			if (!isNullOrUndefined(elms[i].style.color)) {
				let fg_rgba = elms[i].style.color.startsWith('var') ? fallback_fg : cleanColor(elms[i].style.color);
				const is_fg_bright = isBright(fg_rgba);
				if (is_bg_bright != is_fg_bright) {
					// contrast is ok, ignore
					continue;
				}
				if (isRgbFuzzyBlackOrWhite(fg_rgba)) {
					// if fg isn't a particular color just adjust to bg
					fg_rgba = cleanColor(is_bg_bright ? 'black' : 'white');
				} else {
					// for unique color 
					const amount = is_fg_bright ? -50 : 50;
					fg_rgba = shiftRgbaLightness(fg_rgba, amount);
				}
				fg_rgba.a = 1;
				let newFg = rgbaToCssColor(fg_rgba);
				elms[i].style.color = newFg;

			}
		} catch (ex) {
			log(ex);
			debugger;
		}
	}
	return html_doc.body.innerHTML;
}

function forceDeltaBgOpacity(delta, opacity) {
	if (!delta || delta.ops === undefined || delta.ops.length == 0) {
		return delta;
	}
	delta.ops
		.filter(x => x.attributes !== undefined && x.attributes.background !== undefined)
		.forEach(x => x.attributes.background = cleanColor(x.attributes.background, 0, 'rgbaStyle'));

	delta.ops
		.filter(x => x.attributes !== undefined && x.attributes.background !== undefined)
		.forEach(x => log(x.attributes.background));

	return delta;
}

function fixHtmlBug1(htmlStr) {
	// replace <span>Â </span>
	return htmlStr.replaceAll('<span>Â </span>', '');
}

function removeUnicode(str) {
	str = str.replace(/[\uE000-\uF8FF]/ig, '');
	return str;
}
function fixUnicode(text) {
	// replaces any combo of chars in [] with single space
	const regex = /(?!\w*(\w)\w*\1)[Âï¿½]+/ig;
	const regex2 = /[^\u0000-\u007F]+/ig;

	let fixedText = text.replaceAll(regex, ' ');
	fixedText = fixedText.replaceAll(regex2, '');
	return fixedText;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers
