using SmartMed.Models.Diagnostics;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IStartupDiagnosticsService
    {
        OperationResult<ApplicationStartupContext> BuildContext();
    }
}
