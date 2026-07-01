using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IPerformanceLogRepository : IRepository
    {
        void Add(PerformanceLog entry);
        List<PerformanceLog> GetRecentSlowOperations(int thresholdMs, int limit = 20);
        List<PerformanceLog> GetByOperation(string operationName, int limit = 50);
        double GetAverageDuration(string operationName, System.DateTime since);
    }
}
