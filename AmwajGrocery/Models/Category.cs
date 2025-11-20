using System.ComponentModel.DataAnnotations;

namespace AmwajGrocery.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NameAr { get; set; }

        [Required]
        public string NameEn { get; set; }

        public string ImageUrl { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}