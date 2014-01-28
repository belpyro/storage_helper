using System.Data.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using HomeStorage.Messages;

namespace HomeStorage.ViewModels
{
    public class StorageInfoViewModel : Screen, IHandle<PhotoSucessfullMessage>
    {
        private const string TEMPLATE_NAME = "storage{0}.jpg";

        private int _id;
        private string _name;
        private string _imageName;

        private BitmapImage _photo;

        private INavigationService _service;
        private IEventAggregator _aggregator;
        private Storages _storageItem;

        public StorageInfoViewModel(IEventAggregator aggregator, INavigationService service)
        {
            _aggregator = aggregator;
            _aggregator.Subscribe(this);
            _service = service;
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
                _storageItem.Name = value;

                SaveData();
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
                _storageItem = context.Storages.FirstOrDefault(x => x.Id == this.Id);
                if (_storageItem != null)
                {
                    Name = _storageItem.Name;
                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                    if (_storageItem.ImagePath != null && store.FileExists(_storageItem.ImagePath))
                    {
                        var imageFile = store.OpenFile(_storageItem.ImagePath, FileMode.Open, FileAccess.Read);
                        var ms = new MemoryStream();
                        imageFile.CopyTo(ms);
                        Photo = new BitmapImage();
                        Photo.SetSource(ms);
                        imageFile.Close();
                    }
                }
            }
        }

        public void SetPhoto()
        {
            _imageName = string.Format(TEMPLATE_NAME, Id);
            _service.UriFor<PhotoViewModel>().WithParam(x => x.FileName, _imageName).Navigate();
        }

        public void Handle(PhotoSucessfullMessage message)
        {
            _storageItem.ImagePath = message.ImageName; 
            SaveData();
        }

        private void SaveData()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                var item = context.Storages.FirstOrDefault(x => x.Id == _storageItem.Id);

                if (item != null)
                {
                    item.ImagePath = _storageItem.ImagePath;
                    item.Name = _storageItem.Name;
                    context.SubmitChanges(ConflictMode.FailOnFirstConflict);
                }
            }
        }
    }
}
