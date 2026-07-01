using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IBackupHistoryRepository : IRepository
    {
        List<BackupHistory> GetAll();
        BackupHistory GetById(int id);
        BackupHistory GetLatest();
        void Add(BackupHistory entry);
        void Delete(int id);
        void DeleteOlderThan(int keepCount);
    }
}
