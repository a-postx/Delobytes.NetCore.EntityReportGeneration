using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace Delobytes.NetCore.EntityReportGeneration.Tests;

public class EntityReportGeneratorTests
{
    private static readonly string CsvDelimiter = "`";
    private static readonly Action<EntityReportGeneratorOptions> RegularOptions = options =>
    {
        options.CsvDelimiter = "`";
    };
    private static readonly Action<EntityReportGeneratorOptions> DetailedEnumerablesOptions = options =>
    {
        options.CsvDelimiter = "`";
        options.DetailedEnumerables = true;
    };


    #region Infrastructure
    private WebApplication CreateApplication(Action<EntityReportGeneratorOptions> options)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Services.AddEntityReportGenerator(options);

        WebApplication app = builder.Build();

        return app;
    }

    private IEntityReportGenerator GetReportGenerator()
    {
        WebApplication app = CreateApplication(RegularOptions);
        return app.Services.GetRequiredService<IEntityReportGenerator>();
    }

    private IEntityReportGenerator GetDetailedReportGenerator()
    {
        WebApplication app = CreateApplication(DetailedEnumerablesOptions);
        return app.Services.GetRequiredService<IEntityReportGenerator>();
    }

    private List<ObjectWithNullableList> GetNullableListEntities()
    {
        List<string> strings = new List<string> { "string1", "string2" };

        Guid id1 = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();

        ObjectWithNullableList obj1 = new ObjectWithNullableList { Id = 1, IsDeleted = true, Name = "Obj1", GuidProp = id1, Properties = strings };
        ObjectWithNullableList obj2 = new ObjectWithNullableList { Id = 2, IsDeleted = false, Name = "Obj2", GuidProp = id2, Properties = strings };

        List<ObjectWithNullableList> entitiesList = new List<ObjectWithNullableList>
        {
            obj1,
            obj2
        };

        return entitiesList;
    }

    private List<ObjectWithList> GetListEntities()
    {
        List<string> strings = new List<string> { "string1", "string2" };
        Guid id1 = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();

        ObjectWithList obj1 = new ObjectWithList { Id = 1, IsDeleted = true, Name = "Obj1", GuidProp = id1, Properties = strings };
        ObjectWithList obj2 = new ObjectWithList { Id = 2, IsDeleted = false, Name = "Obj2", GuidProp = id2, Properties = strings };

        List<ObjectWithList> entitiesList = new List<ObjectWithList>
        {
            obj1,
            obj2
        };

        return entitiesList;
    }

    private List<ObjectWithEnumerable> GetEnumerableEntities()
    {
        string[] strings = new string[2];
        strings.SetValue("string1", 0);
        strings.SetValue("string2", 1);
        Guid id1 = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();

        ObjectWithEnumerable obj1 = new ObjectWithEnumerable { Id = 1, IsDeleted = true, Name = "Obj1", GuidProp = id1, Properties = strings };
        ObjectWithEnumerable obj2 = new ObjectWithEnumerable { Id = 2, IsDeleted = false, Name = "Obj2", GuidProp = id2, Properties = strings };

        List<ObjectWithEnumerable> entitiesList = new List<ObjectWithEnumerable>
        {
            obj1,
            obj2
        };

        return entitiesList;
    }

    private List<ObjectWithNullableEnumerable> GetNullableEnumerableEntities()
    {
        string[] strings = new string[2];
        strings.SetValue("string1", 0);
        strings.SetValue("string2", 1);
        Guid id1 = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();

        ObjectWithNullableEnumerable obj1 = new ObjectWithNullableEnumerable { Id = 1, IsDeleted = true, Name = "Obj1", GuidProp = id1, Properties = strings };
        ObjectWithNullableEnumerable obj2 = new ObjectWithNullableEnumerable { Id = 2, IsDeleted = false, Name = "Obj2", GuidProp = id2, Properties = strings };

        List<ObjectWithNullableEnumerable> entitiesList = new List<ObjectWithNullableEnumerable>
        {
            obj1,
            obj2
        };

        return entitiesList;
    }
    #endregion


    [Fact]
    public void EntityReportGenerator_ConfiguredSuccessfully()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        Action configureOptions = () =>
        {
            builder.Services.AddEntityReportGenerator(RegularOptions);
        };

        Exception ex = Record.Exception(configureOptions);

        ex.Should().BeNull();
    }

    [Fact]
    public void EntityReportGenerator_GenerateExcelContentSuccessfully_FromGeneric_WithTwoElements()
    {
        IEntityReportGenerator generator = GetReportGenerator();
        List<ObjectWithList> entitiesForPage1 = GetListEntities();
        List<ObjectWithList> entitiesForPage2 = GetListEntities();

        Dictionary<string, IEnumerable<ObjectWithList>> sheets = new Dictionary<string, IEnumerable<ObjectWithList>>
        {
            { "page1", entitiesForPage1 },
            { "page2", entitiesForPage2 }
        };

        byte[]? content = null;

        Action execute = () =>
        {
            content = generator.GenerateExcelContent(sheets);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();

        using (MemoryStream stream = new MemoryStream(content!))
        using (ExcelPackage package = new ExcelPackage())
        {
            package.Load(stream);

            package.Workbook.Should().NotBeNull();
            package.Workbook.Worksheets.Count.Should().Be(2);

            ExcelWorksheet sheet = package.Workbook.Worksheets[0];

            sheet.Name.Should().Be(sheets.ElementAt(0).Key);
            sheet.Dimension.Columns.Should().Be(5);
            sheet.Dimension.Rows.Should().Be(3);

            sheet.Cells.Value.Should().NotBeNull();

            object r0c0Value = ((object[,])sheet.Cells.Value)[0, 0];
            r0c0Value.Should().Be(nameof(ObjectWithList.Id));
            object r0c1Value = ((object[,])sheet.Cells.Value)[0, 1];
            r0c1Value.Should().Be(nameof(ObjectWithList.IsDeleted));
            object r0c2Value = ((object[,])sheet.Cells.Value)[0, 2];
            r0c2Value.Should().Be(nameof(ObjectWithList.Name));
            object r0c3Value = ((object[,])sheet.Cells.Value)[0, 3];
            r0c3Value.Should().Be(nameof(ObjectWithList.GuidProp));
            object r0c4Value = ((object[,])sheet.Cells.Value)[0, 4];
            r0c4Value.Should().Be(nameof(ObjectWithList.Properties));

            object r1c0Value = ((object[,])sheet.Cells.Value)[1, 0];
            r1c0Value.Should().Be(entitiesForPage1[0].Id);
            object r1c1Value = ((object[,])sheet.Cells.Value)[1, 1];
            r1c1Value.Should().Be(entitiesForPage1[0].IsDeleted);
            object r1c2Value = ((object[,])sheet.Cells.Value)[1, 2];
            r1c2Value.Should().Be(entitiesForPage1[0].Name);
            object r1c3Value = ((object[,])sheet.Cells.Value)[1, 3];
            bool guid1Parsed = Guid.TryParse(r1c3Value.ToString(), out Guid guid1value);
            guid1Parsed.Should().Be(true);
            guid1value.Should().Be(entitiesForPage1[0].GuidProp.ToString());
            object r1c4Value = ((object[,])sheet.Cells.Value)[1, 4];
            //EPPlus заносит значение первого элемента коллекции
            r1c4Value.Should().Be(entitiesForPage1[0].Properties[0]);

            object r2c0Value = ((object[,])sheet.Cells.Value)[2, 0];
            r2c0Value.Should().Be(entitiesForPage1[1].Id);
            object r2c1Value = ((object[,])sheet.Cells.Value)[2, 1];
            r2c1Value.Should().Be(entitiesForPage1[1].IsDeleted);
            object r2c2Value = ((object[,])sheet.Cells.Value)[2, 2];
            r2c2Value.Should().Be(entitiesForPage1[1].Name);
            object r2c3Value = ((object[,])sheet.Cells.Value)[2, 3];
            bool guid2Parsed = Guid.TryParse(r2c3Value.ToString(), out Guid guid2value);
            guid2Parsed.Should().Be(true);
            guid2value.Should().Be(entitiesForPage1[1].GuidProp.ToString());
            object r2c4Value = ((object[,])sheet.Cells.Value)[2, 4];
            r2c4Value.Should().Be(entitiesForPage1[1].Properties[0]);

            ExcelWorksheet sheet2 = package.Workbook.Worksheets[1];

            sheet2.Name.Should().Be(sheets.ElementAt(1).Key);
            sheet2.Dimension.Columns.Should().Be(5);
            sheet2.Dimension.Rows.Should().Be(3);

            sheet2.Cells.Value.Should().NotBeNull();

            object s2r0c0Value = ((object[,])sheet2.Cells.Value)[0, 0];
            s2r0c0Value.Should().Be(nameof(ObjectWithList.Id));
            object s2r0c1Value = ((object[,])sheet2.Cells.Value)[0, 1];
            s2r0c1Value.Should().Be(nameof(ObjectWithList.IsDeleted));
            object s2r0c2Value = ((object[,])sheet2.Cells.Value)[0, 2];
            s2r0c2Value.Should().Be(nameof(ObjectWithList.Name));
            object s2r0c3Value = ((object[,])sheet2.Cells.Value)[0, 3];
            s2r0c3Value.Should().Be(nameof(ObjectWithList.GuidProp));
            object s2r0c4Value = ((object[,])sheet2.Cells.Value)[0, 4];
            s2r0c4Value.Should().Be(nameof(ObjectWithList.Properties));

            object s2r1c0Value = ((object[,])sheet2.Cells.Value)[1, 0];
            s2r1c0Value.Should().Be(entitiesForPage2[0].Id);
            object s2r1c1Value = ((object[,])sheet2.Cells.Value)[1, 1];
            s2r1c1Value.Should().Be(entitiesForPage2[0].IsDeleted);
            object s2r1c2Value = ((object[,])sheet2.Cells.Value)[1, 2];
            s2r1c2Value.Should().Be(entitiesForPage2[0].Name);
            object s2r1c3Value = ((object[,])sheet2.Cells.Value)[1, 3];
            bool s2guid1Parsed = Guid.TryParse(s2r1c3Value.ToString(), out Guid s2guid1value);
            s2guid1Parsed.Should().Be(true);
            s2guid1value.Should().Be(entitiesForPage2[0].GuidProp.ToString());
            object s2r1c4Value = ((object[,])sheet2.Cells.Value)[1, 4];
            //EPPlus заносит значение первого элемента коллекции
            s2r1c4Value.Should().Be(entitiesForPage2[0].Properties[0]);

            object s2r2c0Value = ((object[,])sheet2.Cells.Value)[2, 0];
            s2r2c0Value.Should().Be(entitiesForPage2[1].Id);
            object s2r2c1Value = ((object[,])sheet2.Cells.Value)[2, 1];
            s2r2c1Value.Should().Be(entitiesForPage2[1].IsDeleted);
            object s2r2c2Value = ((object[,])sheet2.Cells.Value)[2, 2];
            s2r2c2Value.Should().Be(entitiesForPage2[1].Name);
            object s2r2c3Value = ((object[,])sheet2.Cells.Value)[2, 3];
            bool s2guid2Parsed = Guid.TryParse(s2r2c3Value.ToString(), out Guid s2guid2value);
            s2guid2Parsed.Should().Be(true);
            s2guid2value.Should().Be(entitiesForPage2[1].GuidProp.ToString());
            object s2r2c4Value = ((object[,])sheet2.Cells.Value)[2, 4];
            s2r2c4Value.Should().Be(entitiesForPage2[1].Properties[0]);
        }
    }

    [Fact]
    public void EntityReportGenerator_GenerateExcelContentSuccessfully_FromExpando()
    {
        IEntityReportGenerator generator = GetReportGenerator();

        string introColumn0Header = "Заголовки";
        string introColumn0Value = "word";
        string introColumn1Header = "Цифры";
        int introColumn1Value = 100500;
        string introColumn2Header = "Флаги";
        bool introColumn2Value = true;
        string introColumn3Header = "Ид";
        Guid introColumn3Value = Guid.NewGuid();

        IDictionary<string, object> rowColumnsAndValues = new Dictionary<string, object>
        {
            { introColumn0Header, introColumn0Value },
            { introColumn1Header, introColumn1Value },
            { introColumn2Header, introColumn2Value },
            { introColumn3Header, introColumn3Value }
        };

        string objectColumn0Header = "колонка0";
        string objectColumn0Value = "значение0";
        string objectColumn1Header = "c1";
        string objectColumn1Value = "value1";
        string objectColumn2Header = "c2";
        int objectColumn2Value = 1005;
        string objectColumn3Header = 555.ToString();
        double objectColumn3Value = 99.995;
        string objectColumn4Header = "c5";
        bool objectColumn4Value = true;

        Dictionary<string, object> dynamicObject = new Dictionary<string, object>
        {
            { objectColumn0Header, objectColumn0Value },
            { objectColumn1Header, objectColumn1Value },
            { objectColumn2Header, objectColumn2Value },
            { objectColumn3Header, objectColumn3Value },
            { objectColumn4Header, objectColumn4Value }
        };

        foreach (KeyValuePair<string, object> item in dynamicObject)
        {
            rowColumnsAndValues[item.Key] = item.Value;
        }

        List<IDictionary<string, object>> sheetRows = new List<IDictionary<string, object>>
        {
            rowColumnsAndValues
        };

        string sheetName = "лист1";

        Dictionary<string, IEnumerable<IDictionary<string, object>>> sheets =
                new Dictionary<string, IEnumerable<IDictionary<string, object>>>
                {
                    { sheetName, sheetRows }
                };

        byte[]? content = null;

        Action execute = () =>
        {
            content = generator.GenerateExcelContent(sheets);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();

        using (MemoryStream stream = new MemoryStream(content!))
        using (ExcelPackage package = new ExcelPackage())
        {
            package.Load(stream);

            package.Workbook.Should().NotBeNull();
            package.Workbook.Worksheets.Count.Should().Be(1);

            ExcelWorksheet sheet = package.Workbook.Worksheets[0];

            sheet.Name.Should().Be(sheetName);
            sheet.Dimension.Columns.Should().Be(9);
            sheet.Dimension.Rows.Should().Be(2);

            sheet.Cells.Value.Should().NotBeNull();

            object introColumn0HeaderValue = ((object[,])sheet.Cells.Value)[0, 0];
            introColumn0HeaderValue.Should().Be(introColumn0Header);
            object introColumn0ValueValue = ((object[,])sheet.Cells.Value)[1, 0];
            introColumn0ValueValue.Should().Be(introColumn0Value);

            object introColumn1HeaderValue = ((object[,])sheet.Cells.Value)[0, 1];
            introColumn1HeaderValue.Should().Be(introColumn1Header);
            object introColumn1ValueValue = ((object[,])sheet.Cells.Value)[1, 1];
            introColumn1ValueValue.Should().Be(introColumn1Value);

            object introColumn2HeaderValue = ((object[,])sheet.Cells.Value)[0, 2];
            introColumn2HeaderValue.Should().Be(introColumn2Header);
            object introColumn2ValueValue = ((object[,])sheet.Cells.Value)[1, 2];
            introColumn2ValueValue.Should().Be(introColumn2Value);

            object introColumn3HeaderValue = ((object[,])sheet.Cells.Value)[0, 3];
            introColumn3HeaderValue.Should().Be(introColumn3Header);
            object introColumn3ValueValue = ((object[,])sheet.Cells.Value)[1, 3];
            bool parsed = Guid.TryParse(introColumn3ValueValue.ToString(), out Guid guid);
            parsed.Should().Be(true);
            guid.Should().Be(introColumn3Value);

            object objectColumn0HeaderValue = ((object[,])sheet.Cells.Value)[0, 4];
            objectColumn0HeaderValue.Should().Be(objectColumn0Header);
            object objectColumn0ValueValue = ((object[,])sheet.Cells.Value)[1, 4];
            objectColumn0ValueValue.Should().Be(objectColumn0Value);

            object objectColumn1HeaderValue = ((object[,])sheet.Cells.Value)[0, 5];
            objectColumn1HeaderValue.Should().Be(objectColumn1Header);
            object objectColumn1ValueValue = ((object[,])sheet.Cells.Value)[1, 5];
            objectColumn1ValueValue.Should().Be(objectColumn1Value);

            object objectColumn2HeaderValue = ((object[,])sheet.Cells.Value)[0, 6];
            objectColumn2HeaderValue.Should().Be(objectColumn2Header);
            object objectColumn2ValueValue = ((object[,])sheet.Cells.Value)[1, 6];
            objectColumn2ValueValue.Should().Be(objectColumn2Value);

            object objectColumn3HeaderValue = ((object[,])sheet.Cells.Value)[0, 7];
            objectColumn3HeaderValue.Should().Be(objectColumn3Header);
            object objectColumn3ValueValue = ((object[,])sheet.Cells.Value)[1, 7];
            objectColumn3ValueValue.Should().Be(objectColumn3Value);

            object objectColumn4HeaderValue = ((object[,])sheet.Cells.Value)[0, 8];
            objectColumn4HeaderValue.Should().Be(objectColumn4Header);
            object objectColumn4ValueValue = ((object[,])sheet.Cells.Value)[1, 8];
            objectColumn4ValueValue.Should().Be(objectColumn4Value);
        }
    }

    [Fact]
    public void EntityReportGenerator_GenerateExcelContentSuccessfully_FromDirect()
    {
        IEntityReportGenerator generator = GetReportGenerator();
        List<ObjectWithList> entities = GetListEntities();

        byte[]? content = null;

        string sheetName = "sheet1";

        Action execute = () =>
        {
            content = generator.GenerateExcelContentDirect(sheetName, entities);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();

        using (MemoryStream stream = new MemoryStream(content!))
        using (ExcelPackage package = new ExcelPackage())
        {
            package.Load(stream);

            package.Workbook.Should().NotBeNull();
            package.Workbook.Worksheets.Count.Should().Be(1);

            ExcelWorksheet sheet = package.Workbook.Worksheets[0];

            sheet.Name.Should().Be(sheetName);
            sheet.Dimension.Columns.Should().Be(5);
            sheet.Dimension.Rows.Should().Be(3);

            sheet.Cells.Value.Should().NotBeNull();

            object r0c0Value = ((object[,])sheet.Cells.Value)[0, 0];
            r0c0Value.Should().Be(nameof(ObjectWithList.Id));
            object r0c1Value = ((object[,])sheet.Cells.Value)[0, 1];
            r0c1Value.Should().Be(nameof(ObjectWithList.IsDeleted));
            object r0c2Value = ((object[,])sheet.Cells.Value)[0, 2];
            r0c2Value.Should().Be(nameof(ObjectWithList.Name));
            object r0c3Value = ((object[,])sheet.Cells.Value)[0, 3];
            r0c3Value.Should().Be(nameof(ObjectWithList.GuidProp));
            object r0c4Value = ((object[,])sheet.Cells.Value)[0, 4];
            r0c4Value.Should().Be(nameof(ObjectWithList.Properties));

            object r1c0Value = ((object[,])sheet.Cells.Value)[1, 0];
            r1c0Value.Should().Be(entities[0].Id);
            object r1c1Value = ((object[,])sheet.Cells.Value)[1, 1];
            r1c1Value.Should().Be(entities[0].IsDeleted);
            object r1c2Value = ((object[,])sheet.Cells.Value)[1, 2];
            r1c2Value.Should().Be(entities[0].Name);
            object r1c3Value = ((object[,])sheet.Cells.Value)[1, 3];
            bool guid1Parsed = Guid.TryParse(r1c3Value.ToString(), out Guid guid1value);
            guid1Parsed.Should().Be(true);
            guid1value.Should().Be(entities[0].GuidProp!.ToString());
            object r1c4Value = ((object[,])sheet.Cells.Value)[1, 4];
            //EPPlus заносит значение первого элемента коллекции
            r1c4Value.Should().Be(entities[0]?.Properties[0]);

            object r2c0Value = ((object[,])sheet.Cells.Value)[2, 0];
            r2c0Value.Should().Be(entities[1].Id);
            object r2c1Value = ((object[,])sheet.Cells.Value)[2, 1];
            r2c1Value.Should().Be(entities[1].IsDeleted);
            object r2c2Value = ((object[,])sheet.Cells.Value)[2, 2];
            r2c2Value.Should().Be(entities[1].Name);
            object r2c3Value = ((object[,])sheet.Cells.Value)[2, 3];
            bool guid2Parsed = Guid.TryParse(r2c3Value.ToString(), out Guid guid2value);
            guid2Parsed.Should().Be(true);
            guid2value.Should().Be(entities[1].GuidProp!.ToString());
            object r2c4Value = ((object[,])sheet.Cells.Value)[2, 4];
            r2c4Value.Should().Be(entities[1]?.Properties[0]);
        }
    }

    [Fact]
    public void EntityReportGenerator_GenerateCsvContentSuccessfully_WithNonNullableList()
    {
        IEntityReportGenerator generator = GetReportGenerator();
        List<ObjectWithList> objectList = GetListEntities();

        string? content = null;

        Action execute = () =>
        {
            content = generator.GenerateCsvContent(objectList);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();
        content.Should().Contain(CsvDelimiter);
        content.Should().Contain(nameof(ObjectWithList.Id));
        content.Should().Contain(nameof(ObjectWithList.IsDeleted));
        content.Should().Contain(nameof(ObjectWithList.Name));
        content.Should().Contain(nameof(ObjectWithList.GuidProp));
        content.Should().Contain(nameof(ObjectWithList.Properties));
        content.Should().Contain(objectList[0].Id.ToString());
        content.Should().Contain(objectList[1].Id.ToString());
        content.Should().Contain(objectList[0].IsDeleted.ToString());
        content.Should().Contain(objectList[1].IsDeleted.ToString());
        content.Should().Contain(objectList[0].Name);
        content.Should().Contain(objectList[1].Name);
        content.Should().Contain(objectList[0].GuidProp.ToString());
        content.Should().Contain(objectList[1].GuidProp.ToString());
        content.Should().Contain(objectList[0].Properties!.ToString()!.Replace("`", ""));
        content.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void EntityReportGenerator_GenerateCsvContentSuccessfully_WithNullableList()
    {
        IEntityReportGenerator generator = GetReportGenerator();
        List<ObjectWithNullableList> objectList = GetNullableListEntities();

        string? content = null;

        Action execute = () =>
        {
            content = generator.GenerateCsvContent(objectList);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();
        content.Should().Contain(CsvDelimiter);
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Id));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.IsDeleted));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Name));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.GuidProp));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Properties));
        content.Should().Contain(objectList[0].Id.ToString());
        content.Should().Contain(objectList[1].Id.ToString());
        content.Should().Contain(objectList[0].IsDeleted.ToString());
        content.Should().Contain(objectList[1].IsDeleted.ToString());
        content.Should().Contain(objectList[0].Name);
        content.Should().Contain(objectList[1].Name);
        content.Should().Contain(objectList[0].GuidProp.ToString());
        content.Should().Contain(objectList[1].GuidProp.ToString());
        content.Should().Contain(objectList[0].Properties!.ToString()!.Replace("`", ""));
        content.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void EntityReportGenerator_GenerateCsvContentSuccessfully_WithEnumerable()
    {
        IEntityReportGenerator generator = GetReportGenerator();
        List<ObjectWithEnumerable> objectList = GetEnumerableEntities();

        string? content = null;

        Action execute = () =>
        {
            content = generator.GenerateCsvContent(objectList);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();
        content.Should().Contain(CsvDelimiter);
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Id));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.IsDeleted));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Name));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.GuidProp));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Properties));
        content.Should().Contain(objectList[0].Id.ToString());
        content.Should().Contain(objectList[1].Id.ToString());
        content.Should().Contain(objectList[0].IsDeleted.ToString());
        content.Should().Contain(objectList[1].IsDeleted.ToString());
        content.Should().Contain(objectList[0].Name);
        content.Should().Contain(objectList[1].Name);
        content.Should().Contain(objectList[0].GuidProp.ToString());
        content.Should().Contain(objectList[1].GuidProp.ToString());
        content.Should().Contain(objectList[0].Properties!.ToString()!.Replace("`", ""));
        content.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void EntityReportGenerator_GenerateCsvContentSuccessfully_WithNullableEnumerable()
    {
        IEntityReportGenerator generator = GetReportGenerator();
        List<ObjectWithNullableEnumerable> objectList = GetNullableEnumerableEntities();

        string? content = null;

        Action execute = () =>
        {
            content = generator.GenerateCsvContent(objectList);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();
        content.Should().Contain(CsvDelimiter);
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Id));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.IsDeleted));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Name));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.GuidProp));
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Properties));
        content.Should().Contain(objectList[0].Id.ToString());
        content.Should().Contain(objectList[1].Id.ToString());
        content.Should().Contain(objectList[0].IsDeleted.ToString());
        content.Should().Contain(objectList[1].IsDeleted.ToString());
        content.Should().Contain(objectList[0].Name);
        content.Should().Contain(objectList[1].Name);
        content.Should().Contain(objectList[0].GuidProp.ToString());
        content.Should().Contain(objectList[1].GuidProp.ToString());
        content.Should().Contain(objectList[0].Properties!.ToString()!.Replace("`", ""));
        content.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void EntityReportGenerator_GenerateCsvContentSuccessfully_WithDetailedEnumerable()
    {
        IEntityReportGenerator generator = GetDetailedReportGenerator();
        List<ObjectWithEnumerable> objectList = GetEnumerableEntities();

        string? content = null;

        Action execute = () =>
        {
            content = generator.GenerateCsvContent(objectList);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();
        content.Should().Contain(CsvDelimiter);
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Properties));
        content.Should().Contain(string.Join(",", objectList[0].Properties!));
        content.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void EntityReportGenerator_GenerateCsvContentSuccessfully_WithListAndDetailedEnumerable()
    {
        IEntityReportGenerator generator = GetDetailedReportGenerator();
        List<ObjectWithList> objectList = GetListEntities();

        string? content = null;

        Action execute = () =>
        {
            content = generator.GenerateCsvContent(objectList);
        };

        Exception ex = Record.Exception(execute);

        ex.Should().BeNull();
        content.Should().NotBeNull();
        content.Should().Contain(CsvDelimiter);
        content.Should().Contain(nameof(ObjectWithNullableEnumerable.Properties));
        content.Should().Contain(string.Join(",", objectList[0].Properties!));
        content.Should().Contain(Environment.NewLine);
    }
}
