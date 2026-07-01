using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class MedicineCategory : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
