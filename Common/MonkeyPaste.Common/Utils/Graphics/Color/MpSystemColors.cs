using System.Collections.Generic;

namespace MonkeyPaste.Common {
    public static class MpSystemColors {
        public static string White => "#FFFFFFFF";
        public static string DarkGray => "#FF696969"; //DimGray
        public static string Gray => "#FFC0C0C0"; //silver
        public static string LightGray => "#FFDCDCDC"; //gainsboro
        public static string Yellow => "#FFFFFF00"; //yellow
        public static string Red => "#FFFF0000"; //red

        public static string Transparent => "#00FFFFFF"; //transparent
        public static string Black => "#FF000000";

        public const int COLOR_PALETTE_ROWS = 5;
        public const int COLOR_PALETTE_COLS = 14;
        public static List<string> ContentColors => new List<string>() {
            // NOTE is 5x14 
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
            "#00414141"
        };

        #region X11 Colors
        //from https://www.w3schools.com/colors/colors_x11.asp
        public static List<string> X11ColorNames => new List<string>() {
            "aliceblue",
            "antiquewhite",
            "antiquewhite1",
            "antiquewhite2",
            "antiquewhite3",
            "antiquewhite4",
            "aquamarine1",
            "aquamarine2",
            "aquamarine4",
            "azure1",
            "azure2",
            "azure3",
            "azure4",
            "beige",
            "bisque1",
            "bisque2",
            "bisque3",
            "bisque4",
            "black",
            "blanchedalmond",
            "blue1",
            "blue2",
            "blue4",
            "blueviolet",
            "brown",
            "brown1",
            "brown2",
            "brown3",
            "brown4",
            "burlywood",
            "burlywood1",
            "burlywood2",
            "burlywood3",
            "burlywood4",
            "cadetblue",
            "cadetblue1",
            "cadetblue2",
            "cadetblue3",
            "cadetblue4",
            "chartreuse1",
            "chartreuse2",
            "chartreuse3",
            "chartreuse4",
            "chocolate",
            "chocolate1",
            "chocolate2",
            "chocolate3",
            "coral",
            "coral1",
            "coral2",
            "coral3",
            "coral4",
            "cornflowerblue",
            "cornsilk1",
            "cornsilk2",
            "cornsilk3",
            "cornsilk4",
            "cyan1",
            "cyan2",
            "cyan3",
            "cyan4",
            "darkgoldenrod",
            "darkgoldenrod1",
            "darkgoldenrod2",
            "darkgoldenrod3",
            "darkgoldenrod4",
            "darkgreen",
            "darkkhaki",
            "darkolivegreen",
            "darkolivegreen1",
            "darkolivegreen2",
            "darkolivegreen3",
            "darkolivegreen4",
            "darkorange",
            "darkorange1",
            "darkorange2",
            "darkorange3",
            "darkorange4",
            "darkorchid",
            "darkorchid1",
            "darkorchid2",
            "darkorchid3",
            "darkorchid4",
            "darksalmon",
            "darkseagreen",
            "darkseagreen1",
            "darkseagreen2",
            "darkseagreen3",
            "darkseagreen4",
            "darkslateblue",
            "darkslategray",
            "darkslategray1",
            "darkslategray2",
            "darkslategray3",
            "darkslategray4",
            "darkturquoise",
            "darkviolet",
            "deeppink1",
            "deeppink2",
            "deeppink3",
            "deeppink4",
            "deepskyblue1",
            "deepskyblue2",
            "deepskyblue3",
            "deepskyblue4",
            "dimgray",
            "dodgerblue1",
            "dodgerblue2",
            "dodgerblue3",
            "dodgerblue4",
            "firebrick",
            "firebrick1",
            "firebrick2",
            "firebrick3",
            "firebrick4",
            "floralwhite",
            "forestgreen",
            "gainsboro",
            "ghostwhite",
            "gold1",
            "gold2",
            "gold3",
            "gold4",
            "goldenrod",
            "goldenrod1",
            "goldenrod2",
            "goldenrod3",
            "goldenrod4",
            "gray",
            "gray1",
            "gray2",
            "gray3",
            "gray4",
            "gray5",
            "gray6",
            "gray7",
            "gray8",
            "gray9",
            "gray10",
            "gray11",
            "gray12",
            "gray13",
            "gray14",
            "gray15",
            "gray16",
            "gray17",
            "gray18",
            "gray19",
            "gray20",
            "gray21",
            "gray22",
            "gray23",
            "gray24",
            "gray25",
            "gray26",
            "gray27",
            "gray28",
            "gray29",
            "gray30",
            "gray31",
            "gray32",
            "gray33",
            "gray34",
            "gray35",
            "gray36",
            "gray37",
            "gray38",
            "gray39",
            "gray40",
            "gray41",
            "gray42",
            "gray43",
            "gray44",
            "gray45",
            "gray46",
            "gray47",
            "gray48",
            "gray49",
            "gray50",
            "gray51",
            "gray52",
            "gray53",
            "gray54",
            "gray55",
            "gray56",
            "gray57",
            "gray58",
            "gray59",
            "gray60",
            "gray61",
            "gray62",
            "gray63",
            "gray64",
            "gray65",
            "gray66",
            "gray67",
            "gray68",
            "gray69",
            "gray70",
            "gray71",
            "gray72",
            "gray73",
            "gray74",
            "gray75",
            "gray76",
            "gray77",
            "gray78",
            "gray79",
            "gray80",
            "gray81",
            "gray82",
            "gray83",
            "gray84",
            "gray85",
            "gray86",
            "gray87",
            "gray88",
            "gray89",
            "gray90",
            "gray91",
            "gray92",
            "gray93",
            "gray94",
            "gray95",
            "gray97",
            "gray98",
            "gray99",
            "green1",
            "green2",
            "green3",
            "green4",
            "greenyellow",
            "honeydew1",
            "honeydew2",
            "honeydew3",
            "honeydew4",
            "hotpink",
            "hotpink1",
            "hotpink2",
            "hotpink3",
            "hotpink4",
            "indianred",
            "indianred1",
            "indianred2",
            "indianred3",
            "indianred4",
            "ivory1",
            "ivory2",
            "ivory3",
            "ivory4",
            "khaki",
            "khaki1",
            "khaki2",
            "khaki3",
            "khaki4",
            "lavender",
            "lavenderblush1",
            "lavenderblush2",
            "lavenderblush3",
            "lavenderblush4",
            "lawngreen",
            "lemonchiffon1",
            "lemonchiffon2",
            "lemonchiffon3",
            "lemonchiffon4",
            "light",
            "lightblue",
            "lightblue1",
            "lightblue2",
            "lightblue3",
            "lightblue4",
            "lightcoral",
            "lightcyan1",
            "lightcyan2",
            "lightcyan3",
            "lightcyan4",
            "lightgoldenrod1",
            "lightgoldenrod2",
            "lightgoldenrod3",
            "lightgoldenrod4",
            "lightgoldenrodyellow",
            "lightgray",
            "lightpink",
            "lightpink1",
            "lightpink2",
            "lightpink3",
            "lightpink4",
            "lightsalmon1",
            "lightsalmon2",
            "lightsalmon3",
            "lightsalmon4",
            "lightseagreen",
            "lightskyblue",
            "lightskyblue1",
            "lightskyblue2",
            "lightskyblue3",
            "lightskyblue4",
            "lightslateblue",
            "lightslategray",
            "lightsteelblue",
            "lightsteelblue1",
            "lightsteelblue2",
            "lightsteelblue3",
            "lightsteelblue4",
            "lightyellow1",
            "lightyellow2",
            "lightyellow3",
            "lightyellow4",
            "limegreen",
            "linen",
            "magenta",
            "magenta2",
            "magenta3",
            "magenta4",
            "maroon",
            "maroon1",
            "maroon2",
            "maroon3",
            "maroon4",
            "medium",
            "mediumaquamarine",
            "mediumblue",
            "mediumorchid",
            "mediumorchid1",
            "mediumorchid2",
            "mediumorchid3",
            "mediumorchid4",
            "mediumpurple",
            "mediumpurple1",
            "mediumpurple2",
            "mediumpurple3",
            "mediumpurple4",
            "mediumseagreen",
            "mediumslateblue",
            "mediumspringgreen",
            "mediumturquoise",
            "mediumvioletred",
            "midnightblue",
            "mintcream",
            "mistyrose1",
            "mistyrose2",
            "mistyrose3",
            "mistyrose4",
            "moccasin",
            "navajowhite1",
            "navajowhite2",
            "navajowhite3",
            "navajowhite4",
            "navyblue",
            "oldlace",
            "olivedrab",
            "olivedrab1",
            "olivedrab2",
            "olivedrab4",
            "orange",
            "orange2",
            "orange3",
            "orange4",
            "orangered1",
            "orangered2",
            "orangered3",
            "orangered4",
            "orchid",
            "orchid1",
            "orchid2",
            "orchid3",
            "orchid4",
            "pale",
            "palegoldenrod",
            "palegreen",
            "palegreen1",
            "palegreen2",
            "palegreen3",
            "palegreen4",
            "paleturquoise",
            "paleturquoise1",
            "paleturquoise2",
            "paleturquoise3",
            "paleturquoise4",
            "palevioletred",
            "palevioletred1",
            "palevioletred2",
            "palevioletred3",
            "palevioletred4",
            "papayawhip",
            "peachpuff1",
            "peachpuff2",
            "peachpuff3",
            "peachpuff4",
            "pink",
            "pink1",
            "pink2",
            "pink3",
            "pink4",
            "plum",
            "plum1",
            "plum2",
            "plum3",
            "plum4",
            "powderblue",
            "purple",
            "purple1",
            "purple2",
            "purple3",
            "purple4",
            "red1",
            "red2",
            "red3",
            "red4",
            "rosybrown",
            "rosybrown1",
            "rosybrown2",
            "rosybrown3",
            "rosybrown4",
            "royalblue",
            "royalblue1",
            "royalblue2",
            "royalblue3",
            "royalblue4",
            "saddlebrown",
            "salmon",
            "salmon1",
            "salmon2",
            "salmon3",
            "salmon4",
            "sandybrown",
            "seagreen1",
            "seagreen2",
            "seagreen3",
            "seagreen4",
            "seashell1",
            "seashell2",
            "seashell3",
            "seashell4",
            "sienna",
            "sienna1",
            "sienna2",
            "sienna3",
            "sienna4",
            "skyblue",
            "skyblue1",
            "skyblue2",
            "skyblue3",
            "skyblue4",
            "slateblue",
            "slateblue1",
            "slateblue2",
            "slateblue3",
            "slateblue4",
            "slategray",
            "slategray1",
            "slategray2",
            "slategray3",
            "slategray4",
            "snow1",
            "snow2",
            "snow3",
            "snow4",
            "springgreen1",
            "springgreen2",
            "springgreen3",
            "springgreen4",
            "steelblue",
            "steelblue1",
            "steelblue2",
            "steelblue3",
            "steelblue4",
            "tan",
            "tan1",
            "tan2",
            "tan3",
            "tan4",
            "thistle",
            "thistle1",
            "thistle2",
            "thistle3",
            "thistle4",
            "tomato1",
            "tomato2",
            "tomato3",
            "tomato4",
            "turquoise",
            "turquoise1",
            "turquoise2",
            "turquoise3",
            "turquoise4",
            "violet",
            "violetred",
            "violetred1",
            "violetred2",
            "violetred3",
            "violetred4",
            "wheat",
            "wheat1",
            "wheat2",
            "wheat3",
            "wheat4",
            "white",
            "whitesmoke",
            "yellow1",
            "yellow2",
            "yellow3",
            "yellow4",
            "yellowgreen"
        };

