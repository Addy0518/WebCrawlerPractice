using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Console1.ExtensionMethods;

public static class EnumExtension
{
    /// <summary>
    /// 取得[Description]屬性
    /// </summary>
    /// <typeparam name="T">Enum</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetEnumDescription<T>(this T value)
        where T : Enum
    {
        // 取得value欄位屬性
        FieldInfo filedInfo = value.GetType().GetField(value.ToString());
        // GetCustomAttributes可以取得需要什麼樣的屬性標籤
        var descriptionAttributes = (DescriptionAttribute[])filedInfo.GetCustomAttributes(typeof(DescriptionAttribute));
        // 如果大於0的話返回Description
        if (descriptionAttributes.Length > 0)
        {
            return descriptionAttributes[0].Description.ToString();
        }
        // 如果小於0的話返回Name
        else
        {
            return value.ToString();
        }
    }

    public class EnumValueValidationAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public EnumValueValidationAttribute(Type enumType)
        {
            _enumType = enumType;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (!Enum.IsDefined(_enumType, value))
            {
                return new ValidationResult($"{value} 不是有效的 {_enumType.Name} 值");
            }

            return ValidationResult.Success;
        }
    }

    public class ExcelEnumAttribute : Attribute
    {
        public Type EnumType { get; }

        public ExcelEnumAttribute(Type enumType) => EnumType = enumType;
    }
}
