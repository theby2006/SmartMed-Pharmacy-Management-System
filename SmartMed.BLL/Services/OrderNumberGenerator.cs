using System;
using System.Data;
using System.Data.SqlClient;
using SmartMed.BLL.Interfaces;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class OrderNumberGenerator : IOrderNumberGenerator
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OrderNumberGenerator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public OperationResult<string> GenerateNextNumber()
        {
            try
            {
                string year = DateTime.UtcNow.ToString("yyyy");
                string prefix = $"ORD-{year}-";

                using (SqlConnection connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (SqlCommand command = new SqlCommand(@"
                            SELECT ISNULL(MAX(CAST(SUBSTRING(OrderNumber, LEN(@Prefix) + 1, 6) AS INT)), 0) + 1
                            FROM Orders WITH (UPDLOCK, SERIALIZABLE)
                            WHERE OrderNumber LIKE @PrefixPattern", connection, transaction))
                        {
                            command.Parameters.Add(new SqlParameter("@Prefix", prefix));
                            command.Parameters.Add(new SqlParameter("@PrefixPattern", prefix + "%"));

                            object result = command.ExecuteScalar();
                            int nextSeq = result != null ? Convert.ToInt32(result) : 1;

                            if (nextSeq > 999999)
                                return OperationResult<string>.Failure("Order number sequence exhausted for the year.");

                            string orderNumber = prefix + nextSeq.ToString("D6");
                            transaction.Commit();
                            return OperationResult<string>.Success(orderNumber);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return OperationResult<string>.Failure($"Failed to generate order number: {ex.Message}");
            }
        }
    }
}
