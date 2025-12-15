using System.ComponentModel.DataAnnotations; // For [Required] attribute
namespace SensoreApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required]
        public String FirstName { get; set; } 
        [Required]
        public  String LastName { get; set; }
        [Required, EmailAddress]
        public  String Email { get; set; }
        // Phone is stored as a string
        // because leading zeros are significant and we don't want to do maths on them.
        [Required]
        public  String Phone { get; set; }
        public bool IsActive { get; set; }
        // Will use Visual Studio Scaffolding for login/auth

        // this property has been added to allow for role based admin management without the use of authentication (teamate contrainsts mentioned in logbook)
        // For Patiernt, Clinician, Admin user roles
        public UserRole Role { get; set; }

        //for sensor devices assigned to users
        // nullable to allow for users without sensor devices
        public int? SensorDeviceID { get; set; }
        
    }
}
