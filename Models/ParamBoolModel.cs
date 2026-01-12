namespace ImageHandle.Models
{
    public class ParamBoolModel
    {
        public ParamBoolModel(string paramName, bool paramValue, string paramDisplayName = "", string remark = "")
        {
            ParamName = paramName;
            ParamValue = paramValue;
            ParamDisplayName = paramDisplayName == "" ? paramName : paramDisplayName;
            Remark = remark == "" ? paramName : remark;
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public bool ParamValue { get; set; }

        /// <summary>
        /// 参数显示名称
        /// </summary>
        public string ParamDisplayName { get; set; }

        public string Remark { get; set; }
    }
}