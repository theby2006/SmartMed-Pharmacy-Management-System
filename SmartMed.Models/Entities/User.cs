using System;
using SmartMed.Models.Common;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public string DisplayName { get; set; }

        public RoleType Role { get; set; }

        public string Email { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockedUntil { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
