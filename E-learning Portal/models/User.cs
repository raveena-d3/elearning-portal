namespace E_learning_Portal.models
{
    public enum Role
    {
        Admin,
        Instructor,
        Student
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; // store hashed passwords
        public Role Role { get; set; }
    }
}
