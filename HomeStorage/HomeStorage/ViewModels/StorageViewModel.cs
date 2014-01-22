using Caliburn.Micro;

namespace HomeStorage.ViewModels
{
    public class StorageViewModel : PropertyChangedBase
    {
        private string _name;
        private string _imagePath;

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyOfPropertyChange(() => Name); }
        }

        public string ImagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; NotifyOfPropertyChange(() => ImagePath); }
        }
    }
}
