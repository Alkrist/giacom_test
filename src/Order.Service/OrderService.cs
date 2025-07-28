using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        // 1. Task
        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(statusName);
            return orders;
        }

        // 2. Task
        public async Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            // get new status ID or return null if status is not found in db
            var newStatusId = await _orderRepository.GetStatusIdByNameAsync(newStatus);
            if (newStatusId == null)
            {
                return null;
            }

            // update order
            var updated = await _orderRepository.UpdateOrderStatusAsync(orderId, newStatusId);
            if (!updated)
            {
                return null;
            }

            return await GetOrderByIdAsync(orderId);
        }

        // 3. Task
        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
        {
            // validate products whether they exist, gather their data
            var productDetails = new List<ProductDetail>();
            foreach (var item in request.Items)
            {
                var product = await _orderRepository.GetProductByIdAsync(item.ProductId, item.ServiceId);
                if (product == null)
                {
                    throw new ArgumentException($"Product {item.ProductId} not found in service {item.ServiceId}");
                }
                productDetails.Add(product);
            }

            // create and save the order
            var orderId = await _orderRepository.CreateOrderAsync(
                request.ResellerId,
                request.CustomerId,
                request.Items.Zip(productDetails, (item, product) =>
                    new OrderItemCreationData
                    {
                        ProductId = item.ProductId,
                        ServiceId = item.ServiceId,
                        Quantity = item.Quantity,
                        UnitCost = product.UnitCost,
                        UnitPrice = product.UnitPrice
                    }));

            return await GetOrderByIdAsync(orderId);
        }

        // 4. Task
        public async Task<IEnumerable<ProfitSummary>> GetMonthlyProfitAsync()
        {
            // get all orders tagged as completed
            var completedOrders = await GetOrdersByStatusAsync("Completed");

            // save summary data for all orders in a month
            return completedOrders
                .GroupBy(o => new { o.CreatedDate.Year, o.CreatedDate.Month })
                .Select(g => new ProfitSummary
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalCost = g.Sum(o => o.TotalCost),
                    TotalRevenue = g.Sum(o => o.TotalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();
        }
    }
}
