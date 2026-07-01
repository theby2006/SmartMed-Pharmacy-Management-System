using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
public interface IPurchaseItemRepository : IRepository
{
    List<PurchaseItem> GetByPurchaseId(int purchaseId);

    int Add(PurchaseItem item);

    void AddRange(List<PurchaseItem> items);

    void AddRange(List<PurchaseItem> items, IDbConnection connection, IDbTransaction transaction);

    bool ExistsByMedicineAndBatch(int medicineId, string batchNumber);
}
}
