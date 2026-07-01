using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class Setting : BaseEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsSystem { get; set; }
    }
}
