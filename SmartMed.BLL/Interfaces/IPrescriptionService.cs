using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IPrescriptionService
    {
        OperationResult<string> UploadPrescription(int orderId, string sourceFilePath);
    }
}
