namespace E_learning_Portal.models
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
