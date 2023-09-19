using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Delobytes.NetCore.EntityReportGeneration.Tests;

[MemoryDiagnoser]
public class GeneratorBechmarkTests
{
    private static readonly Action<EntityReportGeneratorOptions> RegularOptions = options =>
    {
        options.CsvDelimiter = "`";
    };
    private static readonly Action<EntityReportGeneratorOptions> DetailedOptions = options =>
    {
        options.CsvDelimiter = "`";
        options.DetailedEnumerables = true;
    };

    [GlobalSetup]
    public void Setup()
    {

    }

    private List<ObjectWithEnumerable> Get100kEntities()
    {
        List<ObjectWithEnumerable> entitiesList = new List<ObjectWithEnumerable>(99999);

        for (int i = 0; i < 99999; i++)
        {
            string[] strings = new string[2];
            strings.SetValue("string1 " + i, 0);
            strings.SetValue("string2 " + i, 1);

            ObjectWithEnumerable obj = new ObjectWithEnumerable
            {
                Id = i,
                Name = "Name " + i,
                IsDeleted = (i % 2) == 0,
                GuidProp = Guid.NewGuid(),
                Properties = strings
            };

            entitiesList.Add(obj);
        }

        return entitiesList;
    }

    private List<ObjectWithEnumerable> Get10kEntitiesWith18Items()
    {
        List<ObjectWithEnumerable> entitiesList = new List<ObjectWithEnumerable>(9999);

        for (int i = 0; i < 9999; i++)
        {
            string[] strings = new string[18];
            strings.SetValue("string1 " + i, 0);
            strings.SetValue("string2 " + i, 1);
            strings.SetValue("string3 " + i, 2);
            strings.SetValue("string4 " + i, 3);
            strings.SetValue("string5 " + i, 4);
            strings.SetValue("string6 " + i, 5);
            strings.SetValue("string7 " + i, 6);
            strings.SetValue("string8 " + i, 7);
            strings.SetValue("string9 " + i, 8);
            strings.SetValue("string10 " + i, 9);
            strings.SetValue("string11 " + i, 10);
            strings.SetValue("string12 " + i, 11);
            strings.SetValue("string13 " + i, 12);
            strings.SetValue("string14 " + i, 13);
            strings.SetValue("string15 " + i, 14);
            strings.SetValue("string16 " + i, 15);
            strings.SetValue("string17 " + i, 16);
            strings.SetValue("string18 " + i, 17);

            ObjectWithEnumerable obj = new ObjectWithEnumerable
            {
                Id = i,
                Name = "Name " + i,
                IsDeleted = (i % 2) == 0,
                GuidProp = Guid.NewGuid(),
                Properties = strings
            };

            entitiesList.Add(obj);
        }

        return entitiesList;
    }

    [Benchmark]
    public void Large_1M_Entities_Exported()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddEntityReportGenerator(RegularOptions);
        WebApplication app = builder.Build();
        IEntityReportGenerator generator = app.Services.GetRequiredService<IEntityReportGenerator>();

        List<ObjectWithEnumerable> objectList = Get100kEntities();

        generator.GenerateCsvContent(objectList);
    }

    [Benchmark]
    public void Medium_10k_Entities_With18items_InDetails_Exported()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddEntityReportGenerator(DetailedOptions);
        WebApplication app = builder.Build();
        IEntityReportGenerator generator = app.Services.GetRequiredService<IEntityReportGenerator>();

        List<ObjectWithEnumerable> objectList = Get10kEntitiesWith18Items();

        generator.GenerateCsvContent(objectList);
    }
}
