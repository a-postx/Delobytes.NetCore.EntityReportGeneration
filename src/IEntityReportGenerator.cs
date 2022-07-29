namespace Delobytes.NetCore.EntityReportGeneration;

public interface IEntityReportGenerator
{
    /// <summary>
    /// Сгенерировать содержимое csv-файла для объектов определённых в виде класса.
    /// </summary>
    /// <param name="dataset">Набор объектов.</param>
    /// <returns>Содержимое файла.</returns>
    string GenerateCsvContent<T>(IEnumerable<T> dataset) where T : class;

    /// <summary>
    /// Сгенерировать содержимое эксель-файла для ряда страниц. Страница состоит из названия
    /// и ряда строк.
    /// Метод подходит для экспорта объектов определённых в виде класса.
    /// </summary>
    /// <param name="pagesDataset">Набор объектов для преобразования в страницы файла</param>
    /// <returns>Содержимое файла.</returns>
    byte[] GenerateExcelContent<T>(IDictionary<string, IEnumerable<T>> pagesDataset) where T : class;
    /// <summary>
    /// Сгенерировать содержимое эксель-файла для ряда страниц. Страница состоит из названия
    /// и ряда строк.
    /// Метод подходит для экспорта динамически создаваемых объектов типа ExpandoObject.
    /// </summary>
    /// <param name="pagesDataset">Набор объектов для преобразования в страницы файла</param>
    /// <returns>Содержимое файла.</returns>
    byte[] GenerateExcelContent(IDictionary<string, IEnumerable<IDictionary<string, object>>> pagesDataset);
    /// <summary>
    /// Сгенерировать содержимое эксель-файла для одной страницы без использования
    /// промежуточного преобразования к таблице.
    /// Метод подходит для экспорта объектов определённых в виде класса.
    /// </summary>
    /// <typeparam name="T">Тип.</typeparam>
    /// <param name="sheetName">Название страницы.</param>
    /// <param name="dataset">Список объектов.</param>
    /// <returns>Содержимое файла.</returns>
    byte[] GenerateExcelContentDirect<T>(string sheetName, IEnumerable<T> dataset) where T : class;
}
