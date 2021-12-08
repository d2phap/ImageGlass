
namespace ImageGlass.Base;

public partial class Helpers
{
    /// <summary>
    /// Convert the given object to Enum type
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    /// <param name="value">Value</param>
    /// <returns></returns>
    public static T ParseEnum<T>(object value)
    {
        return (T)Enum.Parse(typeof(T), value.ToString(), true);
    }


    /// <summary>
    /// Convert the given value to specific type
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="value">Value</param>
    /// <returns></returns>
    public static T ConvertType<T>(object value)
    {
        var type = typeof(T);

        if (type.IsEnum)
        {
            return ParseEnum<T>(value);
        }
        else
        {
            return (T)Convert.ChangeType(value, type);
        }
    }
}
