using System.Collections.Generic;

namespace AutoParts.ViewModels
{
    public class CreateOrderViewModel
    {
        public int CustomerId { get; set; }
        public string ShippingAddress { get; set; } // <--- Додай це
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
    }
}