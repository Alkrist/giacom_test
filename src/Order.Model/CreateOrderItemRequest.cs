using System;
using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    public class CreateOrderItemRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
