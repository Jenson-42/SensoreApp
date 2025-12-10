using System.ComponentModel.DataAnnotations; // to allow for data annotations like [Key] and [Required]

namespace SensoreApp.Models
{
    public class SensorDevices
    {
        [Key] // data annotation to specify primary key, the EF will now recognize this property as the PK
        public int SensorDeviceID { get; set; } // primary key (entity framework will auto detect this as PK due to naming convention)

        public string SerialNumber { get; set; } // unique serial number of the sensor device

        [Required] // data annotation to specify that this field is required (cannot be null)
        [MaxLength(100)] // data annotation to specify maximum length of the string based on ERD Model
        public string? Model { get; set; } // model of the sensor device, ? allows database to accept null values as set in ERD
        public DateTime RegisteredAt { get; set; } = DateTime.Now; // timestamp of when the device was registered which is the current time by default

    }
}
