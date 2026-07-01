using System;

namespace SmartMed.Models.Entities
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string MachineName { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
