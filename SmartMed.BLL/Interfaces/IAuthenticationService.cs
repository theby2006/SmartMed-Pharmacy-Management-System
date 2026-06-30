using SmartMed.Models.Results;
using SmartMed.Models.Session;

namespace SmartMed.BLL.Interfaces
{
    public interface IAuthenticationService
    {
        OperationResult<SessionContext> LoginAdmin(string username, string password);

        OperationResult<SessionContext> LoginCustomer(string identifier, string pin = null);

        OperationResult Logout();

        bool IsAuthenticated { get; }

        SessionContext CurrentSession { get; }
    }
}
