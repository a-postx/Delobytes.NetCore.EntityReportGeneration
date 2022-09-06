namespace Delobytes.NetCore.EntityReportGeneration.Tests;

public class ObjectWithGenericList
{
    public int? Id { get; set; }
    public bool? IsDeleted { get; set; }
    public string? Name { get; set; }
    public Guid? ObjGuid { get; set; }
    public IEnumerable<string>? Properties { get; set; }
}
