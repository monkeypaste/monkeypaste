using MonkeyPaste.Avalonia;
using System.Diagnostics;
using System.IO.Compression;

namespace MonkeyPaste.Common.Plugin.Ledgerizer {
    internal class Program {


        const string BUILD_CONFIG =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
        static string[] Plugins = [
            "ChatGpt",
            "ComputerVision",
            "FileConverter",
            "ImageAnnotator",
            "MinimalExample",
            "QrCoder",
            "TextToSpeech",
            "TextTranslator",
            "WebSearch"
        ];
        static bool LEDGERIZE_LOCAL = true;

        static string LocalLedgerDir = MpLedgerConstants.LOCAL_LEDGER_DIR;

        static string LedgerPath =
            Path.Combine(
                LocalLedgerDir,
                LEDGERIZE_LOCAL ?
                    MpLedgerConstants.LOCAL_LEDGER_NAME :
                    MpLedgerConstants.REMOTE_LEDGER_NAME);

        static string LocalReleasesDir = MpLedgerConstants.LOCAL_RELEASE_DIR;

        static void Main(string[] args) {
            MpFileIo.DeleteDirectory(LocalReleasesDir);

            MpManifestLedger ledger = new MpManifestLedger();
            foreach (var plugin in Plugins) {
                string mfp = Path.Combine(
                                        MpCommonHelpers.GetSolutionDir(),
                                        "Plugins",
                                        plugin,
                                        "manifest.json");
                string mft = MpFileIo.ReadTextFromFile(mfp);
                var mf = mft.DeserializeObject<MpManifestFormat>();

                if (LEDGERIZE_LOCAL) {
                    mf.packageUrl = PublishPlugin(Path.GetDirectoryName(mfp));
                    if (mf.packageUrl == null) {
                        continue;
                    }
                }
                ledger.manifests.Add(mf);
            }
            string ledger_text = ledger.SerializeObjectOmitNulls().ToPrettyPrintJson();
            MpFileIo.WriteTextToFile(LedgerPath, ledger_text);
            Console.WriteLine($"Ledger written to: ");
            Console.WriteLine(LedgerPath);
        }

        static string PublishPlugin(string projDir) {
            if (!LocalReleasesDir.IsDirectory()) {
                MpFileIo.CreateDirectory(LocalReleasesDir);
            }
            string publish_dir = Path.Combine(LocalReleasesDir, Path.GetFileName(projDir));

            if (CanBuild(projDir)) {
                MpFileIo.DeleteDirectory(Path.Combine(projDir, "bin"));
                MpFileIo.DeleteDirectory(Path.Combine(projDir, "obj"));
                var proc = new Process();
                proc.StartInfo.FileName = @"C:\Program Files\dotnet\dotnet.exe";
                proc.StartInfo.Arguments = $"publish --configuration {BUILD_CONFIG} --output {publish_dir}";
                proc.StartInfo.WorkingDirectory = projDir;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                string proc_output = proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();
                int exit_code = proc.ExitCode;
                proc.Close();

                if (exit_code != 0) {
                    Console.WriteLine("");
                    Console.WriteLine($"Error from '{Path.GetFileName(projDir)}' exit code '{exit_code}'");
                    Console.WriteLine(proc_output);
                    Console.WriteLine("");
                    return null;
                }
            } else if (Path.Combine(projDir, "bin", BUILD_CONFIG) is string build_dir &&
                        build_dir.IsDirectory()) {
                if (publish_dir.IsDirectory()) {
                    MpFileIo.DeleteDirectory(publish_dir);
                }
                MpFileIo.CreateDirectory(publish_dir);
                new DirectoryInfo(build_dir).CopyContents(new DirectoryInfo(publish_dir), true, true);
            } else {
                return null;
            }

            if (!publish_dir.IsDirectory()) {
                return null;
            }
            string output_path = Path.Combine(LocalReleasesDir, $"{Path.GetFileName(projDir)}.zip");
            ZipFile.CreateFromDirectory(publish_dir, output_path, CompressionLevel.Fastest, true);
            MpFileIo.DeleteDirectory(publish_dir);
            Console.WriteLine(output_path + " DONE");

            return output_path.ToFileSystemUriFromPath();
        }

        static bool CanBuild(string projDir) {
            string dot_net_match_str = @"<Project Sdk=""Microsoft.NET.Sdk"">";

            if (Directory.GetFiles(projDir).FirstOrDefault(x => x.EndsWith(".csproj")) is string proj_path &&
                !MpFileIo.ReadTextFromFile(proj_path).Contains(dot_net_match_str)) {
                return false;
            }
            return true;
        }
        static (string, string) GetProcessArgs(string projDir, string publish_dir) {
            string dot_net_match_str = @"<Project Sdk=""Microsoft.NET.Sdk"">";

            if (Directory.GetFiles(projDir).FirstOrDefault(x => x.EndsWith(".csproj")) is string proj_path &&
                !MpFileIo.ReadTextFromFile(proj_path).Contains(dot_net_match_str)) {
                return (
                    @"C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe",
                    @$"/p:OutDir={publish_dir}"
                    );
            }
            return (
                    @"C:\Program Files\dotnet\dotnet.exe",
                    $"publish --configuration Release --output {publish_dir}"
                    );
        }
    }
}
