using System;
using System.Collections.Generic;
using System.Diagnostics;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class PerformanceMonitorService : IPerformanceMonitorService
    {
        private readonly IPerformanceLogRepository _repository;
        private const int SlowThresholdMs = 500;

        public PerformanceMonitorService(IPerformanceLogRepository repository)
        {
            Guard.AgainstNull(repository, nameof(repository));
            _repository = repository;
        }

        public IDisposable BeginOperation(string operationName)
        {
            return new OperationTracker(this, operationName);
        }

        public void RecordOperation(string operationName, int durationMs)
        {
            try
            {
                PerformanceLog entry = new PerformanceLog
                {
                    OperationName = operationName,
                    DurationMs = durationMs,
                    IsSlow = durationMs >= SlowThresholdMs,
                    MachineName = Environment.MachineName,
                    CreatedDate = DateTime.UtcNow
                };
                _repository.Add(entry);
            }
            catch
            {
            }
        }

        public OperationResult<List<PerformanceLog>> GetSlowOperations(int thresholdMs = 1000, int limit = 20)
        {
            try
            {
                return OperationResult<List<PerformanceLog>>.Success(_repository.GetRecentSlowOperations(thresholdMs, limit));
            }
            catch (DataAccessException ex)
            {
                return OperationResult<List<PerformanceLog>>.Failure(ex.Message);
            }
        }

        public OperationResult<double> GetAverageDuration(string operationName, DateTime since)
        {
            try
            {
                return OperationResult<double>.Success(_repository.GetAverageDuration(operationName, since));
            }
            catch (DataAccessException ex)
            {
                return OperationResult<double>.Failure(ex.Message);
            }
        }

        private class OperationTracker : IDisposable
        {
            private readonly PerformanceMonitorService _service;
            private readonly string _operationName;
            private readonly Stopwatch _stopwatch;

            public OperationTracker(PerformanceMonitorService service, string operationName)
            {
                _service = service;
                _operationName = operationName;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                _service.RecordOperation(_operationName, (int)_stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
