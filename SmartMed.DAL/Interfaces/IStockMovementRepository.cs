using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IStockMovementRepository : IRepository
    {
        List<StockMovement> GetByStockBatchId(int stockBatchId);

        List<StockMovement> GetByMedicineId(int medicineId);

        List<StockMovement> GetByReference(string referenceType, int referenceId);

        int Add(StockMovement movement);

        int Add(StockMovement movement, IDbConnection connection, IDbTransaction transaction);
    }
}
