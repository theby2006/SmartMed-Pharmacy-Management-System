using System.Data.SqlClient;

namespace SmartMed.DAL.Interfaces
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
