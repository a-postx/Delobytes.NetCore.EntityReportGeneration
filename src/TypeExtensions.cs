namespace Delobytes.NetCore.EntityReportGeneration;

internal static class TypeExtensions
{
    public static string GetFriendlyName(this Type type)
    {
        string friendlyName = type.Name;

        if (type.IsGenericType)
        {
            int iBacktick = friendlyName.IndexOf('`');

            if (iBacktick > 0)
            {
                friendlyName = friendlyName.Remove(iBacktick);
            }

            friendlyName += "<";
            Type[] typeParameters = type.GetGenericArguments();

            for (int i = 0; i < typeParameters.Length; ++i)
            {
                string typeParamName = typeParameters[i].Name;
                friendlyName += i == 0 ? typeParamName : "," + typeParamName;
            }

            friendlyName += ">";
        }

        return friendlyName;
    }
}
