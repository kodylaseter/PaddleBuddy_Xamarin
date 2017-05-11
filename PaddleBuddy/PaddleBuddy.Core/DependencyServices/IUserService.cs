namespace PaddleBuddy.Core.DependencyServices
{
    public interface IUserService
    {
        bool IsLoggedIn();

        int GetUserId();

        void SetUserId(int id);

        void ClearUserId();

        string GetJwt();

        void SetJwt(string token);

        void ClearJwt();
    }
}
