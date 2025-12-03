using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensoreApp.Models
{
    public class ThresholdSettings
    {
        [Key] // entity framework knows that this is a pk - avoids issues with migrations previously 
        public int ThresholdID { get; set; } // This is the primary key 

        [ForeignKey("For Users")] // same concept as above excpect it indicated that this links to the user table 
        public int? UserID { get; set; } // This is the foreign key that links to the users table

        [Column(TypeName = "decimal(5,2)")] // avoids issues of value being 'silently truncated' as stated in package manager console (previously had a warning in Alert.cs about this)
        public decimal ThresholdValue { get; set; } // This is for threshold value which could be a percentage

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp for when the threshold setting is first created
      

    }
}
