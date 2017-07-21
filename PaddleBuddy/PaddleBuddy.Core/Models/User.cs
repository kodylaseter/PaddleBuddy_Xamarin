using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public int Id { get; set; }

        public string Validate()
        {
            if (string.IsNullOrEmpty(Email))
            {
                return "Email cannot be empty";
            }
            if (string.IsNullOrEmpty(Password) || Password.Length < PBPrefs.MinPasswordLength)
            {
                return "Password must be at least 6 characters";
            }
            if (!(Email.IndexOf("@") > -1)) 
            {
                return "Email format is invalid";
            }
            return null;
        }

        public override string ToString()
        {
            return $"Email: {Email}, Password: {Password}, Token: {Token}, Id: {Id}";
        }
    }
}
