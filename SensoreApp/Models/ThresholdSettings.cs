using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensoreApp.Models
{
    public class ThresholdSettings
    {
        [Key]
        public int ThresholdID { get; set; }

        public int? UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public User User { get; set; }   // navigation property

        [Column(TypeName = "decimal(5,2)")]
        public decimal ThresholdValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
