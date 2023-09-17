namespace Delobytes.NetCore.EntityReportGeneration.Tests;

public class ObjectWithNullableList
{
    public int? Id { get; set; }
    public bool? IsDeleted { get; set; }
    public string? Name { get; set; }
    public Guid? GuidProp { get; set; }
    public List<string>? Properties { get; set; }
}
