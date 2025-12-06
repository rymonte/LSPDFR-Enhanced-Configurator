using System.Collections.Generic;

namespace LSPDFREnhancedConfigurator.Models
{
    /// <summary>
    /// Maps XML elements to their line numbers in the generated XML
    /// </summary>
    public class XmlLineMapping
    {
        public Dictionary<string, int> RankStartLines { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> RankEndLines { get; } = new Dictionary<string, int>();
        public Dictionary<string, XmlElementPosition> ElementPositions { get; } = new Dictionary<string, XmlElementPosition>();
    }

    public class XmlElementPosition
    {
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string RankName { get; set; } = string.Empty;
        public string ElementType { get; set; } = string.Empty; // "Name", "XpFrom", "Salary", "Vehicle", "Station", "Outfit"
        public string ElementValue { get; set; } = string.Empty;
    }
}
