using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface ICustomerService
    {
        OperationResult<int> RegisterCustomer(Customer customer, string pin);

        OperationResult UpdateProfile(Customer customer);

        OperationResult<List<Customer>> GetAllCustomers();

        OperationResult<List<Customer>> SearchCustomers(string keyword);

        OperationResult<Customer> GetCustomerById(int id);

        OperationResult DeactivateCustomer(int id);
    }
}
