namespace SpeedLR.Model
{
    public class ColorGroup
    {
        public string Name { get; set; }
        public List<string> ColorHexes { get; set; }

        public ColorGroup(string name, List<string> colorHexes)
        {
            Name = name;
            ColorHexes = colorHexes;
        }
    }
}
