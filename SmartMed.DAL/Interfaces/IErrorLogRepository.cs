using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IErrorLogRepository : IRepository
    {
        List<ErrorLog> GetAll(int limit = 100);
        ErrorLog GetById(int id);
        void Add(ErrorLog entry);
        List<ErrorLog> Search(string keyword, int limit = 50);
        int GetErrorCountSince(System.DateTime since);
    }
}
