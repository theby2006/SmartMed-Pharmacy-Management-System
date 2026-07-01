using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IStockBatchRepository : IRepository
    {
        StockBatch GetById(int id);

        List<StockBatch> GetByMedicineId(int medicineId);

        StockBatch GetBatch(int medicineId, string batchNumber);

        List<StockBatch> GetFIFOBatches(int medicineId, int quantity);

        int GetAvailableStock(int medicineId);

        int Add(StockBatch batch);

        int Add(StockBatch batch, IDbConnection connection, IDbTransaction transaction);

        void UpdateQuantity(int batchId, int quantity);

        void UpdateQuantity(int batchId, int quantity, IDbConnection connection, IDbTransaction transaction);

        void Deactivate(int batchId);
    }
}
