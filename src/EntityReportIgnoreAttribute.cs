namespace Delobytes.NetCore.EntityReportGeneration;

/// <summary>
/// Атрибут показывает, что свойство должно быть проигнорировано.
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Property)]
public class EntityReportIgnoreAttribute : Attribute
{

}
