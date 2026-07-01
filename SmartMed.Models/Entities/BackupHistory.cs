using System;
using SmartMed.Models.Common;

namespace SmartMed.Models.Entities
{
    public class BackupHistory : BaseEntity
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string DatabaseName { get; set; }
        public string BackupType { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public int? CreatedByUserId { get; set; }
    }
}
