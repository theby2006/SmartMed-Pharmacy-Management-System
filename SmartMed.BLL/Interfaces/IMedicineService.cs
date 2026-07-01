using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IMedicineService
    {
        OperationResult<Medicine> GetMedicineById(int id);
        OperationResult<List<Medicine>> GetAllMedicines();
        OperationResult<List<Medicine>> GetMedicinesByCategory(int categoryId);
        OperationResult<List<Medicine>> SearchMedicines(string keyword);
        OperationResult<List<Medicine>> GetLowStockMedicines();
        OperationResult<List<Medicine>> GetNearExpiryMedicines();
        OperationResult<int> AddMedicine(Medicine medicine);
        OperationResult UpdateMedicine(Medicine medicine);
        OperationResult DeleteMedicine(int id);
    }
}
