using System;
using System.IO;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

        private readonly IOrderRepository _orderRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ISessionManager _sessionManager;

        public PrescriptionService(
            IOrderRepository orderRepository,
            IAuditLogRepository auditLogRepository,
            ISessionManager sessionManager)
        {
            Guard.AgainstNull(orderRepository, nameof(orderRepository));
            Guard.AgainstNull(auditLogRepository, nameof(auditLogRepository));
            Guard.AgainstNull(sessionManager, nameof(sessionManager));

            _orderRepository = orderRepository;
            _auditLogRepository = auditLogRepository;
            _sessionManager = sessionManager;
        }

        public OperationResult<string> UploadPrescription(int orderId, string sourceFilePath)
        {
            try
            {
                Guard.AgainstNullOrWhiteSpace(sourceFilePath, nameof(sourceFilePath));

                if (!File.Exists(sourceFilePath))
                    return OperationResult<string>.Failure("Prescription file not found.");

                string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
                if (Array.IndexOf(AllowedExtensions, extension) < 0)
                    return OperationResult<string>.Failure("Only .jpg, .png, and .pdf files are accepted for prescriptions.");

                FileInfo fileInfo = new FileInfo(sourceFilePath);
                if (fileInfo.Length > MaxFileSizeBytes)
                    return OperationResult<string>.Failure("Prescription file must not exceed 5 MB.");

                string rootPath = AppSettings.GetRequiredString(ConfigKeys.PrescriptionUploadRootPath);
                string fullRootPath = Path.IsPathRooted(rootPath)
                    ? rootPath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootPath);

                Directory.CreateDirectory(fullRootPath);

                string storedFileName = Guid.NewGuid().ToString("N") + extension;
                string destinationPath = Path.Combine(fullRootPath, storedFileName);

                File.Copy(sourceFilePath, destinationPath, overwrite: false);

                string relativePath = Path.Combine(rootPath, storedFileName);
                _orderRepository.UpdatePrescriptionPath(orderId, relativePath);

                string machine = Environment.MachineName;
                _auditLogRepository.Log(
                    _sessionManager.CurrentSession?.UserId,
                    _sessionManager.CurrentSession?.Username ?? "System",
                    AuditAction.PrescriptionUploaded,
                    machine,
                    $"Prescription uploaded for order {orderId}");

                return OperationResult<string>.Success(relativePath, "Prescription uploaded successfully.");
            }
            catch (ValidationException ex)
            {
                return OperationResult<string>.Failure(ex.Message);
            }
            catch (IOException ex)
            {
                return OperationResult<string>.Failure($"Failed to store prescription file: {ex.Message}");
            }
        }
    }
}
