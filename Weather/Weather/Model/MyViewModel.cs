using System.ComponentModel;

namespace Weather.Model
{
    public class MyViewModel : INotifyPropertyChanged
    {
        private bool _isLocationOn;
        public bool IsLocationOn
        {
            get { return _isLocationOn; }
            set
            {
                if (_isLocationOn != value)
                {
                    _isLocationOn = value;
                    OnPropertyChanged(nameof(IsLocationOn));
                }
            }
        }

        private string _imgbg;
        public string ImgBg
        {
            get { return _imgbg; }
            set
            {
                if (_imgbg != value)
                {
                    _imgbg = value;
                    OnPropertyChanged(nameof(ImgBg));
                }
            }
        }

        public MyViewModel(string img, bool isLocationOn)
        {
            ImgBg = img;
            IsLocationOn = isLocationOn;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}