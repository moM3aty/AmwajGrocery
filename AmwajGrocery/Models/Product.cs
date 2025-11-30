using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmwajGrocery.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NameAr { get; set; }
        [Required]
        public string NameEn { get; set; }

        public string ShortDescriptionAr { get; set; }
        public string ShortDescriptionEn { get; set; }

        public string LongDescriptionAr { get; set; }
        public string LongDescriptionEn { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? PriceCarton { get; set; } 

        [Column(TypeName = "decimal(18,3)")]
        public decimal? OldPrice { get; set; }

        public string ImageUrl { get; set; }
        public bool InStock { get; set; } = true;
        public bool IsBestSeller { get; set; }
        public bool IsHotDeal { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public string MetaKeywords { get; set; } 
        public string MetaDescription { get; set; } 

        [NotMapped]
        public string DisplayPrice => $"{Price:F3} OMR";
    }
}