var allLanguages = hljs.listLanguages();
var commonLanguages = ["cpp", "cs", "css", "javascript", "java", "objectivec", "perl", "php", "python", "ruby", "sql", "xml", "autohotkey", "lua", "actionscript", "swift", "vbscript"];
var highlightResult;
document.getElementById("commonLanguageTitle").title = commonLanguages.toString().replace(/,/g, ", ");

document.getElementById("highlight").onclick = function () {
    var code = document.getElementById("pasteCode").value;
    document.getElementById("languageOutput").hidden = true;
    document.getElementById("option2").hidden = true;
    document.getElementById("option2Languages").hidden = true;
    document.getElementById("option2SelectTd").hidden = true;
    document.getElementById("option2Select").disabled = false;
    document.getElementById("option2Select").innerText = "Select";
    document.getElementById("highlightCode").innerHTML = "";
    document.getElementById("highlightCode").className = "";
    document.getElementById("option1Select").onclick = null;
    document.getElementById("option2Select").onclick = null;
    document.getElementById("error").hidden = true;

    if (document.getElementById("commonLanguagesOnly").checked) {
        highlightResult = hljs.highlightAuto(code, commonLanguages);
    } else {
        highlightResult = hljs.highlightAuto(code, allLanguages);
    }

    if (typeof highlightResult.language != "undefined") {
        document.getElementById("highlightCode").innerHTML = highlightResult.value;

        document.getElementById("pasteCode").value = "";
        var languageObj = hljs.getLanguage(highlightResult.language);
        var languages = [];
        if (typeof languageObj.aliases != "undefined") {
            languages = languageObj.aliases.slice();
        }
        languages.unshift(highlightResult.language);

        document.getElementById("languageOutput").hidden = false;
        document.getElementById("option1Languages").innerText = languages.toString().replace(/,/g, ", ");
        document.getElementById("option1Select").disabled = true;
        document.getElementById("option1Select").innerText = "Selected";
        document.getElementById("option1Select").onclick = function () {
            document.getElementById("option1Select").disabled = true;
            document.getElementById("option1Select").innerText = "Selected";
            document.getElementById("option2Select").disabled = false;
            document.getElementById("option2Select").innerText = "Select";
            document.getElementById("highlightCode").innerHTML = highlightResult.value;
        };

        if (typeof highlightResult.second_best != "undefined") {
            var languageObj = hljs.getLanguage(highlightResult.second_best.language);
            var languages = [];
            if (typeof languageObj.aliases != "undefined") {
                languages = languageObj.aliases.slice();
            }
            languages.unshift(highlightResult.second_best.language);
            document.getElementById("option2Languages").innerText = languages.toString().replace(/,/g, ", ");
            document.getElementById("option2Select").onclick = function () {
                document.getElementById("option1Select").disabled = false;
                document.getElementById("option1Select").innerText = "Select";
                document.getElementById("option2Select").disabled = true;
                document.getElementById("option2Select").innerText = "Selected";
                document.getElementById("highlightCode").innerHTML = highlightResult.second_best.value;
            };
            document.getElementById("option2").hidden = false;
            document.getElementById("option2Languages").hidden = false;
            document.getElementById("option2SelectTd").hidden = false;
        }

        document.getElementById("highlightCode").className = "hljs";

    } else {
        document.getElementById("error").hidden = false;
        if (document.getElementById("pasteCode").value.length == 0) {
            document.getElementById("error").innerText = "Error: No code entered. Please paste your code above and try again.";
        } else {
            if (document.getElementById("commonLanguagesOnly").checked) {
                document.getElementById("error").innerText = 'Error: Unable to identify the programming language. Please add more code or uncheck the "Common Languages Only" option.';
            } else {
                document.getElementById("error").innerText = "Error: Unable to identify the programming language. Please add more code to increase the accuracy of the detection.";
            }
        }
    }

    console.log(document.getElementById("option1Languages").innerText);
};