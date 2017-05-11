using System.Threading.Tasks;
using PaddleBuddy.Core.DependencyServices;
using PaddleBuddy.Core.Models;

namespace PaddleBuddy.Core.Services
{
    public class UserService : ApiService, IUserService
    {
        private IUserService UserServiceDevice { get; set; }
        private static UserService _userService;

        public static UserService GetInstance() 
        {
            return _userService ?? (_userService = new UserService());
        }

        public void SetUserServiceDevice(IUserService userService)
        {
            UserServiceDevice = userService;
        }

        public async Task<Response> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email)) email = "test";
            if (string.IsNullOrEmpty(password)) password = "test";
            var user = new User
            {
                Email = email,
                Password = password
            };
            var response = await PostAsync("login?src=mobile", user, false);
            return response;
        }

        public async Task<Response> Register(string email, string password) 
        {
			var user = new User
			{
				Email = email,
				Password = password
			};
            var response = await PostAsync("register?src=mobile", user, false);
            LogService.Log(response);
            return response;
        }

        public void SetUserPrefs(User user)
        {
            SetJwt(user.Token);
            SetUserId(user.Id);
        }

        public void ClearUserPrefs()
        {
            ClearJwt();
            ClearUserId();
        }

        #region interface

        public void SetJwt(string token)
        {
            UserServiceDevice.SetJwt(token);
        }

        public string GetJwt()
        {
            return UserServiceDevice.GetJwt();
        }

        public void ClearJwt()
        {
            UserServiceDevice.ClearJwt();
        }


        public bool IsLoggedIn()
        {
            return UserServiceDevice.IsLoggedIn();
        }

        public int GetUserId()
        {
            return UserServiceDevice.GetUserId();
        }

        public void SetUserId(int id)
        {
            UserServiceDevice.SetUserId(id);
        }

        public void ClearUserId()
        {
            UserServiceDevice.ClearUserId();
        }
        #endregion
    }
}