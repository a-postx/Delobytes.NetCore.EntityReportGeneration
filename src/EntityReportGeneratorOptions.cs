namespace Delobytes.NetCore.EntityReportGeneration;

/// <summary>
/// Настройки генератора.
/// </summary>
public class EntityReportGeneratorOptions
{
    /// <summary>
    /// Символ-разделитель для вывода в CSV. 
    /// </summary>
    public string CsvDelimiter { get; set; } = "U+002C";
    /// <summary>
    /// Признак необходимости подробного вывода перечислимых свойств. 
    /// </summary>
    public bool DetailedEnumerables { get; set; } = false;
}
