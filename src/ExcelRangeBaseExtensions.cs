using System.Reflection;
using OfficeOpenXml;

namespace Delobytes.NetCore.EntityReportGeneration;

public static class ExcelRangeBaseExtensions
{
    public static ExcelRangeBase LoadFromCollectionFiltered<T>(this ExcelRangeBase @this, IEnumerable<T> collection, bool printHeaders = true) where T : class
    {
        MemberInfo[] membersToInclude = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => !Attribute.IsDefined(p, typeof(EntityReportIgnoreAttribute)))
            .ToArray();

        return @this.LoadFromCollection(collection, printHeaders,
            OfficeOpenXml.Table.TableStyles.None,
            BindingFlags.Instance | BindingFlags.Public,
            membersToInclude);
    }
}
