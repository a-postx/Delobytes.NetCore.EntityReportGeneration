namespace Delobytes.NetCore.EntityReportGeneration.Tests;

public class ObjectWithTypedList
{
    public int? Id { get; set; }
    public bool? IsDeleted { get; set; }
    public string? Name { get; set; }
    public Guid? ObjGuid { get; set; }
    public List<string> Properties { get; set; } = new List<string>();
}