        public static string aliceblue => "#FFF0F8FF";
        public static string antiquewhite => "#FFFAEBD7";
        public static string antiquewhite1 => "#FFFFEFDB";
        public static string antiquewhite2 => "#FFEEDFCC";
        public static string antiquewhite3 => "#FFCDC0B0";
        public static string antiquewhite4 => "#FF8B8378";
        public static string aquamarine1 => "#FF7FFFD4";
        public static string aquamarine2 => "#FF76EEC6";
        public static string aquamarine4 => "#FF458B74";
        public static string azure1 => "#FFF0FFFF";
        public static string azure2 => "#FFE0EEEE";
        public static string azure3 => "#FFC1CDCD";
        public static string azure4 => "#FF838B8B";
        public static string beige => "#FFF5F5DC";
        public static string bisque1 => "#FFFFE4C4";
        public static string bisque2 => "#FFEED5B7";
        public static string bisque3 => "#FFCDB79E";
        public static string bisque4 => "#FF8B7D6B";
        public static string black => "#FF040404";
        public static string blanchedalmond => "#FFFFEBCD";
        public static string blue1 => "#FF0000FF";
        public static string blue2 => "#FF0000EE";
        public static string blue4 => "#FF00008B";
        public static string blueviolet => "#FF8A2BE2";
        public static string brown => "#FFA52A2A";
        public static string brown1 => "#FFFF4040";
        public static string brown2 => "#FFEE3B3B";
        public static string brown3 => "#FFCD3333";
        public static string brown4 => "#FF8B2323";
        public static string burlywood => "#FFDEB887";
        public static string burlywood1 => "#FFFFD39B";
        public static string burlywood2 => "#FFEEC591";
        public static string burlywood3 => "#FFCDAA7D";
        public static string burlywood4 => "#FF8B7355";
        public static string cadetblue => "#FF5F9EA0";
        public static string cadetblue1 => "#FF98F5FF";
        public static string cadetblue2 => "#FF8EE5EE";
        public static string cadetblue3 => "#FF7AC5CD";
        public static string cadetblue4 => "#FF53868B";
        public static string chartreuse1 => "#FF7FFF00";
        public static string chartreuse2 => "#FF76EE00";
        public static string chartreuse3 => "#FF66CD00";
        public static string chartreuse4 => "#FF458B00";
        public static string chocolate => "#FFD2691E";
        public static string chocolate1 => "#FFFF7F24";
        public static string chocolate2 => "#FFEE7621";
        public static string chocolate3 => "#FFCD661D";
        public static string coral => "#FFFF7F50";
        public static string coral1 => "#FFFF7256";
        public static string coral2 => "#FFEE6A50";
        public static string coral3 => "#FFCD5B45";
        public static string coral4 => "#FF8B3E2F";
        public static string cornflowerblue => "#FF6495ED";
        public static string cornsilk1 => "#FFFFF8DC";
        public static string cornsilk2 => "#FFEEE8CD";
        public static string cornsilk3 => "#FFCDC8B1";
        public static string cornsilk4 => "#FF8B8878";
        public static string cyan1 => "#FF00FFFF";
        public static string cyan2 => "#FF00EEEE";
        public static string cyan3 => "#FF00CDCD";
        public static string cyan4 => "#FF008B8B";
        public static string darkgoldenrod => "#FFB8860B";
        public static string darkgoldenrod1 => "#FFFFB90F";
        public static string darkgoldenrod2 => "#FFEEAD0E";
        public static string darkgoldenrod3 => "#FFCD950C";
        public static string darkgoldenrod4 => "#FF8B6508";
        public static string darkgreen => "#FF006400";
        public static string darkkhaki => "#FFBDB76B";
        public static string darkolivegreen => "#FF556B2F";
        public static string darkolivegreen1 => "#FFCAFF70";
        public static string darkolivegreen2 => "#FFBCEE68";
        public static string darkolivegreen3 => "#FFA2CD5A";
        public static string darkolivegreen4 => "#FF6E8B3D";
        public static string darkorange => "#FFFF8C00";
        public static string darkorange1 => "#FFFF7F00";
        public static string darkorange2 => "#FFEE7600";
        public static string darkorange3 => "#FFCD6600";
        public static string darkorange4 => "#FF8B4500";
        public static string darkorchid => "#FF9932CC";
        public static string darkorchid1 => "#FFBF3EFF";
        public static string darkorchid2 => "#FFB23AEE";
        public static string darkorchid3 => "#FF9A32CD";
        public static string darkorchid4 => "#FF68228B";
        public static string darksalmon => "#FFE9967A";
        public static string darkseagreen => "#FF8FBC8F";
        public static string darkseagreen1 => "#FFC1FFC1";
        public static string darkseagreen2 => "#FFB4EEB4";
        public static string darkseagreen3 => "#FF9BCD9B";
        public static string darkseagreen4 => "#FF698B69";
        public static string darkslateblue => "#FF483D8B";
        public static string darkslategray => "#FF2F4F4F";
        public static string darkslategray1 => "#FF97FFFF";
        public static string darkslategray2 => "#FF8DEEEE";
        public static string darkslategray3 => "#FF79CDCD";
        public static string darkslategray4 => "#FF528B8B";
        public static string darkturquoise => "#FF00CED1";
        public static string darkviolet => "#FF9400D3";
        public static string deeppink1 => "#FFFF1493";
        public static string deeppink2 => "#FFEE1289";
        public static string deeppink3 => "#FFCD1076";
        public static string deeppink4 => "#FF8B0A50";
        public static string deepskyblue1 => "#FF00BFFF";
        public static string deepskyblue2 => "#FF00B2EE";
        public static string deepskyblue3 => "#FF009ACD";
        public static string deepskyblue4 => "#FF00688B";
        public static string dimgray => "#FF696969";
        public static string dodgerblue1 => "#FF1E90FF";
        public static string dodgerblue2 => "#FF1C86EE";
        public static string dodgerblue3 => "#FF1874CD";
        public static string dodgerblue4 => "#FF104E8B";
        public static string firebrick => "#FFB22222";
        public static string firebrick1 => "#FFFF3030";
        public static string firebrick2 => "#FFEE2C2C";
        public static string firebrick3 => "#FFCD2626";
        public static string firebrick4 => "#FF8B1A1A";
        public static string floralwhite => "#FFFFFAF0";
        public static string forestgreen => "#FF228B22";
        public static string gainsboro => "#FFDCDCDC";
        public static string ghostwhite => "#FFF8F8FF";
        public static string gold1 => "#FFFFD700";
        public static string gold2 => "#FFEEC900";
        public static string gold3 => "#FFCDAD00";
        public static string gold4 => "#FF8B7500";
        public static string goldenrod => "#FFDAA520";
        public static string goldenrod1 => "#FFFFC125";
        public static string goldenrod2 => "#FFEEB422";
        public static string goldenrod3 => "#FFCD9B1D";
        public static string goldenrod4 => "#FF8B6914";
        public static string gray => "#FFBEBEBE";
        public static string gray1 => "#FF000000";
        public static string gray2 => "#FF050505";
        public static string gray3 => "#FF080808";
        public static string gray4 => "#FF0A0A0A";
        public static string gray5 => "#FF0D0D0D";
        public static string gray6 => "#FF0F0F0F";
        public static string gray7 => "#FF121212";
        public static string gray8 => "#FF141414";
        public static string gray9 => "#FF171717";
        public static string gray10 => "#FF1A1A1A";
        public static string gray11 => "#FF1C1C1C";
        public static string gray12 => "#FF1F1F1F";
        public static string gray13 => "#FF212121";
        public static string gray14 => "#FF242424";
        public static string gray15 => "#FF262626";
        public static string gray16 => "#FF292929";
        public static string gray17 => "#FF2B2B2B";
        public static string gray18 => "#FF2E2E2E";
        public static string gray19 => "#FF303030";
        public static string gray20 => "#FF333333";
        public static string gray21 => "#FF363636";
        public static string gray22 => "#FF383838";
        public static string gray23 => "#FF3B3B3B";
        public static string gray24 => "#FF3D3D3D";
        public static string gray25 => "#FF404040";
        public static string gray26 => "#FF424242";
        public static string gray27 => "#FF454545";
        public static string gray28 => "#FF474747";
        public static string gray29 => "#FF4A4A4A";
        public static string gray30 => "#FF4D4D4D";
        public static string gray31 => "#FF4F4F4F";
        public static string gray32 => "#FF525252";
        public static string gray33 => "#FF545454";
        public static string gray34 => "#FF575757";
        public static string gray35 => "#FF595959";
        public static string gray36 => "#FF5C5C5C";
        public static string gray37 => "#FF5E5E5E";
        public static string gray38 => "#FF616161";
        public static string gray39 => "#FF636363";
        public static string gray40 => "#FF666666";
        public static string gray41 => "#FF696969";
        public static string gray42 => "#FF6B6B6B";
        public static string gray43 => "#FF6E6E6E";
        public static string gray44 => "#FF707070";
        public static string gray45 => "#FF737373";
        public static string gray46 => "#FF757575";
        public static string gray47 => "#FF787878";
        public static string gray48 => "#FF7A7A7A";
        public static string gray49 => "#FF7D7D7D";
        public static string gray50 => "#FF7F7F7F";
        public static string gray51 => "#FF828282";
        public static string gray52 => "#FF858585";
        public static string gray53 => "#FF878787";
        public static string gray54 => "#FF8A8A8A";
        public static string gray55 => "#FF8C8C8C";
        public static string gray56 => "#FF8F8F8F";
        public static string gray57 => "#FF919191";
        public static string gray58 => "#FF949494";
        public static string gray59 => "#FF969696";
        public static string gray60 => "#FF999999";
        public static string gray61 => "#FF9C9C9C";
        public static string gray62 => "#FF9E9E9E";
        public static string gray63 => "#FFA1A1A1";
        public static string gray64 => "#FFA3A3A3";
        public static string gray65 => "#FFA6A6A6";
        public static string gray66 => "#FFA8A8A8";
        public static string gray67 => "#FFABABAB";
        public static string gray68 => "#FFADADAD";
        public static string gray69 => "#FFB0B0B0";
        public static string gray70 => "#FFB3B3B3";
        public static string gray71 => "#FFB5B5B5";
        public static string gray72 => "#FFB8B8B8";
        public static string gray73 => "#FFBABABA";
        public static string gray74 => "#FFBDBDBD";
        public static string gray75 => "#FFBFBFBF";
        public static string gray76 => "#FFC2C2C2";
        public static string gray77 => "#FFC4C4C4";
        public static string gray78 => "#FFC7C7C7";
        public static string gray79 => "#FFC9C9C9";
        public static string gray80 => "#FFCCCCCC";
        public static string gray81 => "#FFCFCFCF";
        public static string gray82 => "#FFD1D1D1";
        public static string gray83 => "#FFD4D4D4";
        public static string gray84 => "#FFD6D6D6";
        public static string gray85 => "#FFD9D9D9";
        public static string gray86 => "#FFDBDBDB";
        public static string gray87 => "#FFDEDEDE";
        public static string gray88 => "#FFE0E0E0";
        public static string gray89 => "#FFE3E3E3";
        public static string gray90 => "#FFE5E5E5";
        public static string gray91 => "#FFE8E8E8";
        public static string gray92 => "#FFEBEBEB";
        public static string gray93 => "#FFEDEDED";
        public static string gray94 => "#FFF0F0F0";
        public static string gray95 => "#FFF2F2F2";
        public static string gray97 => "#FFF7F7F7";
        public static string gray98 => "#FFFAFAFA";
        public static string gray99 => "#FFFCFCFC";
        public static string green1 => "#FF00FF00";
        public static string green2 => "#FF00EE00";
        public static string green3 => "#FF00CD00";
        public static string green4 => "#FF008B00";
        public static string greenyellow => "#FFADFF2F";
        public static string honeydew1 => "#FFF0FFF0";
        public static string honeydew2 => "#FFE0EEE0";
        public static string honeydew3 => "#FFC1CDC1";
        public static string honeydew4 => "#FF838B83";
        public static string hotpink => "#FFFF69B4";
        public static string hotpink1 => "#FFFF6EB4";
        public static string hotpink2 => "#FFEE6AA7";
        public static string hotpink3 => "#FFCD6090";
        public static string hotpink4 => "#FF8B3A62";
        public static string indianred => "#FFCD5C5C";
        public static string indianred1 => "#FFFF6A6A";
        public static string indianred2 => "#FFEE6363";
        public static string indianred3 => "#FFCD5555";
        public static string indianred4 => "#FF8B3A3A";
        public static string ivory1 => "#FFFFFFF0";
        public static string ivory2 => "#FFEEEEE0";
        public static string ivory3 => "#FFCDCDC1";
        public static string ivory4 => "#FF8B8B83";
        public static string khaki => "#FFF0E68C";
        public static string khaki1 => "#FFFFF68F";
        public static string khaki2 => "#FFEEE685";
        public static string khaki3 => "#FFCDC673";
        public static string khaki4 => "#FF8B864E";
        public static string lavender => "#FFE6E6FA";
        public static string lavenderblush1 => "#FFFFF0F5";
        public static string lavenderblush2 => "#FFEEE0E5";
        public static string lavenderblush3 => "#FFCDC1C5";
        public static string lavenderblush4 => "#FF8B8386";
        public static string lawngreen => "#FF7CFC00";
        public static string lemonchiffon1 => "#FFFFFACD";
        public static string lemonchiffon2 => "#FFEEE9BF";
        public static string lemonchiffon3 => "#FFCDC9A5";
        public static string lemonchiffon4 => "#FF8B8970";
        public static string light => "#FFEEDD82";
        public static string lightblue => "#FFADD8E6";
        public static string lightblue1 => "#FFBFEFFF";
        public static string lightblue2 => "#FFB2DFEE";
        public static string lightblue3 => "#FF9AC0CD";
        public static string lightblue4 => "#FF68838B";
        public static string lightcoral => "#FFF08080";
        public static string lightcyan1 => "#FFE0FFFF";
        public static string lightcyan2 => "#FFD1EEEE";
        public static string lightcyan3 => "#FFB4CDCD";
        public static string lightcyan4 => "#FF7A8B8B";
        public static string lightgoldenrod1 => "#FFFFEC8B";
        public static string lightgoldenrod2 => "#FFEEDC82";
        public static string lightgoldenrod3 => "#FFCDBE70";
        public static string lightgoldenrod4 => "#FF8B814C";
        public static string lightgoldenrodyellow => "#FFFAFAD2";
        public static string lightgray => "#FFD3D3D3";
        public static string lightpink => "#FFFFB6C1";
        public static string lightpink1 => "#FFFFAEB9";
        public static string lightpink2 => "#FFEEA2AD";
        public static string lightpink3 => "#FFCD8C95";
        public static string lightpink4 => "#FF8B5F65";
        public static string lightsalmon1 => "#FFFFA07A";
        public static string lightsalmon2 => "#FFEE9572";
        public static string lightsalmon3 => "#FFCD8162";
        public static string lightsalmon4 => "#FF8B5742";
        public static string lightseagreen => "#FF20B2AA";
        public static string lightskyblue => "#FF87CEFA";
        public static string lightskyblue1 => "#FFB0E2FF";
        public static string lightskyblue2 => "#FFA4D3EE";
        public static string lightskyblue3 => "#FF8DB6CD";
        public static string lightskyblue4 => "#FF607B8B";
        public static string lightslateblue => "#FF8470FF";
        public static string lightslategray => "#FF778899";
        public static string lightsteelblue => "#FFB0C4DE";
        public static string lightsteelblue1 => "#FFCAE1FF";
        public static string lightsteelblue2 => "#FFBCD2EE";
        public static string lightsteelblue3 => "#FFA2B5CD";
        public static string lightsteelblue4 => "#FF6E7B8B";
        public static string lightyellow1 => "#FFFFFFE0";
        public static string lightyellow2 => "#FFEEEED1";
        public static string lightyellow3 => "#FFCDCDB4";
        public static string lightyellow4 => "#FF8B8B7A";
        public static string limegreen => "#FF32CD32";
        public static string linen => "#FFFAF0E6";
        public static string magenta => "#FFFF00FF";
        public static string magenta2 => "#FFEE00EE";
        public static string magenta3 => "#FFCD00CD";
        public static string magenta4 => "#FF8B008B";
        public static string maroon => "#FFB03060";
        public static string maroon1 => "#FFFF34B3";
        public static string maroon2 => "#FFEE30A7";
        public static string maroon3 => "#FFCD2990";
        public static string maroon4 => "#FF8B1C62";
        public static string medium => "#FF66CDAA";
        public static string mediumaquamarine => "#FF66CDAA";
        public static string mediumblue => "#FF0000CD";
        public static string mediumorchid => "#FFBA55D3";
        public static string mediumorchid1 => "#FFE066FF";
        public static string mediumorchid2 => "#FFD15FEE";
        public static string mediumorchid3 => "#FFB452CD";
        public static string mediumorchid4 => "#FF7A378B";
        public static string mediumpurple => "#FF9370DB";
        public static string mediumpurple1 => "#FFAB82FF";
        public static string mediumpurple2 => "#FF9F79EE";
        public static string mediumpurple3 => "#FF8968CD";
        public static string mediumpurple4 => "#FF5D478B";
        public static string mediumseagreen => "#FF3CB371";
        public static string mediumslateblue => "#FF7B68EE";
        public static string mediumspringgreen => "#FF00FA9A";
        public static string mediumturquoise => "#FF48D1CC";
        public static string mediumvioletred => "#FFC71585";
        public static string midnightblue => "#FF191970";
        public static string mintcream => "#FFF5FFFA";
        public static string mistyrose1 => "#FFFFE4E1";
        public static string mistyrose2 => "#FFEED5D2";
        public static string mistyrose3 => "#FFCDB7B5";
        public static string mistyrose4 => "#FF8B7D7B";
        public static string moccasin => "#FFFFE4B5";
        public static string navajowhite1 => "#FFFFDEAD";
        public static string navajowhite2 => "#FFEECFA1";
        public static string navajowhite3 => "#FFCDB38B";
        public static string navajowhite4 => "#FF8B795E";
        public static string navyblue => "#FF000080";
        public static string oldlace => "#FFFDF5E6";
        public static string olivedrab => "#FF6B8E23";
        public static string olivedrab1 => "#FFC0FF3E";
        public static string olivedrab2 => "#FFB3EE3A";
        public static string olivedrab4 => "#FF698B22";
        public static string orange1 => "#FFFFA500";
        public static string orange2 => "#FFEE9A00";
        public static string orange3 => "#FFCD8500";
        public static string orange4 => "#FF8B5A00";
        public static string orangered1 => "#FFFF4500";
        public static string orangered2 => "#FFEE4000";
        public static string orangered3 => "#FFCD3700";
        public static string orangered4 => "#FF8B2500";
        public static string orchid => "#FFDA70D6";
        public static string orchid1 => "#FFFF83FA";
        public static string orchid2 => "#FFEE7AE9";
        public static string orchid3 => "#FFCD69C9";
        public static string orchid4 => "#FF8B4789";
        public static string pale => "#FFDB7093";
        public static string palegoldenrod => "#FFEEE8AA";
        public static string palegreen => "#FF98FB98";
        public static string palegreen1 => "#FF9AFF9A";
        public static string palegreen2 => "#FF90EE90";
        public static string palegreen3 => "#FF7CCD7C";
        public static string palegreen4 => "#FF548B54";
        public static string paleturquoise => "#FFAFEEEE";
        public static string paleturquoise1 => "#FFBBFFFF";
        public static string paleturquoise2 => "#FFAEEEEE";
        public static string paleturquoise3 => "#FF96CDCD";
        public static string paleturquoise4 => "#FF668B8B";
        public static string palevioletred => "#FFDB7093";
        public static string palevioletred1 => "#FFFF82AB";
        public static string palevioletred2 => "#FFEE799F";
        public static string palevioletred3 => "#FFCD6889";
        public static string palevioletred4 => "#FF8B475D";
        public static string papayawhip => "#FFFFEFD5";
        public static string peachpuff1 => "#FFFFDAB9";
        public static string peachpuff2 => "#FFEECBAD";
        public static string peachpuff3 => "#FFCDAF95";
        public static string peachpuff4 => "#FF8B7765";
        public static string pink => "#FFFFC0CB";
        public static string pink1 => "#FFFFB5C5";
        public static string pink2 => "#FFEEA9B8";
        public static string pink3 => "#FFCD919E";
        public static string pink4 => "#FF8B636C";
        public static string plum => "#FFDDA0DD";
        public static string plum1 => "#FFFFBBFF";
        public static string plum2 => "#FFEEAEEE";
        public static string plum3 => "#FFCD96CD";
        public static string plum4 => "#FF8B668B";
        public static string powderblue => "#FFB0E0E6";
        public static string purple => "#FFA020F0";
        public static string purple1 => "#FF9B30FF";
        public static string purple2 => "#FF912CEE";
        public static string purple3 => "#FF7D26CD";
        public static string purple4 => "#FF551A8B";
        public static string red1 => "#FFFF0000";
        public static string red2 => "#FFEE0000";
        public static string red3 => "#FFCD0000";
        public static string red4 => "#FF8B0000";
        public static string rosybrown => "#FFBC8F8F";
        public static string rosybrown1 => "#FFFFC1C1";
        public static string rosybrown2 => "#FFEEB4B4";
        public static string rosybrown3 => "#FFCD9B9B";
        public static string rosybrown4 => "#FF8B6969";
        public static string royalblue => "#FF4169E1";
        public static string royalblue1 => "#FF4876FF";
        public static string royalblue2 => "#FF436EEE";
        public static string royalblue3 => "#FF3A5FCD";
        public static string royalblue4 => "#FF27408B";
        public static string saddlebrown => "#FF8B4513";
        public static string salmon => "#FFFA8072";
        public static string salmon1 => "#FFFF8C69";
        public static string salmon2 => "#FFEE8262";
        public static string salmon3 => "#FFCD7054";
        public static string salmon4 => "#FF8B4C39";
        public static string sandybrown => "#FFF4A460";
        public static string seagreen1 => "#FF54FF9F";
        public static string seagreen2 => "#FF4EEE94";
        public static string seagreen3 => "#FF43CD80";
        public static string seagreen4 => "#FF2E8B57";
        public static string seashell1 => "#FFFFF5EE";
        public static string seashell2 => "#FFEEE5DE";
        public static string seashell3 => "#FFCDC5BF";
        public static string seashell4 => "#FF8B8682";
        public static string sienna => "#FFA0522D";
        public static string sienna1 => "#FFFF8247";
        public static string sienna2 => "#FFEE7942";
        public static string sienna3 => "#FFCD6839";
        public static string sienna4 => "#FF8B4726";
        public static string skyblue => "#FF87CEEB";
        public static string skyblue1 => "#FF87CEFF";
        public static string skyblue2 => "#FF7EC0EE";
        public static string skyblue3 => "#FF6CA6CD";
        public static string skyblue4 => "#FF4A708B";
        public static string slateblue => "#FF6A5ACD";
        public static string slateblue1 => "#FF836FFF";
        public static string slateblue2 => "#FF7A67EE";
        public static string slateblue3 => "#FF6959CD";
        public static string slateblue4 => "#FF473C8B";
        public static string slategray => "#FF708090";
        public static string slategray1 => "#FFC6E2FF";
        public static string slategray2 => "#FFB9D3EE";
        public static string slategray3 => "#FF9FB6CD";
        public static string slategray4 => "#FF6C7B8B";
        public static string snow1 => "#FFFFFAFA";
        public static string snow2 => "#FFEEE9E9";
        public static string snow3 => "#FFCDC9C9";
        public static string snow4 => "#FF8B8989";
        public static string springgreen1 => "#FF00FF7F";
        public static string springgreen2 => "#FF00EE76";
        public static string springgreen3 => "#FF00CD66";
        public static string springgreen4 => "#FF008B45";
        public static string steelblue => "#FF4682B4";
        public static string steelblue1 => "#FF63B8FF";
        public static string steelblue2 => "#FF5CACEE";
        public static string steelblue3 => "#FF4F94CD";
        public static string steelblue4 => "#FF36648B";
        public static string tan => "#FFD2B48C";
        public static string tan1 => "#FFFFA54F";
        public static string tan2 => "#FFEE9A49";
        public static string tan3 => "#FFCD853F";
        public static string tan4 => "#FF8B5A2B";
        public static string thistle => "#FFD8BFD8";
        public static string thistle1 => "#FFFFE1FF";
        public static string thistle2 => "#FFEED2EE";
        public static string thistle3 => "#FFCDB5CD";
        public static string thistle4 => "#FF8B7B8B";
        public static string tomato1 => "#FFFF6347";
        public static string tomato2 => "#FFEE5C42";
        public static string tomato3 => "#FFCD4F39";
        public static string tomato4 => "#FF8B3626";
        public static string turquoise => "#FF40E0D0";
        public static string turquoise1 => "#FF00F5FF";
        public static string turquoise2 => "#FF00E5EE";
        public static string turquoise3 => "#FF00C5CD";
        public static string turquoise4 => "#FF00868B";
        public static string violet => "#FFEE82EE";
        public static string violetred => "#FFD02090";
        public static string violetred1 => "#FFFF3E96";
        public static string violetred2 => "#FFEE3A8C";
        public static string violetred3 => "#FFCD3278";
        public static string violetred4 => "#FF8B2252";
        public static string wheat => "#FFF5DEB3";
        public static string wheat1 => "#FFFFE7BA";
        public static string wheat2 => "#FFEED8AE";
        public static string wheat3 => "#FFCDBA96";
        public static string wheat4 => "#FF8B7E66";
        public static string white => "#FFFFFFFF";
        public static string whitesmoke => "#FFF5F5F5";
        public static string yellow1 => "#FFFFFF00";
        public static string yellow2 => "#FFEEEE00";
        public static string yellow3 => "#FFCDCD00";
        public static string yellow4 => "#FF8B8B00";
        public static string yellowgreen => "#FF9ACD32";


        #endregion
    }
}
