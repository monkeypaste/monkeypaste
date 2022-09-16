
function initFonts() {
    // Append the CSS stylesheet to the page
    var node = document.createElement("style");
    node.innerHTML = registerFontStyles(EnvName);
    document.body.appendChild(node);
}
