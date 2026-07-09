using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.DAL.Interfaces
{
    public interface IOrderRepository : IRepository
    {
        Order GetById(int id);

        Order GetByOrderNumber(string orderNumber);

        List<Order> GetAll();

        List<Order> GetByCustomerId(int customerId);

        List<Order> GetByStatus(OrderStatus status);

        int Add(Order order, IDbConnection connection, IDbTransaction transaction);

        void UpdateStatus(int orderId, OrderStatus status);

        void UpdateStatus(int orderId, OrderStatus status, IDbConnection connection, IDbTransaction transaction);

        void UpdatePrescriptionPath(int orderId, string relativePath);
    }
}
