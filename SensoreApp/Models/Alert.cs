using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // to allow for column type specification and exact SQL mapping

namespace SensoreApp.Models
{
    public class Alert
    {
        [Key]
        public int AlertId { get; set; } // PK -  needed for the database 
        
        [Required]
        public int UserId { get; set; } // FK  links to the patient user who the alert is for 
        
        [Required]
        public int TriggeringFrameId { get; set; } // FK -  links to the sensor frame that cause the alert

        public string? Reason {get; set; } //
        public float TriggerValue { get; set; } //


        // following ERD structure 
        // avoids error of value being 'silent;y truncated' as stated in package manager consol 
        [Column(TypeName = "decimal(5,2)")]
        public decimal ThresholdPct { get; set; } 

        public DateTime StartTime { get; set; } //
        public DateTime? EndTime {  get; set; } // 
        public string Status { get; set; } = "New"; //
        public DateTime? AcknowledgedAt { get; set; } //
        public DateTime CreatedAt { get; set; } = DateTime.Now; //

    }
}
