
namespace SensoreApp.Models
{
    public class SensorDevices
    {
        
        public int SensorDeviceID { get; set; } // primary key (entity framework will auto detect this as PK due to naming convention)

        public string SerialNumber { get; set; } // unique serial number of the sensor device

        public string? Model { get; set; } // model of the sensor device, ? allows database to accept null values as set in ERD
        public DateTime RegisteredAt { get; set; } = DateTime.Now; // timestamp of when the device was registered which is the current time by default

    }
}
