using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        // 1. Task
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName);

        // 2. Task
        Task<byte[]> GetStatusIdByNameAsync(string statusName);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, byte[] newStatusId);

        // 3. Task
        Task<Guid> CreateOrderAsync(Guid resellerId, Guid customerId, IEnumerable<OrderItemCreationData> items);
        Task<ProductDetail> GetProductByIdAsync(Guid productId, Guid serviceId);
    }

    public record OrderItemCreationData
    {
        public Guid ProductId { get; init; }
        public Guid ServiceId { get; init; }
        public int Quantity { get; init; }
        public decimal UnitCost { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
