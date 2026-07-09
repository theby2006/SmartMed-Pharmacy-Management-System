using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartMed.Reports
{
    /// <summary>
    /// A minimal, from-scratch PDF 1.4 writer: no third-party PDF library,
    /// just the object/xref/trailer structure the PDF specification requires.
    /// Produces a paginated tabular report (repeated header row per page,
    /// "Page N of M" footer) using the two built-in Type1 fonts every PDF
    /// reader supports (Helvetica-Bold for the title, Courier for the
    /// monospaced table body, which keeps column alignment correct without
    /// per-cell positioning math).
    /// </summary>
    public static class PdfExporter
    {
        private const int PageWidth = 612;
        private const int PageHeight = 792;
        private const int LeftMargin = 40;
        private const int TopMargin = 750;
        private const int BottomMargin = 50;
        private const int LineHeight = 12;
        private const int TableFontSize = 8;
        private const int MaxLineChars = 110;

        public static byte[] ExportTable(string title, string[] headers, List<string[]> rows)
        {
            Encoding encoding = Encoding.GetEncoding("ISO-8859-1");

            int[] columnWidths = ComputeColumnWidths(headers, rows);
            List<string> headerLine = new List<string> { FormatRow(headers, columnWidths) };

            int dataStartY = TopMargin - (LineHeight * 5);
            int rowsPerPage = Math.Max(1, (dataStartY - BottomMargin) / LineHeight);

            List<List<string[]>> pages = Chunk(rows, rowsPerPage);
            if (pages.Count == 0) pages.Add(new List<string[]>());
            int totalPages = pages.Count;

            List<byte[]> objects = new List<byte[]>();
            List<int> offsets = new List<int>();

            // Object 1: Catalog, Object 2: Pages (filled in after page objects are known)
            objects.Add(null);
            objects.Add(null);

            // Object 3: Helvetica-Bold, Object 4: Courier
            objects.Add(encoding.GetBytes("3 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>\nendobj\n"));
            objects.Add(encoding.GetBytes("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>\nendobj\n"));

            List<int> pageObjectNumbers = new List<int>();
            int nextObjectNumber = 5;

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                int pageObjNum = nextObjectNumber++;
                int contentObjNum = nextObjectNumber++;
                pageObjectNumbers.Add(pageObjNum);

                string content = BuildPageContentStream(title, headerLine[0], pages[pageIndex], columnWidths, pageIndex + 1, totalPages);
                byte[] contentBytes = encoding.GetBytes(content);

                string contentObj =
                    contentObjNum + " 0 obj\n<< /Length " + contentBytes.Length + " >>\nstream\n" +
                    content + "\nendstream\nendobj\n";

                string pageObj =
                    pageObjNum + " 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + PageWidth + " " + PageHeight + "] " +
                    "/Resources << /Font << /FBold 3 0 R /FMono 4 0 R >> >> /Contents " + contentObjNum + " 0 R >>\nendobj\n";

                // Ensure the objects list has exactly enough slots for contentObjNum
                // (index contentObjNum - 1); using "<=" here would leave one
                // trailing null entry that later crashes the writer.
                while (objects.Count < contentObjNum) objects.Add(null);
                objects[pageObjNum - 1] = encoding.GetBytes(pageObj);
                objects[contentObjNum - 1] = encoding.GetBytes(contentObj);
            }

            string kids = string.Join(" ", pageObjectNumbers.Select(n => n + " 0 R"));
            objects[0] = encoding.GetBytes("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
            objects[1] = encoding.GetBytes("2 0 obj\n<< /Type /Pages /Kids [" + kids + "] /Count " + pageObjectNumbers.Count + " >>\nendobj\n");

            using (MemoryStream stream = new MemoryStream())
            {
                WriteAscii(stream, "%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

                foreach (byte[] obj in objects)
                {
                    offsets.Add((int)stream.Length);
                    stream.Write(obj, 0, obj.Length);
                }

                int xrefOffset = (int)stream.Length;
                int objectCount = objects.Count + 1;

                StringBuilder xref = new StringBuilder();
                xref.Append("xref\n0 ").Append(objectCount).Append('\n');
                xref.Append("0000000000 65535 f \n");
                foreach (int offset in offsets)
                {
                    xref.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
                }

                WriteAscii(stream, xref.ToString());
                WriteAscii(stream, "trailer\n<< /Size " + objectCount + " /Root 1 0 R >>\nstartxref\n" + xrefOffset + "\n%%EOF");

                return stream.ToArray();
            }
        }

        private static void WriteAscii(Stream stream, string text)
        {
            byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static int[] ComputeColumnWidths(string[] headers, List<string[]> rows)
        {
            int columnCount = headers.Length;
            int[] widths = new int[columnCount];

            for (int c = 0; c < columnCount; c++)
                widths[c] = headers[c]?.Length ?? 0;

            foreach (string[] row in rows)
            {
                for (int c = 0; c < columnCount && c < row.Length; c++)
                {
                    int len = row[c]?.Length ?? 0;
                    if (len > widths[c]) widths[c] = len;
                }
            }

            int budget = MaxLineChars - (columnCount * 2);
            int totalWidth = widths.Sum();

            if (totalWidth > budget && totalWidth > 0)
            {
                double scale = (double)budget / totalWidth;
                for (int c = 0; c < columnCount; c++)
                    widths[c] = Math.Max(4, (int)(widths[c] * scale));
            }

            return widths;
        }

        private static string FormatRow(string[] cells, int[] columnWidths)
        {
            StringBuilder sb = new StringBuilder();
            for (int c = 0; c < columnWidths.Length; c++)
            {
                string cell = c < cells.Length ? (cells[c] ?? string.Empty) : string.Empty;
                if (cell.Length > columnWidths[c])
                    cell = cell.Substring(0, Math.Max(0, columnWidths[c] - 1)) + "…";
                sb.Append(cell.PadRight(columnWidths[c] + 2));
            }
            return sb.ToString();
        }

        private static List<List<string[]>> Chunk(List<string[]> rows, int chunkSize)
        {
            List<List<string[]>> chunks = new List<List<string[]>>();
            for (int i = 0; i < rows.Count; i += chunkSize)
                chunks.Add(rows.Skip(i).Take(chunkSize).ToList());
            return chunks;
        }

        private static string BuildPageContentStream(string title, string headerLine, List<string[]> pageRows, int[] columnWidths, int pageNumber, int totalPages)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BT\n");
            sb.Append("/FBold 14 Tf\n");
            sb.Append(LeftMargin).Append(' ').Append(TopMargin).Append(" Td\n");
            sb.Append('(').Append(EscapePdfText(title)).Append(") Tj\n");

            sb.Append("/FMono ").Append(TableFontSize - 1).Append(" Tf\n");
            sb.Append("0 -").Append(LineHeight + 4).Append(" Td\n");
            sb.Append('(').Append(EscapePdfText("Generated " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"))).Append(") Tj\n");

            sb.Append("/FMono ").Append(TableFontSize).Append(" Tf\n");
            sb.Append("0 -").Append(LineHeight * 2).Append(" Td\n");
            sb.Append('(').Append(EscapePdfText(headerLine)).Append(") Tj\n");

            foreach (string[] row in pageRows)
            {
                sb.Append("0 -").Append(LineHeight).Append(" Td\n");
                sb.Append('(').Append(EscapePdfText(FormatRow(row, columnWidths))).Append(") Tj\n");
            }

            sb.Append("ET\n");

            sb.Append("BT\n");
            sb.Append("/FMono 8 Tf\n");
            sb.Append(LeftMargin).Append(' ').Append(BottomMargin - 20).Append(" Td\n");
            sb.Append('(').Append(EscapePdfText($"Page {pageNumber} of {totalPages}")).Append(") Tj\n");
            sb.Append("ET\n");

            return sb.ToString();
        }

        private static string EscapePdfText(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            StringBuilder sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c == '\\' || c == '(' || c == ')')
                    sb.Append('\\').Append(c);
                else if (c > 255)
                    sb.Append('?');
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
