using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace JN.Data.Common
{
   public static class  EnumExtension
    {
        /// 扩展方法，获得枚举的Description
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <param name="nameInstend">当枚举没有定义DescriptionAttribute,是否用枚举名代替，默认使用</param>
        /// <returns>枚举的Description</returns>
        public static string GetDescription(this System.Enum value, bool nameInstend = true)
        {
            Type type = value.GetType();
            string name = System.Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }
            FieldInfo field = type.GetField(name);
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute == null && nameInstend == true)
            {
                return name;
            }
            return attribute == null ? null : attribute.Description;
        }

        public static List<SelectOption> GetOptions(this System.Enum value, bool isAddAll = false)
        {
            Type type = value.GetType();
            List<SelectOption> result = new List<SelectOption>();
            if (isAddAll)
            {
                var all = new SelectOption() { label = "全部", value = "" };
                result.Add(all);
            }
            foreach (var suit in System.Enum.GetValues(type))
            {
                SelectOption o = new SelectOption();
                o.label = GetDescription((System.Enum)suit);
                o.value = ((System.Enum)suit).GetHashCode().ToString();
                result.Add(o);
            }
            return result;
        }

        /// <summary>
        /// 验证 枚举值 是否存在
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ExitEnum(this System.Enum value)
        {
            Type type = value.GetType();
            string name = System.Enum.GetName(type, value);
            if (name == null)
            {
                return false;
            }
            FieldInfo field = type.GetField(name);
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute == null)
            {
                return false;
            }
            return true;
        }
    }
}
