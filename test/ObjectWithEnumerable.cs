namespace Delobytes.NetCore.EntityReportGeneration.Tests;

public class ObjectWithEnumerable
{
    public int? Id { get; set; }
    public bool? IsDeleted { get; set; }
    public string? Name { get; set; }
    public Guid? GuidProp { get; set; }
    public IEnumerable<string> Properties { get; set; } = Array.Empty<string>();
}
