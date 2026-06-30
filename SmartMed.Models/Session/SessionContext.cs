using System;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Session
{
    public class SessionContext
    {
        public int? UserId { get; set; }

        public int? CustomerId { get; set; }

        public string Username { get; set; }

        public string DisplayName { get; set; }

        public RoleType? Role { get; set; }

        public DateTime LoginTimeUtc { get; set; }

        public DateTime LastActivityTimeUtc { get; set; }

        public bool IsAuthenticated => UserId.HasValue;
    }
}
