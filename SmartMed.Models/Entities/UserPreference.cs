using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class UserPreference : BaseEntity
    {
        public int UserId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
