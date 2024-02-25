namespace PwshSpectreConsole {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a color value that can be defined by a hex code, a SpectreColor enumeration, or RGB values.
    /// </summary>
    public class SpectreColorValue {

        public SpectreColor? Color { get; }
        public string Hex { get; }
        public (byte R, byte G, byte B) RGB { get; }

        // Mapping of hex values to SpectreColor
        private static readonly Dictionary<SpectreColor, (string Hex, (byte R, byte G, byte B) RGB)> ColorMap = new Dictionary<SpectreColor, (string, (byte, byte, byte))> {
            { SpectreColor.black, ("#000000", (0, 0, 0)) },
            { SpectreColor.maroon, ("#800000", (128, 0, 0)) },
            { SpectreColor.green, ("#008000", (0, 128, 0)) },
            { SpectreColor.olive, ("#808000", (128, 128, 0)) },
            { SpectreColor.navy, ("#000080", (0, 0, 128)) },
            { SpectreColor.purple, ("#800080", (128, 0, 128)) },
            { SpectreColor.teal, ("#008080", (0, 128, 128)) },
            { SpectreColor.silver, ("#C0C0C0", (192, 192, 192)) },
            { SpectreColor.grey, ("#808080", (128, 128, 128)) },
            { SpectreColor.red, ("#FF0000", (255, 0, 0)) },
            { SpectreColor.lime, ("#00FF00", (0, 255, 0)) },
            { SpectreColor.yellow, ("#FFFF00", (255, 255, 0)) },
            { SpectreColor.blue, ("#0000FF", (0, 0, 255)) },
            { SpectreColor.fuchsia, ("#FF00FF", (255, 0, 255)) },
            { SpectreColor.aqua, ("#00FFFF", (0, 255, 255)) },
            { SpectreColor.white, ("#FFFFFF", (255, 255, 255)) },
            { SpectreColor.grey0, ("#000000", (0, 0, 0)) },
            { SpectreColor.navyblue, ("#00005F", (0, 0, 95)) },
            { SpectreColor.darkblue, ("#000087", (0, 0, 135)) },
            { SpectreColor.blue3, ("#0000AF", (0, 0, 175)) },
            { SpectreColor.blue3_1, ("#0000D7", (0, 0, 215)) },
            { SpectreColor.blue1, ("#0000FF", (0, 0, 255)) },
            { SpectreColor.darkgreen, ("#005F00", (0, 95, 0)) },
            { SpectreColor.deepskyblue4, ("#005F5F", (0, 95, 95)) },
            { SpectreColor.deepskyblue4_1, ("#005F87", (0, 95, 135)) },
            { SpectreColor.deepskyblue4_2, ("#005FAF", (0, 95, 175)) },
            { SpectreColor.dodgerblue3, ("#005FD7", (0, 95, 215)) },
            { SpectreColor.dodgerblue2, ("#005FFF", (0, 95, 255)) },
            { SpectreColor.green4, ("#008700", (0, 135, 0)) },
            { SpectreColor.springgreen4, ("#00875F", (0, 135, 95)) },
            { SpectreColor.turquoise4, ("#008787", (0, 135, 135)) },
            { SpectreColor.deepskyblue3, ("#0087AF", (0, 135, 175)) },
            { SpectreColor.deepskyblue3_1, ("#0087D7", (0, 135, 215)) },
            { SpectreColor.dodgerblue1, ("#0087FF", (0, 135, 255)) },
            { SpectreColor.green3, ("#00AF00", (0, 175, 0)) },
            { SpectreColor.springgreen3, ("#00AF5F", (0, 175, 95)) },
            { SpectreColor.darkcyan, ("#00AF87", (0, 175, 135)) },
            { SpectreColor.lightseagreen, ("#00AFAF", (0, 175, 175)) },
            { SpectreColor.deepskyblue2, ("#00AFD7", (0, 175, 215)) },
            { SpectreColor.deepskyblue1, ("#00AFFF", (0, 175, 255)) },
            { SpectreColor.green3_1, ("#00D700", (0, 215, 0)) },
            { SpectreColor.springgreen3_1, ("#00D75F", (0, 215, 95)) },
            { SpectreColor.springgreen2, ("#00D787", (0, 215, 135)) },
            { SpectreColor.cyan3, ("#00D7AF", (0, 215, 175)) },
            { SpectreColor.darkturquoise, ("#00D7D7", (0, 215, 215)) },
            { SpectreColor.turquoise2, ("#00D7FF", (0, 215, 255)) },
            { SpectreColor.green1, ("#00FF00", (0, 255, 0)) },
            { SpectreColor.springgreen2_1, ("#00FF5F", (0, 255, 95)) },
            { SpectreColor.springgreen1, ("#00FF87", (0, 255, 135)) },
            { SpectreColor.mediumspringgreen, ("#00FFAF", (0, 255, 175)) },
            { SpectreColor.cyan2, ("#00FFD7", (0, 255, 215)) },
            { SpectreColor.cyan1, ("#00FFFF", (0, 255, 255)) },
            { SpectreColor.darkred, ("#5F0000", (95, 0, 0)) },
            { SpectreColor.deeppink4, ("#5F005F", (95, 0, 95)) },
            { SpectreColor.purple4, ("#5F0087", (95, 0, 135)) },
            { SpectreColor.purple4_1, ("#5F00AF", (95, 0, 175)) },
            { SpectreColor.purple3, ("#5F00D7", (95, 0, 215)) },
            { SpectreColor.blueviolet, ("#5F00FF", (95, 0, 255)) },
            { SpectreColor.orange4, ("#5F5F00", (95, 95, 0)) },
            { SpectreColor.grey37, ("#5F5F5F", (95, 95, 95)) },
            { SpectreColor.mediumpurple4, ("#5F5F87", (95, 95, 135)) },
            { SpectreColor.slateblue3, ("#5F5FAF", (95, 95, 175)) },
            { SpectreColor.slateblue3_1, ("#5F5FD7", (95, 95, 215)) },
            { SpectreColor.royalblue1, ("#5F5FFF", (95, 95, 255)) },
            { SpectreColor.chartreuse4, ("#5F8700", (95, 135, 0)) },
            { SpectreColor.darkseagreen4, ("#5F875F", (95, 135, 95)) },
            { SpectreColor.paleturquoise4, ("#5F8787", (95, 135, 135)) },
            { SpectreColor.steelblue, ("#5F87AF", (95, 135, 175)) },
            { SpectreColor.steelblue3, ("#5F87D7", (95, 135, 215)) },
            { SpectreColor.cornflowerblue, ("#5F87FF", (95, 135, 255)) },
            { SpectreColor.chartreuse3, ("#5FAF00", (95, 175, 0)) },
            { SpectreColor.darkseagreen4_1, ("#5FAF5F", (95, 175, 95)) },
            { SpectreColor.cadetblue, ("#5FAF87", (95, 175, 135)) },
            { SpectreColor.cadetblue_1, ("#5FAFAF", (95, 175, 175)) },
            { SpectreColor.skyblue3, ("#5FAFD7", (95, 175, 215)) },
            { SpectreColor.steelblue1, ("#5FAFFF", (95, 175, 255)) },
            { SpectreColor.chartreuse3_1, ("#5FD700", (95, 215, 0)) },
            { SpectreColor.palegreen3, ("#5FD75F", (95, 215, 95)) },
            { SpectreColor.seagreen3, ("#5FD787", (95, 215, 135)) },
            { SpectreColor.aquamarine3, ("#5FD7AF", (95, 215, 175)) },
            { SpectreColor.mediumturquoise, ("#5FD7D7", (95, 215, 215)) },
            { SpectreColor.steelblue1_1, ("#5FD7FF", (95, 215, 255)) },
            { SpectreColor.chartreuse2, ("#5FFF00", (95, 255, 0)) },
            { SpectreColor.seagreen2, ("#5FFF5F", (95, 255, 95)) },
            { SpectreColor.seagreen1, ("#5FFF87", (95, 255, 135)) },
            { SpectreColor.seagreen1_1, ("#5FFFAF", (95, 255, 175)) },
            { SpectreColor.aquamarine1, ("#5FFFD7", (95, 255, 215)) },
            { SpectreColor.darkslategray2, ("#5FFFFF", (95, 255, 255)) },
            { SpectreColor.darkred_1, ("#870000", (135, 0, 0)) },
            { SpectreColor.deeppink4_1, ("#87005F", (135, 0, 95)) },
            { SpectreColor.darkmagenta, ("#870087", (135, 0, 135)) },
            { SpectreColor.darkmagenta_1, ("#8700AF", (135, 0, 175)) },
            { SpectreColor.darkviolet, ("#8700D7", (135, 0, 215)) },
            { SpectreColor.purple_1, ("#8700FF", (135, 0, 255)) },
            { SpectreColor.orange4_1, ("#875F00", (135, 95, 0)) },
            { SpectreColor.lightpink4, ("#875F5F", (135, 95, 95)) },
            { SpectreColor.plum4, ("#875F87", (135, 95, 135)) },
            { SpectreColor.mediumpurple3, ("#875FAF", (135, 95, 175)) },
            { SpectreColor.mediumpurple3_1, ("#875FD7", (135, 95, 215)) },
            { SpectreColor.slateblue1, ("#875FFF", (135, 95, 255)) },
            { SpectreColor.yellow4, ("#878700", (135, 135, 0)) },
            { SpectreColor.wheat4, ("#87875F", (135, 135, 95)) },
            { SpectreColor.grey53, ("#878787", (135, 135, 135)) },
            { SpectreColor.lightslategrey, ("#8787AF", (135, 135, 175)) },
            { SpectreColor.mediumpurple, ("#8787D7", (135, 135, 215)) },
            { SpectreColor.lightslateblue, ("#8787FF", (135, 135, 255)) },
            { SpectreColor.yellow4_1, ("#87AF00", (135, 175, 0)) },
            { SpectreColor.darkolivegreen3, ("#87AF5F", (135, 175, 95)) },
            { SpectreColor.darkseagreen, ("#87AF87", (135, 175, 135)) },
            { SpectreColor.lightskyblue3, ("#87AFAF", (135, 175, 175)) },
            { SpectreColor.lightskyblue3_1, ("#87AFD7", (135, 175, 215)) },
            { SpectreColor.skyblue2, ("#87AFFF", (135, 175, 255)) },
            { SpectreColor.chartreuse2_1, ("#87D700", (135, 215, 0)) },
            { SpectreColor.darkolivegreen3_1, ("#87D75F", (135, 215, 95)) },
            { SpectreColor.palegreen3_1, ("#87D787", (135, 215, 135)) },
            { SpectreColor.darkseagreen3, ("#87D7AF", (135, 215, 175)) },
            { SpectreColor.darkslategray3, ("#87D7D7", (135, 215, 215)) },
            { SpectreColor.skyblue1, ("#87D7FF", (135, 215, 255)) },
            { SpectreColor.chartreuse1, ("#87FF00", (135, 255, 0)) },
            { SpectreColor.lightgreen, ("#87FF5F", (135, 255, 95)) },
            { SpectreColor.lightgreen_1, ("#87FF87", (135, 255, 135)) },
            { SpectreColor.palegreen1, ("#87FFAF", (135, 255, 175)) },
            { SpectreColor.aquamarine1_1, ("#87FFD7", (135, 255, 215)) },
            { SpectreColor.darkslategray1, ("#87FFFF", (135, 255, 255)) },
            { SpectreColor.red3, ("#AF0000", (175, 0, 0)) },
            { SpectreColor.deeppink4_2, ("#AF005F", (175, 0, 95)) },
            { SpectreColor.mediumvioletred, ("#AF0087", (175, 0, 135)) },
            { SpectreColor.magenta3, ("#AF00AF", (175, 0, 175)) },
            { SpectreColor.darkviolet_1, ("#AF00D7", (175, 0, 215)) },
            { SpectreColor.purple_2, ("#AF00FF", (175, 0, 255)) },
            { SpectreColor.darkorange3, ("#AF5F00", (175, 95, 0)) },
            { SpectreColor.indianred, ("#AF5F5F", (175, 95, 95)) },
            { SpectreColor.hotpink3, ("#AF5F87", (175, 95, 135)) },
            { SpectreColor.mediumorchid3, ("#AF5FAF", (175, 95, 175)) },
            { SpectreColor.mediumorchid, ("#AF5FD7", (175, 95, 215)) },
            { SpectreColor.mediumpurple2, ("#AF5FFF", (175, 95, 255)) },
            { SpectreColor.darkgoldenrod, ("#AF8700", (175, 135, 0)) },
            { SpectreColor.lightsalmon3, ("#AF875F", (175, 135, 95)) },
            { SpectreColor.rosybrown, ("#AF8787", (175, 135, 135)) },
            { SpectreColor.grey63, ("#AF87AF", (175, 135, 175)) },
            { SpectreColor.mediumpurple2_1, ("#AF87D7", (175, 135, 215)) },
            { SpectreColor.mediumpurple1, ("#AF87FF", (175, 135, 255)) },
            { SpectreColor.gold3, ("#AFAF00", (175, 175, 0)) },
            { SpectreColor.darkkhaki, ("#AFAF5F", (175, 175, 95)) },
            { SpectreColor.navajowhite3, ("#AFAF87", (175, 175, 135)) },
            { SpectreColor.grey69, ("#AFAFAF", (175, 175, 175)) },
            { SpectreColor.lightsteelblue3, ("#AFAFD7", (175, 175, 215)) },
            { SpectreColor.lightsteelblue, ("#AFAFFF", (175, 175, 255)) },
            { SpectreColor.yellow3, ("#AFD700", (175, 215, 0)) },
            { SpectreColor.darkolivegreen3_2, ("#AFD75F", (175, 215, 95)) },
            { SpectreColor.darkseagreen3_1, ("#AFD787", (175, 215, 135)) },
            { SpectreColor.darkseagreen2, ("#AFD7AF", (175, 215, 175)) },
            { SpectreColor.lightcyan3, ("#AFD7D7", (175, 215, 215)) },
            { SpectreColor.lightskyblue1, ("#AFD7FF", (175, 215, 255)) },
            { SpectreColor.greenyellow, ("#AFFF00", (175, 255, 0)) },
            { SpectreColor.darkolivegreen2, ("#AFFF5F", (175, 255, 95)) },
            { SpectreColor.palegreen1_1, ("#AFFF87", (175, 255, 135)) },
            { SpectreColor.darkseagreen2_1, ("#AFFFAF", (175, 255, 175)) },
            { SpectreColor.darkseagreen1, ("#AFFFD7", (175, 255, 215)) },
            { SpectreColor.paleturquoise1, ("#AFFFFF", (175, 255, 255)) },
            { SpectreColor.red3_1, ("#D70000", (215, 0, 0)) },
            { SpectreColor.deeppink3, ("#D7005F", (215, 0, 95)) },
            { SpectreColor.deeppink3_1, ("#D70087", (215, 0, 135)) },
            { SpectreColor.magenta3_1, ("#D700AF", (215, 0, 175)) },
            { SpectreColor.magenta3_2, ("#D700D7", (215, 0, 215)) },
            { SpectreColor.magenta2, ("#D700FF", (215, 0, 255)) },
            { SpectreColor.darkorange3_1, ("#D75F00", (215, 95, 0)) },
            { SpectreColor.indianred_1, ("#D75F5F", (215, 95, 95)) },
            { SpectreColor.hotpink3_1, ("#D75F87", (215, 95, 135)) },
            { SpectreColor.hotpink2, ("#D75FAF", (215, 95, 175)) },
            { SpectreColor.orchid, ("#D75FD7", (215, 95, 215)) },
            { SpectreColor.mediumorchid1, ("#D75FFF", (215, 95, 255)) },
            { SpectreColor.orange3, ("#D78700", (215, 135, 0)) },
            { SpectreColor.lightsalmon3_1, ("#D7875F", (215, 135, 95)) },
            { SpectreColor.lightpink3, ("#D78787", (215, 135, 135)) },
            { SpectreColor.pink3, ("#D787AF", (215, 135, 175)) },
            { SpectreColor.plum3, ("#D787D7", (215, 135, 215)) },
            { SpectreColor.violet, ("#D787FF", (215, 135, 255)) },
            { SpectreColor.gold3_1, ("#D7AF00", (215, 175, 0)) },
            { SpectreColor.lightgoldenrod3, ("#D7AF5F", (215, 175, 95)) },
            { SpectreColor.tan, ("#D7AF87", (215, 175, 135)) },
            { SpectreColor.mistyrose3, ("#D7AFAF", (215, 175, 175)) },
            { SpectreColor.thistle3, ("#D7AFD7", (215, 175, 215)) },
            { SpectreColor.plum2, ("#D7AFFF", (215, 175, 255)) },
            { SpectreColor.yellow3_1, ("#D7D700", (215, 215, 0)) },
            { SpectreColor.khaki3, ("#D7D75F", (215, 215, 95)) },
            { SpectreColor.lightgoldenrod2, ("#D7D787", (215, 215, 135)) },
            { SpectreColor.lightyellow3, ("#D7D7AF", (215, 215, 175)) },
            { SpectreColor.grey84, ("#D7D7D7", (215, 215, 215)) },
            { SpectreColor.lightsteelblue1, ("#D7D7FF", (215, 215, 255)) },
            { SpectreColor.yellow2, ("#D7FF00", (215, 255, 0)) },
            { SpectreColor.darkolivegreen1, ("#D7FF5F", (215, 255, 95)) },
            { SpectreColor.darkolivegreen1_1, ("#D7FF87", (215, 255, 135)) },
            { SpectreColor.darkseagreen1_1, ("#D7FFAF", (215, 255, 175)) },
            { SpectreColor.honeydew2, ("#D7FFD7", (215, 255, 215)) },
            { SpectreColor.lightcyan1, ("#D7FFFF", (215, 255, 255)) },
            { SpectreColor.red1, ("#FF0000", (255, 0, 0)) },
            { SpectreColor.deeppink2, ("#FF005F", (255, 0, 95)) },
            { SpectreColor.deeppink1, ("#FF0087", (255, 0, 135)) },
            { SpectreColor.deeppink1_1, ("#FF00AF", (255, 0, 175)) },
            { SpectreColor.magenta2_1, ("#FF00D7", (255, 0, 215)) },
            { SpectreColor.magenta1, ("#FF00FF", (255, 0, 255)) },
            { SpectreColor.orangered1, ("#FF5F00", (255, 95, 0)) },
            { SpectreColor.indianred1, ("#FF5F5F", (255, 95, 95)) },
            { SpectreColor.indianred1_1, ("#FF5F87", (255, 95, 135)) },
            { SpectreColor.hotpink, ("#FF5FAF", (255, 95, 175)) },
            { SpectreColor.hotpink_1, ("#FF5FD7", (255, 95, 215)) },
            { SpectreColor.mediumorchid1_1, ("#FF5FFF", (255, 95, 255)) },
            { SpectreColor.darkorange, ("#FF8700", (255, 135, 0)) },
            { SpectreColor.salmon1, ("#FF875F", (255, 135, 95)) },
            { SpectreColor.lightcoral, ("#FF8787", (255, 135, 135)) },
            { SpectreColor.palevioletred1, ("#FF87AF", (255, 135, 175)) },
            { SpectreColor.orchid2, ("#FF87D7", (255, 135, 215)) },
            { SpectreColor.orchid1, ("#FF87FF", (255, 135, 255)) },
            { SpectreColor.orange1, ("#FFAF00", (255, 175, 0)) },
            { SpectreColor.sandybrown, ("#FFAF5F", (255, 175, 95)) },
            { SpectreColor.lightsalmon1, ("#FFAF87", (255, 175, 135)) },
            { SpectreColor.lightpink1, ("#FFAFAF", (255, 175, 175)) },
            { SpectreColor.pink1, ("#FFAFD7", (255, 175, 215)) },
            { SpectreColor.plum1, ("#FFAFFF", (255, 175, 255)) },
            { SpectreColor.gold1, ("#FFD700", (255, 215, 0)) },
            { SpectreColor.lightgoldenrod2_1, ("#FFD75F", (255, 215, 95)) },
            { SpectreColor.lightgoldenrod2_2, ("#FFD787", (255, 215, 135)) },
            { SpectreColor.navajowhite1, ("#FFD7AF", (255, 215, 175)) },
            { SpectreColor.mistyrose1, ("#FFD7D7", (255, 215, 215)) },
            { SpectreColor.thistle1, ("#FFD7FF", (255, 215, 255)) },
            { SpectreColor.yellow1, ("#FFFF00", (255, 255, 0)) },
            { SpectreColor.lightgoldenrod1, ("#FFFF5F", (255, 255, 95)) },
            { SpectreColor.khaki1, ("#FFFF87", (255, 255, 135)) },
            { SpectreColor.wheat1, ("#FFFFAF", (255, 255, 175)) },
            { SpectreColor.cornsilk1, ("#FFFFD7", (255, 255, 215)) },
            { SpectreColor.grey100, ("#FFFFFF", (255, 255, 255)) },
            { SpectreColor.grey3, ("#080808", (8, 8, 8)) },
            { SpectreColor.grey7, ("#121212", (18, 18, 18)) },
            { SpectreColor.grey11, ("#1C1C1C", (28, 28, 28)) },
            { SpectreColor.grey15, ("#262626", (38, 38, 38)) },
            { SpectreColor.grey19, ("#303030", (48, 48, 48)) },
            { SpectreColor.grey23, ("#3A3A3A", (58, 58, 58)) },
            { SpectreColor.grey27, ("#444444", (68, 68, 68)) },
            { SpectreColor.grey30, ("#4E4E4E", (78, 78, 78)) },
            { SpectreColor.grey35, ("#585858", (88, 88, 88)) },
            { SpectreColor.grey39, ("#626262", (98, 98, 98)) },
            { SpectreColor.grey42, ("#6C6C6C", (108, 108, 108)) },
            { SpectreColor.grey46, ("#767676", (118, 118, 118)) },
            { SpectreColor.grey50, ("#808080", (128, 128, 128)) },
            { SpectreColor.grey54, ("#8A8A8A", (138, 138, 138)) },
            { SpectreColor.grey58, ("#949494", (148, 148, 148)) },
            { SpectreColor.grey62, ("#9E9E9E", (158, 158, 158)) },
            { SpectreColor.grey66, ("#A8A8A8", (168, 168, 168)) },
            { SpectreColor.grey70, ("#B2B2B2", (178, 178, 178)) },
            { SpectreColor.grey74, ("#BCBCBC", (188, 188, 188)) },
            { SpectreColor.grey78, ("#C6C6C6", (198, 198, 198)) },
            { SpectreColor.grey82, ("#D0D0D0", (208, 208, 208)) },
            { SpectreColor.grey85, ("#DADADA", (218, 218, 218)) },
            { SpectreColor.grey89, ("#E4E4E4", (228, 228, 228)) },
            { SpectreColor.grey93, ("#EEEEEE", (238, 238, 238)) }

        };

        public static IReadOnlyDictionary<SpectreColor, (string Hex, (byte R, byte G, byte B) RGB)> PublicColorMap => new ReadOnlyDictionary<SpectreColor, (string, (byte, byte, byte))>(ColorMap);

        // Constructors  ////////////////////////////////////////////////////////////

        public SpectreColorValue(string hex) {
            if (!IsValidHex(hex)) throw new ArgumentException("Invalid hex code.", nameof(hex));
            Hex = hex;
            RGB = HexToRgb(hex);
            Color = HexToEnum(hex);
        }

        public SpectreColorValue(SpectreColor color) {
            Color = color;
            if (ColorMap.TryGetValue(color, out var colorInfo)) {
                Hex = colorInfo.Hex;
                RGB = colorInfo.RGB;
            } else {
                throw new ArgumentException("Invalid SpectreColor value.", nameof(color));
            }
        }

        public SpectreColorValue(byte r, byte g, byte b) {
            RGB = (r, g, b);
            Hex = RgbToHex(r, g, b);
            Color = RgbToEnum(r, g, b);
        }

        // Converters  //////////////////////////////////////////////////////////////

        private SpectreColor? RgbToEnum(byte r, byte g, byte b) {
            // Attempt to find a matching SpectreColor based on the RGB tuple
            var entry = ColorMap.FirstOrDefault(kv => kv.Value.RGB == (r, g, b));
            if (!entry.Equals(default(KeyValuePair<SpectreColor, (string, (byte, byte, byte))>))) {
                return entry.Key;
            }
            return null; // Or handle this more robustly
        }

        private static string RgbToHex(byte r, byte g, byte b) {
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private static (byte, byte, byte) HexToRgb(string hex) {
            if (!IsValidHex(hex)) throw new ArgumentException("Invalid hex code.", nameof(hex));
            // Assuming hex is in the format "#RRGGBB"
            var r = Convert.ToByte(hex.Substring(1, 2), 16);
            var g = Convert.ToByte(hex.Substring(3, 2), 16);
            var b = Convert.ToByte(hex.Substring(5, 2), 16);
            return (r, g, b);
        }

        private SpectreColor? HexToEnum(string hex) {
            foreach (var pair in ColorMap) {
                if (pair.Value.Hex.Equals(hex, StringComparison.OrdinalIgnoreCase)) {
                    return pair.Key;
                }
            }
            return null;
        }

        // Hex Validation  //////////////////////////////////////////////////////////

        private static bool IsValidHex(string hex) {
            // Regex to validate 6-Digit hex color code
            var hexRegex = new Regex("^#([0-9A-Fa-f]{6})$");
            return hexRegex.IsMatch(hex);
        }

        public override string ToString() {
            return Color?.ToString() ?? Hex ?? $"RGB({RGB.Item1},{RGB.Item2},{RGB.Item3})";
        }
    }
}