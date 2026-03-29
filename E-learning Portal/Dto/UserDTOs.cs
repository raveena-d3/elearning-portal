namespace E_learning_Portal.Dto
{
    public class CreateUserDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!; // "Admin", "Instructor", "Student"
    }

    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
