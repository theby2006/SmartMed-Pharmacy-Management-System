using System;
using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IPurchaseService
    {
        OperationResult<List<Purchase>> GetAllPurchases();

        OperationResult<Purchase> GetPurchaseById(int id);

        OperationResult<List<Purchase>> SearchPurchases(string keyword);

        OperationResult<List<Purchase>> GetPurchasesBySupplier(int supplierId);

        OperationResult<List<Purchase>> GetPurchasesByDateRange(DateTime fromDate, DateTime toDate);

        OperationResult<int> CreatePurchase(Purchase purchase);

        OperationResult ConfirmPurchase(int purchaseId);

        OperationResult CancelPurchase(int purchaseId);
    }
}
