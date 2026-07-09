using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SmartMed.BLL.Services
{
    /// <summary>
    /// Builds a minimal, genuinely valid .xlsx (OpenXML SpreadsheetML) package
    /// from a header row and data rows, using only <see cref="System.IO.Compression.ZipArchive"/>
    /// (a .NET Framework assembly) rather than a third-party OpenXML library.
    /// Produces the smallest set of parts Excel requires: content types,
    /// package relationships, a single-sheet workbook, a bold-header style
    /// sheet, and the worksheet data itself (inline strings for text,
    /// numeric cells for numbers).
    /// </summary>
    internal static class XlsxWriter
    {
        public static byte[] Write(string sheetName, IReadOnlyList<string> headers, IReadOnlyList<object[]> rows)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    WriteEntry(archive, "[Content_Types].xml", ContentTypesXml());
                    WriteEntry(archive, "_rels/.rels", PackageRelsXml());
                    WriteEntry(archive, "xl/workbook.xml", WorkbookXml(SanitizeSheetName(sheetName)));
                    WriteEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelsXml());
                    WriteEntry(archive, "xl/styles.xml", StylesXml());
                    WriteEntry(archive, "xl/worksheets/sheet1.xml", WorksheetXml(headers, rows));
                }

                return memoryStream.ToArray();
            }
        }

        private static void WriteEntry(ZipArchive archive, string entryName, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using (Stream stream = entry.Open())
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string SanitizeSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Report";
            string cleaned = name;
            foreach (char invalid in new[] { '\\', '/', '?', '*', '[', ']', ':' })
                cleaned = cleaned.Replace(invalid, ' ');
            return cleaned.Length > 31 ? cleaned.Substring(0, 31) : cleaned;
        }

        private static string ContentTypesXml()
        {
            return
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                "</Types>";
        }

        private static string PackageRelsXml()
        {
            return
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                "</Relationships>";
        }

        private static string WorkbookXml(string sheetName)
        {
            return
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
                "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                "<sheets><sheet name=\"" + XmlEscape(sheetName) + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                "</workbook>";
        }

        private static string WorkbookRelsXml()
        {
            return
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                "</Relationships>";
        }

        private static string StylesXml()
        {
            return
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                "<fonts count=\"2\">" +
                "<font><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
                "<font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font>" +
                "</fonts>" +
                "<fills count=\"2\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill></fills>" +
                "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                "<cellXfs count=\"2\">" +
                "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
                "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>" +
                "</cellXfs>" +
                "</styleSheet>";
        }

        private static string WorksheetXml(IReadOnlyList<string> headers, IReadOnlyList<object[]> rows)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");

            sb.Append("<row r=\"1\">");
            for (int col = 0; col < headers.Count; col++)
            {
                string cellRef = GetColumnLetter(col) + "1";
                sb.Append("<c r=\"").Append(cellRef).Append("\" t=\"inlineStr\" s=\"1\"><is><t>")
                  .Append(XmlEscape(headers[col])).Append("</t></is></c>");
            }
            sb.Append("</row>");

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                int excelRow = rowIndex + 2;
                sb.Append("<row r=\"").Append(excelRow).Append("\">");

                object[] row = rows[rowIndex];
                for (int col = 0; col < row.Length; col++)
                {
                    string cellRef = GetColumnLetter(col) + excelRow;
                    object value = row[col];

                    if (IsNumeric(value, out string numericText))
                    {
                        sb.Append("<c r=\"").Append(cellRef).Append("\"><v>").Append(numericText).Append("</v></c>");
                    }
                    else
                    {
                        string text = value?.ToString() ?? string.Empty;
                        sb.Append("<c r=\"").Append(cellRef).Append("\" t=\"inlineStr\"><is><t>")
                          .Append(XmlEscape(text)).Append("</t></is></c>");
                    }
                }

                sb.Append("</row>");
            }

            sb.Append("</sheetData></worksheet>");
            return sb.ToString();
        }

        private static bool IsNumeric(object value, out string numericText)
        {
            switch (value)
            {
                case null:
                    numericText = null;
                    return false;
                case decimal d:
                    numericText = d.ToString(CultureInfo.InvariantCulture);
                    return true;
                case double dbl:
                    numericText = dbl.ToString(CultureInfo.InvariantCulture);
                    return true;
                case float f:
                    numericText = f.ToString(CultureInfo.InvariantCulture);
                    return true;
                case int i:
                    numericText = i.ToString(CultureInfo.InvariantCulture);
                    return true;
                case long l:
                    numericText = l.ToString(CultureInfo.InvariantCulture);
                    return true;
                case short sh:
                    numericText = sh.ToString(CultureInfo.InvariantCulture);
                    return true;
                default:
                    numericText = null;
                    return false;
            }
        }

        private static string GetColumnLetter(int zeroBasedIndex)
        {
            int dividend = zeroBasedIndex + 1;
            string columnName = string.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo - 1) / 26;
            }

            return columnName;
        }

        private static string XmlEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
