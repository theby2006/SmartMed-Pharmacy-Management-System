using System;
using SmartMed.Models.Enums;

namespace SmartMed.Models.Entities
{
    public class AuditLogEntry
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string Username { get; set; }

        public AuditAction Action { get; set; }

        public string MachineName { get; set; }

        public DateTime Timestamp { get; set; }

        public string Details { get; set; }
    }
}
