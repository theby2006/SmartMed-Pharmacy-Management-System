using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IOrderNumberGenerator
    {
        OperationResult<string> GenerateNextNumber();
    }
}
