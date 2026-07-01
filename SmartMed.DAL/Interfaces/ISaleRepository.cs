using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.DAL.Interfaces
{
    public interface ISaleRepository : IRepository
    {
        Sale GetById(int id);

        Sale GetBySaleNumber(string saleNumber);

        List<Sale> GetAll();

        List<Sale> GetByDateRange(DateTime fromDate, DateTime toDate);

        List<Sale> GetByCashier(int cashierId);

        List<Sale> GetByStatus(SaleStatus status);

        List<Sale> Search(string keyword);

        int Add(Sale sale);

        int Add(Sale sale, IDbConnection connection, IDbTransaction transaction);

        void UpdateStatus(int saleId, SaleStatus status);

        void UpdateStatus(int saleId, SaleStatus status, IDbConnection connection, IDbTransaction transaction);
    }
}
