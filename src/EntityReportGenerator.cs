using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delobytes.NetCore.EntityReportGeneration;

/// <summary>
/// Генератор отчёта о содержимом свойств объектов.
/// todo: добавить работу с потоком для вывода больших файлов с экономией памяти.
/// </summary>
public class EntityReportGenerator : IEntityReportGenerator
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="options">Настройки генератора.</param>
    /// <param name="logger">Логировщик.</param>
    public EntityReportGenerator(IOptions<EntityReportGeneratorOptions> options,
        ILogger<EntityReportGenerator>? logger = null)
    {
        _options = options.Value;
        _log = logger;
    }

    private readonly EntityReportGeneratorOptions _options;
    private readonly ILogger<EntityReportGenerator>? _log;

    private static string RemoveDelimiterChar(string s, string delimiter)
    {
        return string.Join(string.Empty, s.Split(delimiter.ToCharArray()));
    }

    private static string RemoveExcelSheetInvalidChars(string input)
    {
        Regex regex = new Regex(@"[\s:?*`<>_\[\]/\\]+");
        return regex.Replace(input, "");
    }

    private DataTable ConvertObjectsToExcelDataTable<T>(IEnumerable<T> items, string tableName, string stringToCleanup = "")
    {
        items = items ?? throw new ArgumentNullException(nameof(items));

        DataTable result = new DataTable(tableName);

        try
        {
            PropertyInfo[] rawProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            PropertyInfo[] properties = rawProperties
                .Where(e => !e.GetCustomAttributesData()
                    .Any(a => a.AttributeType.Name == nameof(EntityReportIgnoreAttribute))).ToArray();

            foreach (PropertyInfo prop in properties)
            {
                Type propType = prop.PropertyType;

                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propType = new NullableConverter(propType).UnderlyingType;
                }

                result.Columns.Add(prop.Name, propType);
            }

            foreach (T item in items)
            {
                //new DataRow() выводит числа как текст, поэтому используем прямые объекты
                object[] objectRow = new object[properties.Length];

                for (var i = 0; i < properties.Length; i++)
                {
                    object? objValue = properties[i].GetValue(item, null);
                    objectRow[i] = objValue ?? string.Empty;
                }

                result.Rows.Add(objectRow);
            }
        }
        catch (Exception ex)
        {
            _log?.LogError(ex, "Error convering objects of type {TypeName} to DataTable", typeof(T).GetFriendlyName());
            throw;
        }

        return result;
    }

    private static string ConvertToString(object? property, string stringToCleanup)
    {
        string cellValue = property?.ToString() ?? string.Empty;
        return string.IsNullOrEmpty(stringToCleanup) ? cellValue : RemoveDelimiterChar(cellValue, stringToCleanup);
    }

    ///<inheritdoc/>
    public byte[] GenerateExcelContent<T>(IDictionary<string, IEnumerable<T>> pagesDataset) where T : class
    {
        byte[] result;

        using (ExcelFile file = new ExcelFile())
        {
            foreach (KeyValuePair<string, IEnumerable<T>> page in pagesDataset)
            {
                string safeSheetName = RemoveExcelSheetInvalidChars(page.Key);
                using (DataTable dataTable = ConvertObjectsToExcelDataTable(page.Value, safeSheetName))
                {
                    file.AddWorkSheet(safeSheetName, dataTable);
                }
            }

            result = file.GetContent();
        }

        return result;
    }

    ///<inheritdoc/>
    public byte[] GenerateExcelContent(IDictionary<string, IEnumerable<IDictionary<string, object>>> pagesDataset)
    {
        byte[] result;

        using (ExcelFile file = new ExcelFile())
        {
            foreach (KeyValuePair<string, IEnumerable<IDictionary<string, object>>> page in pagesDataset)
            {
                string safeSheetName = RemoveExcelSheetInvalidChars(page.Key);
                file.AddWorkSheet(safeSheetName, page.Value, true, true);
            }

            result = file.GetContent();
        }

        return result;
    }

    ///<inheritdoc/>
    public byte[] GenerateExcelContentDirect<T>(string sheetName, IEnumerable<T> dataset) where T : class
    {
        byte[] result;

        using (ExcelFile file = new ExcelFile())
        {
            string safeSheetName = RemoveExcelSheetInvalidChars(sheetName);
            file.AddWorkSheet(safeSheetName, dataset);

            result = file.GetContent();
        }

        return result;
    }

    private DataTable ConvertObjectsToDataTableForCsv<T>(IEnumerable<T> items, string tableName, string stringToCleanup = "")
    {
        items = items ?? throw new ArgumentNullException(nameof(items));

        DataTable result = new DataTable(tableName);
        StringBuilder sb = new StringBuilder();

        try
        {
            PropertyInfo[] rawProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            PropertyInfo[] properties = rawProperties
                .Where(e => !e.GetCustomAttributesData()
                    .Any(a => a.AttributeType.Name == nameof(EntityReportIgnoreAttribute))).ToArray();

            foreach (PropertyInfo p in properties)
            {
                result.Columns.Add(p.Name, p.PropertyType == typeof(int) ? typeof(int) : typeof(string));
            }

            foreach (T item in items)
            {
                DataRow dataRow = result.NewRow();

                for (int i = 0; i < properties.Length; i++)
                {
                    object? value = properties[i].GetValue(item, null);

                    if (properties[i].PropertyType == typeof(bool)
                        || properties[i].PropertyType == typeof(int)
                        || properties[i].PropertyType == typeof(string)
                        || properties[i].PropertyType == typeof(Guid))
                    {
                        dataRow[i] = ConvertToString(value, stringToCleanup);
                        continue;
                    }

                    if (_options.DetailedEnumerables && value is IEnumerable enumerable)
                    {
                        sb.Clear();
                        bool firstElement = true;

                        foreach (object element in enumerable)
                        {
                            if (firstElement)
                            {
                                firstElement = false;
                            }
                            else
                            {
                                sb.Append(',');
                            }

                            sb.Append(element.ToString());
                        }

                        dataRow[i] = ConvertToString(sb.ToString(), stringToCleanup);
                    }
                    else
                    {
                        dataRow[i] = ConvertToString(value, stringToCleanup);
                    }
                }

                result.Rows.Add(dataRow);
            }
        }
        catch (Exception ex)
        {
            _log?.LogError(ex, "Error convering objects of type {TypeName} to DataTable", typeof(T).GetFriendlyName());
            throw;
        }

        return result;
    }

    /// <summary>
    /// Сгенерировать содержимое csv-файла для объектов определённых в виде класса.
    /// </summary>
    /// <param name="dataset">Набор объектов.</param>
    /// <returns>Содержимое файла.</returns>
    public string GenerateCsvContent<T>(IEnumerable<T> dataset) where T : class
    {
        string name = typeof(T).GetFriendlyName();
        string safeName = RemoveExcelSheetInvalidChars(name); //на всякий случай

        string result = string.Empty;

        using (DataTable dataTable = ConvertObjectsToDataTableForCsv(dataset, safeName, _options.CsvDelimiter))
        {
            result = dataTable.ToCSV(_options.CsvDelimiter);
        }

        return result;
    }
}
