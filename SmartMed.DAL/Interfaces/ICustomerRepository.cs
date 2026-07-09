using System.Collections.Generic;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface ICustomerRepository : IRepository
    {
        Customer GetById(int id);

        Customer GetByPhoneOrEmail(string identifier);

        List<Customer> GetAll();

        List<Customer> Search(string keyword);

        int Add(Customer customer);

        void Update(Customer customer);

        void UpdatePin(int customerId, string pinHash, string pinSalt);

        void UpdateActiveStatus(int customerId, bool isActive);

        bool ExistsByPhoneOrEmail(string phoneNumber, string email);
    }
}
