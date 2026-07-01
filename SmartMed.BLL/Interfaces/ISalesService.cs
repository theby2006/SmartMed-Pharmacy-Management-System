using System;
using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface ISalesService
    {
        OperationResult<List<Sale>> GetAllSales();

        OperationResult<Sale> GetSaleById(int id);

        OperationResult<Sale> GetSaleByNumber(string saleNumber);

        OperationResult<List<Sale>> SearchSales(string keyword);

        OperationResult<List<Sale>> GetSalesByDateRange(DateTime fromDate, DateTime toDate);

        OperationResult<List<Sale>> GetSalesByCashier(int cashierId);

        OperationResult<int> CreateSale(Sale sale, System.Collections.Generic.List<SaleItem> items, Payment payment);

        OperationResult CancelSale(int saleId);

        OperationResult<List<SaleItem>> GetSaleItems(int saleId);
    }
}
