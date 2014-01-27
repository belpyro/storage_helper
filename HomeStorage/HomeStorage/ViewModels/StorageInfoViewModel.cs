using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Devices;

namespace HomeStorage.ViewModels
{
    public class StorageInfoViewModel : Screen
    {
        private int _id;
        private string _name;
        private BitmapImage _photo;

        private PhotoCamera _camera;

        private INavigationService _service;

        public StorageInfoViewModel(INavigationService service)
        {
            _service = service;
            _service.Navigated += _service_Navigated;
        }

        void _service_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                if (value > 0)
                {
                    LoadData();
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public BitmapImage Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                NotifyOfPropertyChange(() => Photo);
            }
        }

        private void LoadData()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                var storageItem = context.Storages.FirstOrDefault(x => x.Id == this.Id);
                if (storageItem != null)
                {
                    Name = storageItem.Name;
                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                    if (storageItem.ImagePath != null && store.FileExists(storageItem.ImagePath))
                    {
                        var imageFile = store.OpenFile(storageItem.ImagePath, FileMode.Open, FileAccess.Read);
                        Photo = new BitmapImage();
                        Photo.SetSource(imageFile);
                    }
                }
            }
        }

        public void SetPhoto()
        {
            _service.UriFor<PhotoViewModel>().WithParam(x => x.FileName, string.Format("storage{0}.jpg", Id)).Navigate();
        }
    }
}
