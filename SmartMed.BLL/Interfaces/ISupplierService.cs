using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface ISupplierService
    {
        OperationResult<List<Supplier>> GetAllSuppliers();
        OperationResult<Supplier> GetSupplierById(int id);
        OperationResult<List<Supplier>> SearchSuppliers(string keyword);
        OperationResult<int> AddSupplier(Supplier supplier);
        OperationResult UpdateSupplier(Supplier supplier);
        OperationResult DeleteSupplier(int id);
    }
}
