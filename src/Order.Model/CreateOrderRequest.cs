using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    public class CreateOrderRequest
    {
        [Required]
        public Guid ResellerId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
    }
}
