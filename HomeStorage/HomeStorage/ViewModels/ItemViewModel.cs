using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using HomeStorage.Messages;
using Microsoft.Phone;

namespace HomeStorage.ViewModels
{
    public class ItemViewModel : Screen, IHandle<PhotoSucessfullMessage>
    {
        private Items _item = new Items();
        private string _name;
        private string _desription;
        private readonly string template = "pictire{0}.jpg";
        private Categories _category;
        private string _storage;
        private WriteableBitmap _image;
        private INavigationService _service;
        private IEnumerable<Categories> _allCategories;
        private string _categoryName;
        private IEnumerable<Storages> _allStorages;
        private string _storageName;

        public ItemViewModel(IEventAggregator aggregator, INavigationService service)
        {
            this.Activated += ItemViewModel_Activated;
            this.Deactivated += ItemViewModel_Deactivated;

            _service = service;
            aggregator.Subscribe(this);
        }

        #region Item info

        public int Id { get; set; }

        public string Name
        {
            get { return _item.Name; }
            set
            {
                _item.Name = value;
                SaveData();
            }
        }

        public string Description
        {
            get { return _item.Description; }
            set
            {
                _item.Description = value;
                SaveData();
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
                return _item.Categories;
            }
            set
            {
                if (value != _item.Categories)
                {
                    _item.Categories = value;
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
            get { return _item.Storages; }
            set
            {
                if (_item.Storages != value)
                {
                    _item.Storages = value;
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
                if (string.IsNullOrEmpty(_item.ImagePath) && _image == null)
                {
                    _image = PictureDecoder.DecodeJpeg(
                        Application.GetResourceStream(new Uri("Assets/default.jpg", UriKind.Relative)).Stream, 256, 256);
                }
                else
                {
                    using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var file = myIsolatedStorage.OpenFile(_item.ImagePath, FileMode.Open))
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

        void ItemViewModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            _item = null;

            this.Activated -= ItemViewModel_Activated;
            this.Deactivated -= ItemViewModel_Deactivated;
        }

        void ItemViewModel_Activated(object sender, ActivationEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                if (Id > 0)
                {
                    _item = context.Items.FirstOrDefault(x => x.Id == Id);
                }

                AllCategories = context.Categories.ToList();
                AllStorages = context.Storages.ToList();

                NotifyOfPropertyChange(null);
            }
        }

        void SaveData()
        {
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                if (_item.CategoryId <= 0 && !string.IsNullOrEmpty(CategoryName))
                {
                    var category = new Categories() {Name = CategoryName};
                    context.Categories.InsertOnSubmit(category);
                    context.SubmitChanges();
                    _item.CategoryId = category.Id;
                }
                
                if (_item.StorageId <= 0 && !string.IsNullOrEmpty(StorageName))
                {
                    var storage = new Storages() {Name = StorageName};
                    context.Storages.InsertOnSubmit(storage);
                    context.SubmitChanges();
                    _item.StorageId = storage.Id;
                }

                var item = context.Items.First(x => x.Id == Id);
                item.Name = _item.Name;
                item.ImagePath = _item.ImagePath;
                item.Description = _item.Description;
                item.CategoryId = _item.CategoryId;
                item.StorageId = _item.StorageId;

                context.SubmitChanges();
                _item = item;
                NotifyOfPropertyChange(null);
            }
        }

        public void SetPhoto()
        {
            _service.UriFor<PhotoViewModel>().WithParam(x => x.FileName, string.Format(template, Id)).WithParam(x => x.Id, Id).Navigate();
        }

        public void Handle(PhotoSucessfullMessage message)
        {
            LoadData();
            _item.ImagePath = message.ImageName;
            SaveData();
        }

        public void CategoryUpdate()
        {
            SaveData();
        }

        public void StorageUpdate()
        {
            SaveData();
        }
    }
}
