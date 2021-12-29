using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents the type converter for the items of the image list view.
/// </summary>
internal class ImageListViewItemTypeConverter : TypeConverter
{
    #region TypeConverter Overrides
    /// <summary>
    /// Returns whether this converter can convert the 
    /// object to the specified type, using the specified context.
    /// </summary>
    /// <param name="context">Format context.</param>
    /// <param name="destinationType">The type you want to convert to.</param>
    /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        if (destinationType == typeof(InstanceDescriptor))
            return true;

        return base.CanConvertTo(context, destinationType);
    }
    /// <summary>
    /// Converts the given value object to the specified type, 
    /// using the specified context and culture information.
    /// </summary>
    /// <param name="context">Format context.</param>
    /// <param name="culture">The culture info. If null is passed, the current culture is assumed.</param>
    /// <param name="value">The objct to convert.</param>
    /// <param name="destinationType">The type to convert to.</param>
    /// <returns>An object that represents the converted value.</returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value != null && value is ImageListViewItem)
        {
            ImageListViewItem item = (ImageListViewItem)value;

            if (destinationType == typeof(InstanceDescriptor))
            {
                ConstructorInfo consInfo = typeof(ImageListViewItem).GetConstructor(new Type[] {
                            typeof(string), typeof(string), typeof(object)
                        });
                return new InstanceDescriptor(consInfo, new object[] {
                        item.FileName, item.Text, item.Tag
                    });
            }
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
    #endregion
}

