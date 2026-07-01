using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface ISaleNumberGenerator
    {
        OperationResult<string> GenerateNextNumber();
    }
}
