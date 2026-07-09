using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class Customer : BaseEntity
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string PinHash { get; set; }

        public string PinSalt { get; set; }

        public string Address { get; set; }

        public string City { get; set; }
    }
}
