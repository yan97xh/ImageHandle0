namespace ImageHandle.Models
{
    public class ParamEnumModel
    {
        public ParamEnumModel(string paramName, Type type, string paramValue = "", string paramDisplayName = "", string remark = "")
        {
            ParamName = paramName;
            ParamDisplayName = paramDisplayName == "" ? paramName : paramDisplayName;
            Remark = remark == "" ? ParamDisplayName : remark;
            if (!type.IsEnum)
            {
                throw new Exception("类型必须为枚举类型");
            }
            ItemsArray = Enum.GetNames(type);
            ParamValue = paramValue == "" ? ItemsArray[0] : paramValue;
            _type = type;
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ParamValue { get; set; }

        public string[] ItemsArray { get; set; }

        /// <summary>
        /// 参数显示名称
        /// </summary>
        public string ParamDisplayName { get; set; }

        public string Remark { get; set; }

        private Type _type;

        public object GetValue()
        {
            if (Array.IndexOf(ItemsArray, ParamValue) < 0)
            {
                return Activator.CreateInstance(_type);
            }
            return Enum.Parse(_type, ParamValue, true);
        }
    }
}