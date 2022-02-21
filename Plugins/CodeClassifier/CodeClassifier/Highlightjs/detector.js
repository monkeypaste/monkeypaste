let hljs = require('./highlightjs_packed.js');
var allLanguages = hljs.listLanguages();
var commonLanguages = ["cpp", "cs", "css", "javascript", "java", "objectivec", "perl", "php", "python", "ruby", "sql", "xml", "autohotkey", "lua", "actionscript", "swift", "vbscript"];

var input;
var languages;

function detect() {
    var highlightResult = hljs.highlightAuto(input, languages);

    if (typeof highlightResult.language != "undefined") {
        var languageObj = hljs.getLanguage(highlightResult.language);
        var languages = [];
        if (typeof languageObj.aliases != "undefined") {
            languages = languageObj.aliases.slice();
        }
        languages.unshift(highlightResult.language);

        return languages.toString().replace(/,/g, ", ");

    }
    if (input.length == 0) {
        return "Error: No code entered. Please paste your code above and try again.";
    }
    return 'Error: Unable to identify the programming language. Please add more code or uncheck the "Common Languages Only" option.';
}

//document.getElementById("highlight").onclick = function () {
//    detect(document.getElementById("pasteCode").value);
//}