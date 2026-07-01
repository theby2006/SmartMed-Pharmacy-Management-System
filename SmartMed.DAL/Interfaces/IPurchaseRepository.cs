using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;

namespace SmartMed.DAL.Interfaces
{
    public interface IPurchaseRepository : IRepository
    {
        List<Purchase> GetAll();

        Purchase GetById(int id);

        Purchase GetByPurchaseNumber(string purchaseNumber);

        List<Purchase> Search(string keyword);

        List<Purchase> GetBySupplier(int supplierId);

        List<Purchase> GetByDateRange(DateTime fromDate, DateTime toDate);

        int Add(Purchase purchase);

        int Add(Purchase purchase, IDbConnection connection, IDbTransaction transaction);

        void Update(Purchase purchase);

        void Confirm(int purchaseId, DateTime confirmedDate);

        void Confirm(int purchaseId, DateTime confirmedDate, decimal totalAmount, IDbConnection connection, IDbTransaction transaction);

        void Cancel(int purchaseId);

        void Cancel(int purchaseId, IDbConnection connection, IDbTransaction transaction);
    }
}
