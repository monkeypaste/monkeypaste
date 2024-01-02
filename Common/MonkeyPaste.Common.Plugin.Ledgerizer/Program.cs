using Newtonsoft.Json;

namespace MonkeyPaste.Common.Plugin.Ledgerizer {
    internal class Program {
        static string[] Plugins = new[] {
            "ChatGpt",
            "ComputerVision",
            "FileConverter",
            "ImageAnnotator",
            "QrCoder",
            "WebSearch"
        };
        static void Main(string[] args) {
            string ledger_text =
                MpJsonConverter.SerializeObject(
                    new MpManifestLedger() {
                        manifests = Plugins
                                    .Select(x =>
                                        Path.Combine(
                                        MpCommonHelpers.GetSolutionDir(),
                                        "Plugins",
                                        x,
                                        "manifest.json"))
                                    .Select(x => MpFileIo.ReadTextFromFile(x))
                                    .Select(x => MpJsonConverter.DeserializeObject<MpManifestFormat>(x))
                                    .ToList()
                    }
                ,
                new JsonSerializerSettings() {
                    NullValueHandling = NullValueHandling.Ignore
                }).ToPrettyPrintJson();
            MpFileIo.WriteTextToFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\Ledger\ledger.json", ledger_text);

        }
    }
}
