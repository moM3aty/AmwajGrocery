using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmwajGrocery.Models
{
    public enum OrderStatus
    {
        [Display(Name = "لم يتم الرد")]
        NotResponded = 0,

        [Display(Name = "مدفوع")]
        Paid = 1,

        [Display(Name = "ملغي")]
        Cancelled = 2,

        [Display(Name = "قائمة سوداء")]
        Blacklist = 3
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,3)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.NotResponded;


        public string? Notes { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public string ProductName { get; set; } 

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Price { get; set; }
    }
}