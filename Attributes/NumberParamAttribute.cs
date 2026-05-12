using ImageHandle.Models;

namespace ImageHandle.Attributes
{
    internal class NumberParamAttribute : Attribute
    {
        private string diaplayText = "";
        private NumberParamTypeEnum paramtype = NumberParamTypeEnum.Double;
        private string tipText = "";
        private string defaultValue = "";

        public NumberParamAttribute(string diaplayText, NumberParamTypeEnum paramtype, string tipText, string defaultValue)
        {
            this.diaplayText = diaplayText;
            this.paramtype = paramtype;
            this.tipText = tipText;
            this.defaultValue = defaultValue;
        }

        public string DiaplayText { get => diaplayText; set => diaplayText = value; }
        public NumberParamTypeEnum Paramtype { get => paramtype; set => paramtype = value; }
        public string TipText { get => tipText; set => tipText = value; }
        public string DefaultValue { get => defaultValue; set => defaultValue = value; }
    }
}