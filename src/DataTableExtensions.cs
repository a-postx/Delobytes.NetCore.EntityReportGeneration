using System.Data;
using System.Text;

namespace Delobytes.NetCore.EntityReportGeneration;

internal static class DataTableExtensions
{
    public static string ToCSV(this DataTable dataTable, string delimiter)
    {
        string result = null;

        StringBuilder sb = new StringBuilder();
        IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
        sb.AppendLine(string.Join(delimiter, columnNames));

        foreach (DataRow row in dataTable.Rows)
        {
            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
            sb.AppendLine(string.Join(delimiter, fields));
        }

        result = sb.ToString();

        return result;
    }
}
