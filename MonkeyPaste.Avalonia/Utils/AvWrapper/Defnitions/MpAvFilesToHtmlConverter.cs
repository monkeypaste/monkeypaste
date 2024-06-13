using HtmlAgilityPack;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public class MpAvFilesToHtmlConverter : MpIFilesToHtmlConverter {
        public string ConvertToHtml(string[] paths) {
            List<double> colWidths = [30, 200];
            var doc = new HtmlDocument();
            doc.LoadHtml(string.Empty.ToHtmlDocumentFromTextOrPartialHtml());
            var tbody = doc.CreateElement("tbody");
            var table = doc.CreateElement("table",tbody);
            table.AddClass("quill-better-table");
            table.SetAttributeValue("style", $"width: {(int)colWidths.Sum()}px");
            
            var colgroup = doc.CreateElement("colgroup");
            colWidths.ForEach(x => {
                var col = doc.CreateElement("col");
                col.SetAttributeValue("width", $"{x}px");
                colgroup.AppendChild(col);
            });
            table.AppendChild(colgroup);

            var outer_div = doc.CreateElement("div",table);
            outer_div.AddClass("quill-better-table-wrapper");
            doc.DocumentNode.FirstChild.NextSibling.AppendChild(outer_div);

            foreach(var (path,r) in paths.WithIndex()) {     
                var img_elm = doc.CreateElement("img");
                img_elm.SetAttributeValue("src", Mp.Services.IconBuilder.GetPathIconBase64(path, MpIconSize.MediumIcon32).ToBase64ImageUrl());
                
                var img_cell = doc.CreateElement("td",img_elm);
                img_cell.SetAttributeValue("data-row", (r + 1).ToString());
                img_cell.SetAttributeValue("rowspan", 1.ToString());
                img_cell.SetAttributeValue("colspan", 1.ToString());
                
                var path_cell = doc.CreateElement("td",doc.CreateTextNode(path));
                path_cell.SetAttributeValue("data-row", (r + 1).ToString());
                path_cell.SetAttributeValue("rowspan", 1.ToString());
                path_cell.SetAttributeValue("colspan", 1.ToString());

                var tr = doc.CreateElement("tr");
                tr.SetAttributeValue("data-row", (r + 1).ToString());
                tr.AppendChild(img_cell);
                tr.AppendChild(path_cell);
                tbody.AppendChild(tr);
            }
            return doc.DocumentNode.InnerHtml;
        }
    }
}
