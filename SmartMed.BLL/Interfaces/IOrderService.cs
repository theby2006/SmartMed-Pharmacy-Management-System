using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IOrderService
    {
        OperationResult<int> PlaceOrder(Order order, List<OrderItem> items);

        OperationResult<Order> GetOrderById(int id);

        OperationResult<List<Order>> GetOrdersByCustomer(int customerId);

        OperationResult<List<Order>> GetAllOrders();

        OperationResult<List<Order>> GetOrdersByStatus(OrderStatus status);

        OperationResult UpdateOrderStatus(int orderId, OrderStatus newStatus);
    }
}
