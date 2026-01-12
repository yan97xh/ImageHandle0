using ImageHandle.ViewModels;

namespace ImageHandle.Models
{
    public class CoordinateModel : ViewModelBase
    {
        private double _x;
        private double _y;
        private bool _isSelected;

        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}