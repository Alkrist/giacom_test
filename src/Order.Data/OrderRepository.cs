using Microsoft.EntityFrameworkCore;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderEntity = Order.Data.Entities.Order;
using OrderItemEntity = Order.Data.Entities.OrderItem;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            
            return order;
        }

        // 1. Task
        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName)
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Where(x => x.Status.Name.ToLower() == statusName.ToLower())
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        // 2. Task

        // map status ID to status name string
        public async Task<byte[]> GetStatusIdByNameAsync(string statusName)
        {
            return await _orderContext.OrderStatus
                .Where(s => s.Name == statusName)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
        }

        // update status for matching order
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, byte[] newStatusId)
        {
            // get order by guid
            var orderIdBytes = orderId.ToByteArray();
            var order = await _orderContext.Order
                .FirstOrDefaultAsync(o => _orderContext.Database.IsInMemory()
                    ? o.Id.SequenceEqual(orderIdBytes)
                    : o.Id == orderIdBytes);

            if (order == null)
            {
                return false;
            }

            // update status ID for order
            order.StatusId = newStatusId;
            await _orderContext.SaveChangesAsync();
            return true;
        }

        // 3. Task
        public async Task<Guid> CreateOrderAsync(Guid resellerId, Guid customerId, IEnumerable<OrderItemCreationData> items)
        {
            // create an empty order without items first, to get its ID
            var orderEntity = new OrderEntity
            {
                Id = Guid.NewGuid().ToByteArray(),
                ResellerId = resellerId.ToByteArray(),
                CustomerId = customerId.ToByteArray(),
                StatusId = (await _orderContext.OrderStatus.FirstAsync(s => s.Name == "New")).Id,
                CreatedDate = DateTime.UtcNow,
                Items = new List<OrderItemEntity>()
            };

            _orderContext.Order.Add(orderEntity);

            // add items using order ID
            foreach (var item in items)
            {
                orderEntity.Items.Add(new OrderItemEntity
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    ProductId = item.ProductId.ToByteArray(),
                    ServiceId = item.ServiceId.ToByteArray(),
                    Quantity = item.Quantity,
                    OrderId = orderEntity.Id
                });
            }

            await _orderContext.SaveChangesAsync();
            return new Guid(orderEntity.Id);
        }

        public async Task<ProductDetail> GetProductByIdAsync(Guid productId, Guid serviceId)
        {
            var productIdBytes = productId.ToByteArray();
            var serviceIdBytes = serviceId.ToByteArray();

            return await _orderContext.OrderProduct
                .Where(p => _orderContext.Database.IsInMemory()
                    ? p.Id.SequenceEqual(productIdBytes) && p.ServiceId.SequenceEqual(serviceIdBytes)
                    : p.Id == productIdBytes && p.ServiceId == serviceIdBytes)
                .Select(p => new ProductDetail
                {
                    Id = new Guid(p.Id),
                    ServiceId = new Guid(p.ServiceId),
                    Name = p.Name,
                    UnitCost = p.UnitCost,
                    UnitPrice = p.UnitPrice,
                    ServiceName = p.Service.Name
                })
                .FirstOrDefaultAsync();
        }
    }
}
