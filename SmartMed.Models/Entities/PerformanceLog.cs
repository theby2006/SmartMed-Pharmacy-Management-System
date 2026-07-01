using System;

namespace SmartMed.Models.Entities
{
    public class PerformanceLog
    {
        public int Id { get; set; }
        public string OperationName { get; set; }
        public int DurationMs { get; set; }
        public bool IsSlow { get; set; }
        public string MachineName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
