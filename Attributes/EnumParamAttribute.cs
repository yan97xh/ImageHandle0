namespace ImageHandle.Attributes
{
    public class EnumParamAttribute : Attribute
    {
        private string diaplayText = "";
        private string tipText = "";
        private string defaultValue = "";

        /// <summary>
        /// 枚举类型参数
        /// </summary>
        /// <param name="diaplayText"></param>
        /// <param name="tipText"></param>
        /// <param name="defaultValue"></param>
        public EnumParamAttribute(string diaplayText, string tipText = "", string defaultValue = "")
        {
            this.diaplayText = diaplayText;
            this.tipText = tipText;
            this.defaultValue = defaultValue;
        }

        public string DiaplayText { get => diaplayText; set => diaplayText = value; }
        public string TipText { get => tipText; set => tipText = value; }
        public string DefaultValue { get => defaultValue; set => defaultValue = value; }
    }
}