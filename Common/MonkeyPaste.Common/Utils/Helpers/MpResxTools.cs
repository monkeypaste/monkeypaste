using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Resources.NetStandard;

namespace MonkeyPaste.Common {
    public static class MpResxTools {
        public static Dictionary<string, (string value, string comment)> ReadResxFromPath(string path) {
            using ResXResourceReader reader = new ResXResourceReader(path);
            reader.UseResXDataNodes = true;
            Dictionary<string, (string value, string comment)> result = new();
            foreach (DictionaryEntry d in reader) {
                ResXDataNode node = (ResXDataNode)d.Value;
                string value = (string)node.GetValue((ITypeResolutionService)null);
                result.Add(d.Key.ToStringOrEmpty(), (value, node.Comment));
            }
            return result;
        }

        public static string WriteResxToPath(string resx_path, Dictionary<string, (string value, string comment)> resx_lookup) {
            if (resx_path.IsFile()) {
                MpFileIo.DeleteFile(resx_path);
                string resx_cs_path = resx_path.Replace(".resx", ".Designer.cs");
                if (resx_cs_path.IsFile()) {
                    // need to remove code file also
                    MpFileIo.DeleteFile(resx_cs_path);
                }
            }
            using MemoryStream ms = new MemoryStream();
            using ResXResourceWriter oWriter = new ResXResourceWriter(resx_path);

            resx_lookup.ForEach(x => oWriter.AddResource(new ResXDataNode(x.Key, x.Value.value) { Comment = x.Value.comment }));
            oWriter.Generate();
            oWriter.Close();
            return resx_path;
        }

        #region Extensions

        public static IEnumerable<CultureInfo> GetAncestors(this CultureInfo self) {
            var item = self.Parent;

            while (!string.IsNullOrEmpty(item.Name)) {
                yield return item;
                item = item.Parent;
            }
        }
        #endregion
    }
}
