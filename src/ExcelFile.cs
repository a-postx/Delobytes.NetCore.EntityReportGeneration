using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace Delobytes.NetCore.EntityReportGeneration;

internal class ExcelFile : IDisposable
{
    internal ExcelFile()
    {
        _package = new ExcelPackage();
    }

    private readonly ExcelPackage _package;
    private bool _disposedValue;

    protected IList<ExcelWorksheet> WorkSheets { get; } = new List<ExcelWorksheet>();


    internal void SetAuthor(string author)
    {
        _package.Workbook.Properties.Author = author;
    }

    internal void SetSubject(string subject)
    {
        _package.Workbook.Properties.Subject = subject;
    }

    internal void SetCompany(string company)
    {
        _package.Workbook.Properties.Company = company;
    }

    internal void AddWorkSheet(string name, DataTable table, bool printHeaders = true, int rowHeight = 15, int columnWidth = 12)
    {
        ExcelWorksheet worksheet = _package.Workbook.Worksheets.Add(name);
        WorkSheets.Add(worksheet);

        worksheet.DefaultRowHeight = rowHeight;
        worksheet.DefaultColWidth = columnWidth;

        worksheet.Cells["A1"].LoadFromDataTable(table, printHeaders, TableStyles.None);
    }

    internal void AddWorkSheet<T>(string name, IEnumerable<T> collection, bool printHeaders = true, int rowHeight = 15, int columnWidth = 12) where T : class
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(collection);

        ExcelWorksheet worksheet = _package.Workbook.Worksheets.Add(name);
        WorkSheets.Add(worksheet);

        worksheet.DefaultRowHeight = rowHeight;
        worksheet.DefaultColWidth = columnWidth;

        worksheet.Cells["A1"].LoadFromCollectionFiltered(collection, printHeaders);
    }

    internal void AddWorkSheet(string name, IEnumerable<IDictionary<string, object>> valueDics, bool printHeaders = true, bool setAutoFilter = false, int rowHeight = 15, int columnWidth = 12)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(valueDics);

        ExcelWorksheet worksheet = _package.Workbook.Worksheets.Add(name);
        WorkSheets.Add(worksheet);

        worksheet.DefaultRowHeight = rowHeight;
        worksheet.DefaultColWidth = columnWidth;

        if (printHeaders)
        {
            IDictionary<string, object> headerObject = valueDics.FirstOrDefault();

            if (headerObject is not null)
            {
                int headerCol = 1;

                foreach (string item in headerObject.Keys)
                {
                    worksheet.SetValue(1, headerCol, item);
                    headerCol++;
                }
            }
        }

        int col = 1;
        int row = printHeaders ? 2 : 1;

        foreach (IDictionary<string, object> dataObject in valueDics)
        {
            foreach (object item in dataObject.Values)
            {
                worksheet.SetValue(row, col, item);
                col++;
            }

            row++;
            col = 1;
        }

        if (setAutoFilter)
        {
            worksheet.Cells[worksheet.Dimension.Address].AutoFilter = true;
        }
    }

    internal byte[] GetContent()
    {
        byte[] result = _package.GetAsByteArray();
        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _package.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
