namespace ImageHandle.Scripts
{
    public enum ParamType
    {
        String,
        Interger,
        Double,
        Boolean,
        Enum
    }

    public class ScriptParamModel
    {
        public string Name { get; set; }
        public ParamType Type { get; set; }
        public string Value { get; set; }
        public string ParamDisplayName { get; set; }
        public string Remark { get; set; }

        public string[] ParamArrays { get; private set; }
        private Type _type;

        public ScriptParamModel(string name, ParamType type, string value = "", Type? enumType = null, string paramDisplayName = "", string remark = "")
        {
            Name = name;
            Type = type;
            Value = value;
            ParamDisplayName = paramDisplayName == "" ? name : paramDisplayName;
            Remark = remark == "" ? ParamDisplayName : remark;
            if (type == ParamType.Enum)
            {
                if (enumType == null || !enumType.IsEnum)
                {
                    throw new ArgumentException("枚举类型参数必须提供枚举类型");
                }
                ParamArrays = Enum.GetNames(enumType);
                _type = enumType;
                if (Array.IndexOf(ParamArrays, value) < 0)
                {
                    Value = ParamArrays[0];
                }
            }
        }

        public bool CheckValueValid()
        {
            return Type switch
            {
                ParamType.String => true,
                ParamType.Interger => int.TryParse(Value, out _),
                ParamType.Double => double.TryParse(Value, out _),
                ParamType.Boolean => bool.TryParse(Value, out _),
                ParamType.Enum => Enum.IsDefined(_type, Value),
                _ => false,
            };
        }

        public object GetValue()
        {
            return Type switch
            {
                ParamType.String => Value,
                ParamType.Interger => int.TryParse(Value, out var intValue) ? intValue : 0,
                ParamType.Double => double.TryParse(Value, out var doubleValue) ? doubleValue : 0.0,
                ParamType.Boolean => bool.TryParse(Value, out var boolValue) ? boolValue : false,
                ParamType.Enum => Enum.Parse(_type, Value),
                _ => throw new NotSupportedException($"不支持的参数类型: {Type}"),
            };
        }
    }
}