using MonkeyPaste.Common;
using System.IO;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSyntaxThemeHelpers {

        public static string[] SyntaxThemeNames => [
            "a11y-dark",
            "a11y-light",
            "agate",
            "an-old-hope",
            "androidstudio",
            "arduino-light",
            "arta",
            "ascetic",
            "atom-one-dark-reasonable",
            "atom-one-dark",
            "atom-one-light",
            "brown-paper",
            "codepen-embed",
            "color-brewer",
            "dark",
            "default",
            "devibeans",
            "docco",
            "far",
            "felipec",
            "foundation",
            "github-dark-dimmed",
            "github-dark",
            "github",
            "gml",
            "googlecode",
            "gradient-dark",
            "gradient-light",
            "grayscale",
            "hybrid",
            "idea",
            "intellij-light",
            "ir-black",
            "isbl-editor-dark",
            "isbl-editor-light",
            "kimbie-dark",
            "kimbie-light",
            "lightfair",
            "lioshi",
            "magula",
            "monkey-shine",
            "mono-blue",
            "monokai-sublime",
            "monokai",
            "night-owl",
            "nnfx-dark",
            "nnfx-light",
            "nord",
            "obsidian",
            "panda-syntax-dark",
            "panda-syntax-light",
            "paraiso-dark",
            "paraiso-light",
            "pojoaque",
            "purebasic",
            "qtcreator-dark",
            "qtcreator-light",
            "rainbow",
            "routeros",
            "school-book",
            "shades-of-purple",
            "srcery",
            "stackoverflow-dark",
            "stackoverflow-light",
            "sunburst",
            "tokyo-night-dark",
            "tokyo-night-light",
            "tomorrow-night-blue",
            "tomorrow-night-bright",
            "vs",
            "vs2015",
            "xcode",
            "xt256",
        ];

        public static string ReadThemeText(string theme_name) {
            string theme_path =
                Path.Combine(
                    Mp.Services.PlatformInfo.ThemesDir,
                    GetThemeFileName(theme_name));
            if(!theme_path.IsFile()) {
                MpConsole.WriteLine($"Error theme not found: '{theme_name}' at path: '{theme_path}'");
                return string.Empty;
            }
            return MpFileIo.ReadTextFromFile(theme_path);
        }

        private static string GetThemeFileName(string theme_name) {
            return $"{theme_name}.min.css";
        }

        static async Task DownloadAllThemesAsync() {
            string dir = Mp.Services.PlatformInfo.ThemesDir;

            foreach (var theme_name in SyntaxThemeNames) {
                string theme_uri = $"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/{theme_name}.min.css";
                string theme_text = await MpFileIo.ReadTextFromUriAsync(theme_uri);
                if (theme_text.IsNullOrWhiteSpace()) {
                    MpConsole.WriteLine($"Error reading {theme_uri}");
                    continue;
                }
                string theme_path = Path.Combine(dir, GetThemeFileName(theme_name));
                MpFileIo.WriteTextToFile(theme_path, theme_text);
            }
        }
    }
}
