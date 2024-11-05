using SpeedLR.Model;

namespace SpeedLR
{
    public static class ColorData
    {
        public static string DEFAULT_BACKGROUND = "#D3D3D3";
        public static string DEFAULT_FONT = "#000000";

        public static List<ColorGroup> GetColorGroups()
        {
            return new List<ColorGroup>
        {
            new ColorGroup("Black", new List<string> { "#000000", "#1A1A1A", "#333333", "#4D4D4D", "#666666", "#808080", "#999999", "#B3B3B3", "#CCCCCC", "#E6E6E6" }),
            new ColorGroup("White", new List<string> { "#FFFFFF", "#F2F2F2", "#E6E6E6", "#D9D9D9", "#CCCCCC", "#BFBFBF", "#B3B3B3", "#A6A6A6", "#999999", "#8C8C8C" }),
            new ColorGroup("Red", new List<string> { "#FFEBEE", "#FFCDD2", "#EF9A9A", "#E57373", "#EF5350", "#F44336", "#E53935", "#D32F2F", "#C62828", "#B71C1C" }),
            new ColorGroup("Orange", new List<string> { "#FFF3E0", "#FFE0B2", "#FFCC80", "#FFB74D", "#FFA726", "#FF9800", "#FB8C00", "#F57C00", "#EF6C00", "#E65100" }),
            new ColorGroup("Yellow", new List<string> { "#FFFDE7", "#FFF9C4", "#FFF59D", "#FFF176", "#FFEE58", "#FFEB3B", "#FDD835", "#FBC02D", "#F9A825", "#F57F17" }),
            new ColorGroup("Green", new List<string> { "#E8F5E9", "#C8E6C9", "#A5D6A7", "#81C784", "#66BB6A", "#4CAF50", "#43A047", "#388E3C", "#2E7D32", "#1B5E20" }),
            new ColorGroup("Aqua", new List<string> { "#E0F7FA", "#B2EBF2", "#80DEEA", "#4DD0E1", "#26C6DA", "#00BCD4", "#00ACC1", "#0097A7", "#00838F", "#006064" }),
            new ColorGroup("Blue", new List<string> { "#E3F2FD", "#BBDEFB", "#90CAF9", "#64B5F6", "#42A5F5", "#2196F3", "#1E88E5", "#1976D2", "#1565C0", "#0D47A1" }),
            new ColorGroup("Purple", new List<string> { "#F3E5F5", "#E1BEE7", "#CE93D8", "#BA68C8", "#AB47BC", "#9C27B0", "#8E24AA", "#7B1FA2", "#6A1B9A", "#4A148C" }),
            new ColorGroup("Magenta", new List<string> { "#F8BBD0", "#F48FB1", "#F06292", "#EC407A", "#E91E63", "#D81B60", "#C2185B", "#AD1457", "#880E4F", "#6A1B9A" })
        };
        }
    }
}
