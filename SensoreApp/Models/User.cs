namespace SensoreApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public required String FirstName { get; set; }
        public required String LastName { get; set; }
        public required String Email { get; set; }
        // Phone is stored as a string
        // because leading zeros are significant and we don't want to do maths on them.
        public required String Phone { get; set; }
        public bool IsActive { get; set; }
        // Will use Visual Studio Scaffolding for login/auth
    }
}
