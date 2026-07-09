using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartMed.BLL.Interfaces;
using SmartMed.BLL.Services;
using SmartMed.Models.Reports;
using SmartMed.Models.Results;
using SmartMed.Reports;

namespace SmartMed.Tests.BLL
{
    [TestClass]
    public class ExportTests
    {
        private IReportService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _service = new ReportService(new MockReportRepository());
        }

        private static List<CategorySummaryRow> SampleRows()
        {
            return new List<CategorySummaryRow>
            {
                new CategorySummaryRow { CategoryName = "Antibiotics", MedicineCount = 4, TotalStockQuantity = 500, TotalStockValue = 1234.56m },
                new CategorySummaryRow { CategoryName = "Analgesics", MedicineCount = 2, TotalStockQuantity = 1800, TotalStockValue = 8100.00m }
            };
        }

        [TestMethod]
        public void ExportToExcel_ShouldProduceValidZipPackage()
        {
            OperationResult<byte[]> result = _service.ExportToExcel(SampleRows());

            Assert.IsTrue(result.IsSuccess);

            using (MemoryStream stream = new MemoryStream(result.Data))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                string[] entryNames = archive.Entries.Select(e => e.FullName).ToArray();

                CollectionAssert.Contains(entryNames, "[Content_Types].xml");
                CollectionAssert.Contains(entryNames, "_rels/.rels");
                CollectionAssert.Contains(entryNames, "xl/workbook.xml");
                CollectionAssert.Contains(entryNames, "xl/_rels/workbook.xml.rels");
                CollectionAssert.Contains(entryNames, "xl/styles.xml");
                CollectionAssert.Contains(entryNames, "xl/worksheets/sheet1.xml");
            }
        }

        [TestMethod]
        public void ExportToExcel_SheetShouldContainHeadersAndValues()
        {
            OperationResult<byte[]> result = _service.ExportToExcel(SampleRows());

            using (MemoryStream stream = new MemoryStream(result.Data))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
                Assert.IsNotNull(sheetEntry);

                string xml;
                using (StreamReader reader = new StreamReader(sheetEntry.Open(), Encoding.UTF8))
                    xml = reader.ReadToEnd();

                StringAssert.Contains(xml, "CategoryName");
                StringAssert.Contains(xml, "Antibiotics");
                StringAssert.Contains(xml, "1234.56");
                StringAssert.Contains(xml, "<sheetData>");
                StringAssert.Contains(xml, "</sheetData>");
            }
        }

        [TestMethod]
        public void ExportToExcel_ShouldFail_WhenDataIsEmpty()
        {
            OperationResult<byte[]> result = _service.ExportToExcel(new List<CategorySummaryRow>());

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void ExportToPdf_ShouldProduceValidPdfStructure()
        {
            OperationResult<byte[]> result = _service.ExportToPdf("Category Summary", SampleRows());

            Assert.IsTrue(result.IsSuccess);

            string content = Encoding.GetEncoding("ISO-8859-1").GetString(result.Data);

            StringAssert.StartsWith(content, "%PDF-1.4");
            Assert.IsTrue(content.TrimEnd().EndsWith("%%EOF"));
            StringAssert.Contains(content, "/Type /Catalog");
            StringAssert.Contains(content, "/Type /Pages");
            StringAssert.Contains(content, "/Type /Page");
            StringAssert.Contains(content, "endobj");
            StringAssert.Contains(content, "xref");
            StringAssert.Contains(content, "trailer");
            StringAssert.Contains(content, "Antibiotics");
        }

        [TestMethod]
        public void ExportToPdf_ShouldFail_WhenDataIsEmpty()
        {
            OperationResult<byte[]> result = _service.ExportToPdf("Empty", new List<CategorySummaryRow>());

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void PdfExporter_ExportTable_ShouldPaginate_WhenManyRows()
        {
            string[] headers = { "Name", "Value" };
            List<string[]> rows = Enumerable.Range(1, 200)
                .Select(i => new[] { $"Item{i}", i.ToString() })
                .ToList();

            byte[] bytes = PdfExporter.ExportTable("Big Report", headers, rows);
            string content = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);

            StringAssert.Contains(content, "Page 1 of");
            StringAssert.Contains(content, "Item1");
            StringAssert.Contains(content, "Item200");

            int pageObjectCount = CountOccurrences(content, "/Type /Page ");
            Assert.IsTrue(pageObjectCount > 1, "Expected more than one page for 200 rows.");
        }

        [TestMethod]
        public void PdfExporter_ExportTable_ShouldEscapeParenthesesAndBackslashes()
        {
            string[] headers = { "Name" };
            List<string[]> rows = new List<string[]> { new[] { "Item (special)" } };

            byte[] bytes = PdfExporter.ExportTable("Escaping", headers, rows);
            string content = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);

            StringAssert.Contains(content, "\\(special\\)");
        }

        private static int CountOccurrences(string text, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(token, index)) != -1)
            {
                count++;
                index += token.Length;
            }
            return count;
        }
    }
}
