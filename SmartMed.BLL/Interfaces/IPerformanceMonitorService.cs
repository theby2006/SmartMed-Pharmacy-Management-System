using System;
using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IPerformanceMonitorService
    {
        IDisposable BeginOperation(string operationName);
        void RecordOperation(string operationName, int durationMs);
        OperationResult<List<PerformanceLog>> GetSlowOperations(int thresholdMs = 1000, int limit = 20);
        OperationResult<double> GetAverageDuration(string operationName, DateTime since);
    }
}
