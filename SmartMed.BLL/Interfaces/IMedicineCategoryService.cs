using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IMedicineCategoryService
    {
        OperationResult<List<MedicineCategory>> GetAllCategories();
        OperationResult<MedicineCategory> GetCategoryById(int id);
        OperationResult<int> AddCategory(MedicineCategory category);
        OperationResult UpdateCategory(MedicineCategory category);
        OperationResult DeleteCategory(int id);
    }
}
