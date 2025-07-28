using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        // 1. Task
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName);
        // 2. Task
        Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, string newStatus);

        // 3. Task
        Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request);

        // 4. Task
        Task<IEnumerable<ProfitSummary>> GetMonthlyProfitAsync();
    }
}
