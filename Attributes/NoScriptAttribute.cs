namespace ImageHandle.Attributes
{
    internal class NoScriptAttribute : Attribute
    {
        private string msg = null;

        public NoScriptAttribute(string msg = "")
        {
        }
    }
}