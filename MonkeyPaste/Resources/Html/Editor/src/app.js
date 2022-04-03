// const http = require('http');

// const hostname = '127.0.0.1';
// const port = 3000;

// const server = http.createServer((req, res) => {
//   res.statusCode = 200;
//   res.setHeader('Content-Type', 'text/plain');
//   res.end('Hello World');
// });

// server.listen(port, hostname, () => {
//   console.log(`Server running at http://${hostname}:${port}/`);
// });

import $ from "jquery";
window.jQuery = $;
window.$ = $;
require("quill");
require("quill-better-table");
require("quill-html-edit-button");
require("katex");
require("@fortawesome/fontawesome-free");
require("../lib/jquery-context-menus/src/context-menu.js");
require("./components/content/content-style.css");
require("./components/editor/editor-style.css");
require("./components/editor/quill-style.css");
require("./components/template/template-style.css");
