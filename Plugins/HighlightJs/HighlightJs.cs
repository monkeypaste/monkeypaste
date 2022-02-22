using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;

namespace CodeClassify {

    public class HighlightJs : MpIAnalyzerPluginComponent {        
        private string[] commonLanguages = new string[] { "cpp", "cs", "css", "javascript", "java", "objectivec", "perl", "php", "python", "ruby", "sql", "xml", "autohotkey", "lua", "actionscript", "swift", "vbscript" };

        //WebBrowser browser;

        //bool loaded = false;
        //public CodeClassify() {
        //    string curDir = Directory.GetCurrentDirectory();
        //    var uri = new Uri("pack://application:,,,/CodeClassify;component/classifier.html");
        //    var assembly = Assembly.GetExecutingAssembly();
        //    var stream = assembly.GetManifestResourceStream("CodeClassify.classifier.html");

        //    browser = new WebBrowser();
        //    browser.NavigateToStream(stream);
        //    browser.LoadCompleted += (s, e) => { browser.InvokeScript("init('')"); };
        //}

        //public async Task<object> AnalyzeAsync(object args) {

        //    object outResult = null;
        //    bool isDone = false;

        //    var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());

        //    var languages = reqParts.FirstOrDefault(x => x.enumId == 1).value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        //    string code = reqParts.FirstOrDefault(x => x.enumId == 2).value;
        //    code = string.Format(@"<pre><code>{0}</code></pre>", code);
        //    browser.InvokeScript("init", code);
        //    browser.LoadCompleted += (s, e) => {
        //        outResult = browser.InvokeScript("detect");
        //        browser.LoadCompleted += (s1, e1) => {
        //            isDone = true;
        //        };
        //    };
        //    while (!isDone) {
        //        await Task.Delay(100);
        //    }


        //    Console.WriteLine("Detection: " + outResult.ToString());
        //    return outResult.ToString();
        //}
        //Window window;
        //WebView2 webView;

        bool loaded = false;
        public HighlightJs() {
            //string curDir = Directory.GetCurrentDirectory();
            //var uri = new Uri("pack://application:,,,/CodeClassify;component/classifier.html");
            //var assembly = Assembly.GetExecutingAssembly();
            //var stream = assembly.GetManifestResourceStream("CodeClassify.classifier.html");
            //webView = new WebView2();
            //var webView2Environment = await CoreWebView2Environment.CreateAsync();
            //await webView.EnsureCoreWebView2Async(webView2Environment);
            //webView.Source = uri;
        }

        public async Task<object> AnalyzeAsync(object args) {
            var argParts = args as object[];
            var wv = argParts[0] as WebView2;
            var reqStr = argParts[1] as string;
            var reqParts = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(reqStr);


            var languages = reqParts.FirstOrDefault(x => x.enumId == 1).value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string code = reqParts.FirstOrDefault(x => x.enumId == 2).value;
            code = string.Format(@"<pre><code>{0}</code></pre>", code);

            string curDir = Directory.GetCurrentDirectory();
            var uri = new Uri("pack://application:,,,/CodeClassify;component/classifier.html");
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("CodeClassify.classifier.html");
            //webView = new WebView2();
            //var webView2Environment = await CoreWebView2Environment.CreateAsync();
            //await webView.EnsureCoreWebView2Async(null);
            //wv.Source = uri;
            bool isDone = false;
            string html = File.ReadAllText(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MpWpfApp\bin\Debug\Plugins\CodeClassify\classifier.html");
            wv.NavigateToString(html);
            wv.NavigationCompleted += (s, e) => {
                isDone = true;
            };
            while(!isDone) {
                await Task.Delay(100);
            }
            //await wv.EnsureCoreWebView2Async();

            await wv.ExecuteScriptFunctionAsync("init", code, commonLanguages);
            string outResult = await wv.ExecuteScriptFunctionAsync("detect");

            Console.WriteLine("Detection: " + outResult.ToString());
            return outResult.ToString();
        }
    }

    public static class Extensions {
        public static async Task<string> ExecuteScriptFunctionAsync(this WebView2 webView2, string functionName, params object[] parameters) {
            string script = functionName + "(";
            for (int i = 0; i < parameters.Length; i++) {
                script += JsonConvert.SerializeObject(parameters[i]);
                if (i < parameters.Length - 1) {
                    script += ", ";
                }
            }
            script += ");";
            return await webView2.ExecuteScriptAsync(script);
        }
    }
}
