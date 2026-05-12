using ImageHandle.Models;

namespace ImageHandle.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NavigationPageAttribute : Attribute
    {
        private PageEnum _pageEnum;

        public NavigationPageAttribute(PageEnum pageEnum)
        {
            _pageEnum = pageEnum;
        }

        public PageEnum PageEnum { get => _pageEnum; set => _pageEnum = value; }
    }
}