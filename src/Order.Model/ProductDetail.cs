using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Model
{
    public class ProductDetail
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string Name { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public string ServiceName { get; set; }
    }
}
