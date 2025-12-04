using System.ComponentModel.DataAnnotations;

namespace AmwajGrocery.Models
{
    public class SiteSetting
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "نص الشريط الإعلاني (عربي)")]
        public string BannerTextAr { get; set; }

        [Display(Name = "نص الشريط الإعلاني (إنجليزي)")]
        public string BannerTextEn { get; set; }
    }
}