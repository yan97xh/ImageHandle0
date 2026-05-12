using ImageHandle.Scripts;

namespace ImageHandle.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ScriptParamAttribute : Attribute
    {
        private string name = "";
        private ParamType type;
        private string value = "";
        private string paramDisplayName = "";
        private string remark = "";

        /// <summary>
        ///
        /// </summary>
        /// <param name="name">参数名称，paramDisplayName = “” 时 就显示该参数</param>
        /// <param name="type">参数类型 </param>
        /// <param name="value">参数默认值</param>
        /// <param name="paramDisplayName">参数显示名称</param>
        /// <param name="remark">备注</param>
        public ScriptParamAttribute(string name, ParamType type, string value = "", string paramDisplayName = "", string remark = "")
        {
            this.name = name;
            this.type = type;
            this.value = value;
            this.paramDisplayName = paramDisplayName;
            this.remark = remark;
        }

        public string Name { get => name; set => name = value; }
        public ParamType Type { get => type; set => type = value; }
        public string Value { get => value; set => this.value = value; }
        public string ParamDisplayName { get => paramDisplayName; set => paramDisplayName = value; }
        public string Remark { get => remark; set => remark = value; }
    }
}