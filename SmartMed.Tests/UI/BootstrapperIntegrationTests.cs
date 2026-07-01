using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.DAL.Infrastructure;
using SmartMed.DAL.Interfaces;
using SmartMed.DAL.Repositories;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;
using SmartMed.Models.Results;
using SmartMed.Models.Session;

namespace SmartMed.Tests.UI
{
    [TestClass]
    public class BootstrapperIntegrationTests
    {
        [TestMethod]
        public void MedicineDependencies_ShouldCompose_ThroughAbstractions()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);
            IDbConnectionFactory connectionFactory = new SqlConnectionFactory(connectionString);

            IMedicineCategoryRepository categoryRepository = new MedicineCategoryRepository(connectionFactory);
            IMedicineRepository medicineRepository = new MedicineRepository(connectionFactory);

            IMedicineCategoryService categoryService = new MedicineCategoryService(categoryRepository, medicineRepository);
            IMedicineService medicineService = new MedicineService(medicineRepository, categoryRepository);

            Assert.IsNotNull(categoryService);
            Assert.IsNotNull(medicineService);
        }

        [TestMethod]
        public void CategoryService_ShouldRequire_MedicineRepository()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);
            IDbConnectionFactory connectionFactory = new SqlConnectionFactory(connectionString);

            IMedicineCategoryRepository categoryRepository = new MedicineCategoryRepository(connectionFactory);
            IMedicineRepository medicineRepository = new MedicineRepository(connectionFactory);

            IMedicineCategoryService categoryService = new MedicineCategoryService(categoryRepository, medicineRepository);

            Assert.IsNotNull(categoryService);
            Assert.IsInstanceOfType(categoryService, typeof(MedicineCategoryService));
        }

        [TestMethod]
        public void SupplierDependencies_ShouldCompose_ThroughAbstractions()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);
            IDbConnectionFactory connectionFactory = new SqlConnectionFactory(connectionString);

            ISupplierRepository supplierRepository = new SupplierRepository(connectionFactory);
            IAuditLogRepository auditLogRepository = new SupplierTestAuditLogRepository();
            ISessionManager sessionManager = new SupplierTestSessionManager();
            ISupplierService supplierService = new SupplierService(supplierRepository, auditLogRepository, sessionManager);

            Assert.IsNotNull(supplierService);
        }

        [TestMethod]
        public void SupplierService_ShouldRequire_SupplierRepository()
        {
            string connectionString = AppSettings.GetConnectionString(ConfigKeys.PrimaryConnectionStringName);
            IDbConnectionFactory connectionFactory = new SqlConnectionFactory(connectionString);

            ISupplierRepository supplierRepository = new SupplierRepository(connectionFactory);
            IAuditLogRepository auditLogRepository = new SupplierTestAuditLogRepository();
            ISessionManager sessionManager = new SupplierTestSessionManager();
            ISupplierService supplierService = new SupplierService(supplierRepository, auditLogRepository, sessionManager);

            Assert.IsNotNull(supplierService);
            Assert.IsInstanceOfType(supplierService, typeof(SupplierService));
        }
    }

    internal class SupplierTestAuditLogRepository : IAuditLogRepository
    {
        public void LogLogin(int userId, string username, string machineName) { }
        public void LogLogout(int? userId, string username, string machineName) { }
        public void LogFailedAttempt(string username, string machineName, string details) { }
        public void Log(int? userId, string username, AuditAction action, string machineName, string details) { }
    }

    internal class SupplierTestSessionManager : ISessionManager
    {
        public SessionContext CurrentSession => null;
        public bool IsActive => false;
        public bool HasRole(RoleType role) => false;
        public SessionContext StartSession(User user) => null;
        public void EndSession() { }
    }
}
