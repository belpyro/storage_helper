using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Caliburn.Micro;
using HomeStorage.Messages;
using Microsoft.Phone;

namespace HomeStorage.ViewModels
{
    public class ItemViewModel : Screen, IHandle<PhotoSucessfullMessage>
    {
        private string _name;
        private string _desription;
        private string _imagePath;
        private readonly string template = "pictire{0}.jpg";
        private Categories _category;
        private Storages _storage;
        private WriteableBitmap _image;
        private INavigationService _service;
        private IEventAggregator _aggregator;
        private IEnumerable<Categories> _allCategories;
        private string _categoryName;
        private IEnumerable<Storages> _allStorages;
        private string _storageName;

        public ItemViewModel(IEventAggregator aggregator, INavigationService service)
        {
            this.Activated += ItemViewModel_Activated;
            this.Deactivated += ItemViewModel_Deactivated;

            _service = service;
            _service.Navigating += OnNavigating;
            _aggregator = aggregator;
        }

        #region Item info

        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Description
        {
            get { return _desription; }
            set
            {
                _desription = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public IEnumerable<Categories> AllCategories
        {
            get { return _allCategories; }
            set
            {
                _allCategories = value;
                NotifyOfPropertyChange(() => AllCategories);
            }
        }

        public Categories Category
        {
            get
            {
                return _category;
            }
            set
            {
                if (value != _category)
                {
                    _category = value;
                    NotifyOfPropertyChange(() => Category);
                }
            }
        }

        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                _categoryName = value;
                NotifyOfPropertyChange(() => CategoryName);
            }
        }

        public IEnumerable<Storages> AllStorages
        {
            get { return _allStorages; }
            set
            {
                _allStorages = value;
                NotifyOfPropertyChange(() => AllStorages);
            }
        }

        public Storages Storage
        {
            get { return _storage; }
            set
            {
                if (_storage != value)
                {
                    _storage = value;
                    NotifyOfPropertyChange(() => Storage);
                }
            }
        }

        public string StorageName
        {
            get { return _storageName; }
            set
            {
                _storageName = value;
                NotifyOfPropertyChange(() => StorageName);
            }
        }

        public WriteableBitmap ImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(_imagePath))
                {
                    _image = PictureDecoder.DecodeJpeg(
                        Application.GetResourceStream(new Uri("Assets/default.jpg", UriKind.Relative)).Stream, 256, 256);
                }
                else
                {
                    using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var file = myIsolatedStorage.OpenFile(_imagePath, FileMode.Open))
                        {
                            var memStream = new MemoryStream((int)file.Length);
                            file.CopyTo(memStream, 2000);

                            memStream.Seek(0, SeekOrigin.Begin);
                            _image = PictureDecoder.DecodeJpeg(memStream, 256, 256);
                        }
                    }
                }

                return _image;
            }
        }

        #endregion

        #region Methods

        void ItemViewModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            this.Activated -= ItemViewModel_Activated;
            this.Deactivated -= ItemViewModel_Deactivated;

            _service.Navigating -= OnNavigating;
        }

        void ItemViewModel_Activated(object sender, ActivationEventArgs e)
        {
            _aggregator.Subscribe(this);

            if (Id <= 0)
            {
                SaveNewItem();
                NotifyOfPropertyChange(null);
            }
            else
            {
                LoadData();
            }
        }

        void LoadData()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                if (Id > 0)
                {
                    var item = context.Items.FirstOrDefault(x => x.Id == Id);
                    
                    if (item != null)
                    {
                        Name = item.Name;
                        Description = item.Description;
                        Category = item.Categories;
                        Storage = item.Storages;
                        _imagePath = item.ImagePath;
                    }
                }

                AllCategories = context.Categories.ToList();
                AllStorages = context.Storages.ToList(); 
            }

            NotifyOfPropertyChange(null);
        }

        void SaveData()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                Categories category = null;
                Storages storage = null;

                if (Category == null && !string.IsNullOrEmpty(CategoryName))
                {
                    category = new Categories() { Name = CategoryName };
                    context.Categories.InsertOnSubmit(category);
                }

                if (Storage == null && !string.IsNullOrEmpty(StorageName))
                {
                    storage = new Storages() { Name = StorageName };
                    context.Storages.InsertOnSubmit(storage);
                }

                if (category != null || storage != null)
                {
                    context.SubmitChanges();

                    if (category != null)
                    {
                        Category = category;
                    }
                    if (storage != null)
                    {
                        Storage = storage;
                    }
                }

                var item = context.Items.FirstOrDefault(x => x.Id == Id);
                if (item != null)
                {
                    item.Name = Name;
                    item.ImagePath = _imagePath;
                    item.Description = Description;
                    item.CategoryId = Category.Id;
                    item.StorageId = Storage.Id;
                }

                context.SubmitChanges();
            }
        }

        void SaveImageData()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                var item = context.Items.First(x => x.Id == Id);
                item.ImagePath = _imagePath;
                context.SubmitChanges(); 
            }

            NotifyOfPropertyChange(() => ImagePath);
        }

        void SaveNewItem()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                var item = new Items { Name = "-пусто-", StorageId = 1, CategoryId = 1, ImagePath = "default.jpg" };
                context.Items.InsertOnSubmit(item);
                context.SubmitChanges();
                Id = item.Id;
                Name = item.Name;
                Description = item.Description;
                _imagePath = item.ImagePath;
                Category = item.Categories;
                Storage = item.Storages;
            }
        }

        public void SetPhoto()
        {
            _service.UriFor<PhotoViewModel>().WithParam(x => x.FileName, string.Format(template, Id)).WithParam(x => x.Id, Id).Navigate();
        }

        public void Handle(PhotoSucessfullMessage message)
        {
            _imagePath = message.ImageName;
            SaveImageData();
            LoadData();
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                SaveData();
            }
        }

        #endregion
    }
}
