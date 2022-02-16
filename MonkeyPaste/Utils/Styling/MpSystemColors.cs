using System.Collections.Generic;

namespace MonkeyPaste {
    public static class MpSystemColors {
        public static string DarkGray => "#FF696969"; //DimGray
        public static string Gray => "#FFC0C0C0"; //silver
        public static string LightGray => "#FFDCDCDC"; //gainsboro
        public static string Yellow => "#FFFFFF00"; //yellow
        public static string Red => "#FFFF0000"; //red
        public static string Transparent => "#00FFFFFF"; //transparent

        public static string DefaultControlBackground1 => "#FFF0F0F0";
        public static string DefaultControlBackground2 => "#FFE5E5E5";

        public static string IsSelectedBorderColor => Red;
        public static string IsHoveringBorderColor => Yellow;
        public static string IsInactiveBorderColor => Transparent;

        //private static List<List<string>> _contentColors = new List<List<string>> {
        //    //14x7
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(248, 160, 174),
        //                MpColorHelpers.RgbaToHex(243, 69, 68),
        //                MpColorHelpers.RgbaToHex(229, 116, 102),
        //                MpColorHelpers.RgbaToHex(211, 159, 161),
        //                MpColorHelpers.RgbaToHex(191, 53, 50)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(252, 168, 69),
        //                MpColorHelpers.RgbaToHex(251, 108, 40),
        //                MpColorHelpers.RgbaToHex(253, 170, 130),
        //                MpColorHelpers.RgbaToHex(189, 141, 103),
        //                MpColorHelpers.RgbaToHex(177, 86, 55)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(215, 157, 60),
        //                MpColorHelpers.RgbaToHex(168, 123, 82),
        //                MpColorHelpers.RgbaToHex(214, 182, 133),
        //                MpColorHelpers.RgbaToHex(162, 144, 122),
        //                MpColorHelpers.RgbaToHex(123, 85, 72)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(247, 245, 144),
        //                MpColorHelpers.RgbaToHex(252, 240, 78),
        //                MpColorHelpers.RgbaToHex(239, 254, 185),
        //                MpColorHelpers.RgbaToHex(198, 193, 127),
        //                MpColorHelpers.RgbaToHex(224, 200, 42)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(189, 254, 40),
        //                MpColorHelpers.RgbaToHex(143, 254, 115),
        //                MpColorHelpers.RgbaToHex(217, 231, 170),
        //                MpColorHelpers.RgbaToHex(172, 183, 38),
        //                MpColorHelpers.RgbaToHex(140, 157, 45)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(50, 255, 76),
        //                MpColorHelpers.RgbaToHex(68, 199, 33),
        //                MpColorHelpers.RgbaToHex(193, 214, 135),
        //                MpColorHelpers.RgbaToHex(127, 182, 99),
        //                MpColorHelpers.RgbaToHex(92, 170, 58)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(54, 255, 173),
        //                MpColorHelpers.RgbaToHex(32, 195, 178),
        //                MpColorHelpers.RgbaToHex(170, 206, 160),
        //                MpColorHelpers.RgbaToHex(160, 201, 197),
        //                MpColorHelpers.RgbaToHex(32, 159, 148)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(96, 255, 227),
        //                MpColorHelpers.RgbaToHex(46, 238, 249),
        //                MpColorHelpers.RgbaToHex(218, 253, 233),
        //                MpColorHelpers.RgbaToHex(174, 193, 208),
        //                MpColorHelpers.RgbaToHex(40, 103, 146)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(149, 204, 243),
        //                MpColorHelpers.RgbaToHex(43, 167, 237),
        //                MpColorHelpers.RgbaToHex(215, 244, 248),
        //                MpColorHelpers.RgbaToHex(153, 178, 198),
        //                MpColorHelpers.RgbaToHex(30, 51, 160)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(99, 141, 227),
        //                MpColorHelpers.RgbaToHex(22, 127, 193),
        //                MpColorHelpers.RgbaToHex(201, 207, 233),
        //                MpColorHelpers.RgbaToHex(150, 163, 208),
        //                MpColorHelpers.RgbaToHex(52, 89, 170)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(157, 176, 255),
        //                MpColorHelpers.RgbaToHex(148, 127, 220),
        //                MpColorHelpers.RgbaToHex(216, 203, 233),
        //                MpColorHelpers.RgbaToHex(180, 168, 192),
        //                MpColorHelpers.RgbaToHex(109, 90, 179)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(221, 126, 230),
        //                MpColorHelpers.RgbaToHex(186, 141, 200),
        //                MpColorHelpers.RgbaToHex(185, 169, 231),
        //                MpColorHelpers.RgbaToHex(203, 178, 200),
        //                MpColorHelpers.RgbaToHex(170, 90, 179)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(225, 103, 164),
        //                MpColorHelpers.RgbaToHex(252, 74, 210),
        //                MpColorHelpers.RgbaToHex(238, 233, 237),
        //                MpColorHelpers.RgbaToHex(195, 132, 163),
        //                MpColorHelpers.RgbaToHex(205, 60, 117)
        //            },
        //            new List<string> {
        //                MpColorHelpers.RgbaToHex(255, 255, 255),
        //                MpColorHelpers.RgbaToHex(223, 223, 223),
        //                MpColorHelpers.RgbaToHex(187, 187, 187),
        //                MpColorHelpers.RgbaToHex(137, 137, 137),
        //                MpColorHelpers.RgbaToHex(255, 255, 255),
        //            }
        //        };
        //public static List<List<string>> ContentColors => _contentColors;


        public static List<string> ContentColors => new List<string>() {
            "#FFF8A0AE",
            "#FFFCA845",
            "#FFD79D3C",
            "#FFF7F590",
            "#FFBDFE28",
            "#FF32FF4C",
            "#FF36FFAD",
            "#FF60FFE3",
            "#FF95CCF3",
            "#FF638DE3",
            "#FF9DB0FF",
            "#FFDD7EE6",
            "#FFE167A4",
            "#FFFFFFFF",
            "#FFF34544",
            "#FFFB6C28",
            "#FFA87B52",
            "#FFFCF04E",
            "#FF8FFE73",
            "#FF44C721",
            "#FF20C3B2",
            "#FF2EEEF9",
            "#FF2BA7ED",
            "#FF167FC1",
            "#FF947FDC",
            "#FFBA8DC8",
            "#FFFC4AD2",
            "#FFDFDFDF",
            "#FFE57466",
            "#FFFDAA82",
            "#FFD6B685",
            "#FFEFFEB9",
            "#FFD9E7AA",
            "#FFC1D687",
            "#FFAACEA0",
            "#FFDAFDE9",
            "#FFD7F4F8",
            "#FFC9CFE9",
            "#FFD8CBE9",
            "#FFB9A9E7",
            "#FFEEE9ED",
            "#FFBBBBBB",
            "#FFD39FA1",
            "#FFBD8D67",
            "#FFA2907A",
            "#FFC6C17F",
            "#FFACB726",
            "#FF7FB663",
            "#FFA0C9C5",
            "#FFAEC1D0",
            "#FF99B2C6",
            "#FF96A3D0",
            "#FFB4A8C0",
            "#FFCBB2C8",
            "#FFC384A3",
            "#FF898989",
            "#FFBF3532",
            "#FFB15637",
            "#FF7B5548",
            "#FFE0C82A",
            "#FF8C9D2D",
            "#FF5CAA3A",
            "#FF209F94",
            "#FF286792",
            "#FF1E33A0",
            "#FF3459AA",
            "#FF6D5AB3",
            "#FFAA5AB3",
            "#FFCD3C75",
            "#FF414141"
        };
    }
}
