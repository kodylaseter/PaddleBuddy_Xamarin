namespace PaddleBuddy.Core.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return $"Email: {Email}, Password: {Password}, Token: {Token}, Id: {Id}";
        }
    }
}
