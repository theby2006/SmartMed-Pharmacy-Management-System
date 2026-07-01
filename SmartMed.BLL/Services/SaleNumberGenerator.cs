using System;
using System.Data;
using System.Data.SqlClient;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Exceptions;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class SaleNumberGenerator : ISaleNumberGenerator
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SaleNumberGenerator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public OperationResult<string> GenerateNextNumber()
        {
            try
            {
                string year = DateTime.UtcNow.ToString("yyyy");
                string prefix = $"SAL-{year}-";

                using (SqlConnection connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (SqlCommand command = new SqlCommand(@"
                            SELECT ISNULL(MAX(CAST(SUBSTRING(SaleNumber, LEN(@Prefix) + 1, 6) AS INT)), 0) + 1
                            FROM Sales WITH (UPDLOCK, SERIALIZABLE)
                            WHERE SaleNumber LIKE @PrefixPattern", connection, transaction))
                        {
                            command.Parameters.Add(new SqlParameter("@Prefix", prefix));
                            command.Parameters.Add(new SqlParameter("@PrefixPattern", prefix + "%"));

                            object result = command.ExecuteScalar();
                            int nextSeq = result != null ? Convert.ToInt32(result) : 1;

                            if (nextSeq > 999999)
                                return OperationResult<string>.Failure("Sale number sequence exhausted for the year.");

                            string saleNumber = prefix + nextSeq.ToString("D6");
                            transaction.Commit();
                            return OperationResult<string>.Success(saleNumber);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return OperationResult<string>.Failure($"Failed to generate sale number: {ex.Message}");
            }
        }
    }
}
