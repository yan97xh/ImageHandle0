namespace ImageHandle.Models
{
    public class ParamValueModel
    {
        public ParamValueModel(string paramName, string paramValue, string paramDisplayName = "", string remark = "")
        {
            ParamName = paramName;
            ParamValue = paramValue;
            ParamDisplayName = paramDisplayName == "" ? paramName : paramDisplayName;
            Remark = remark == "" ? ParamDisplayName : remark;
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ParamValue { get; set; }

        /// <summary>
        /// 参数显示名称
        /// </summary>
        public string ParamDisplayName { get; set; }

        public string Remark { get; set; }
    }
}