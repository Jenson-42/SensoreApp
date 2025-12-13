namespace SensoreApp.Models
{
    // Enum to represent define user roles in the system
    // This is to prevent invalid values being used for user roles
    public enum UserRole
    // will be stored as a number in the database
    {
        Patient,
        Clinician,
        Admin
    }
}
