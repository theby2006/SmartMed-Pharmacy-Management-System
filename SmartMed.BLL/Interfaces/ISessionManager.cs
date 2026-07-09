using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Session;

namespace SmartMed.BLL.Interfaces
{
    public interface ISessionManager
    {
        SessionContext StartSession(User user);

        SessionContext StartCustomerSession(Customer customer);

        void EndSession();

        SessionContext CurrentSession { get; }

        bool IsActive { get; }

        bool HasRole(RoleType role);
    }
}
