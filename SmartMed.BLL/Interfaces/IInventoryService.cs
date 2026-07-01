using System.Collections.Generic;
using System.Data;
using SmartMed.BLL.Models;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IInventoryService
    {
        OperationResult<int> GetMedicineStock(int medicineId);

        OperationResult<List<StockBatch>> GetStockBatches(int medicineId);

        OperationResult<List<StockBatch>> GetFIFOBatches(int medicineId, int quantity);

        OperationResult<List<StockMovement>> GetStockMovements(int medicineId);

        OperationResult SyncMedicineStock();

        OperationResult<List<BatchDeduction>> DeductFIFO(int medicineId, int quantity, IDbConnection connection, IDbTransaction transaction);
    }
}
