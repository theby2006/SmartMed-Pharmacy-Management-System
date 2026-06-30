using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.Models.Entities;
using SmartMed.Models.Enums;

namespace SmartMed.Tests.Models
{
    [TestClass]
    public class AuditLogEntryModelTests
    {
        [TestMethod]
        public void AuditLogEntry_ShouldSetPropertiesCorrectly()
        {
            DateTime timestamp = DateTime.UtcNow;

            AuditLogEntry entry = new AuditLogEntry
            {
                Id = 1,
                UserId = 5,
                Username = "admin",
                Action = AuditAction.Login,
                MachineName = "SERVER-PC",
                Timestamp = timestamp,
                Details = "Successful login"
            };

            Assert.AreEqual(1, entry.Id);
            Assert.AreEqual(5, entry.UserId);
            Assert.AreEqual("admin", entry.Username);
            Assert.AreEqual(AuditAction.Login, entry.Action);
            Assert.AreEqual("SERVER-PC", entry.MachineName);
            Assert.AreEqual(timestamp, entry.Timestamp);
            Assert.AreEqual("Successful login", entry.Details);
        }

        [TestMethod]
        public void AuditLogEntry_DefaultValues_ShouldBeNull()
        {
            AuditLogEntry entry = new AuditLogEntry();

            Assert.AreEqual(0, entry.Id);
            Assert.IsNull(entry.UserId);
            Assert.AreEqual(default(AuditAction), entry.Action);
            Assert.IsNull(entry.Username);
            Assert.IsNull(entry.MachineName);
            Assert.AreEqual(DateTime.MinValue, entry.Timestamp);
            Assert.IsNull(entry.Details);
        }
    }
}
