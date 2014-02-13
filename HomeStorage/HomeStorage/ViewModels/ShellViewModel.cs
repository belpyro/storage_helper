using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Caliburn.Micro;

namespace HomeStorage.ViewModels
{
    public class ShellViewModel : Screen
    {

        private ObservableCollection<Items> _storageItems;
        private List<Categories> _itemCategories;
        private Categories _itemCategory;
        private string _itemName;
        private int _selectedIndex;
        private readonly INavigationService _service;
        private List<Storages> _itemStorages;
        private Storages _itemStorage;

        #region Items

        public ShellViewModel(INavigationService service)
        {
            this.Activated += ShellViewModel_Activated;
            this.Deactivated += ShellViewModel_Deactivated;
            _service = service;
            _service.Navigated += ServiceNavigated;
        }

        private void ServiceNavigated(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                LoadData();
            }
        }

        void ShellViewModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            this.Activated -= ShellViewModel_Activated;
            this.Deactivated -= ShellViewModel_Deactivated;
        }

        void ShellViewModel_Activated(object sender, ActivationEventArgs e)
        {
            PrepareImages();
            LoadData();
        }

        public void LoadData()
        {
            //try load data
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                context.CreateIfNotExists();

                StorageItems = new ObservableCollection<Items>(context.Items.OrderBy(x => x.Name).ToList());
                ItemCategories = new List<Categories>(context.Categories.ToList());
                ItemStorages = new List<Storages>(context.Storages.ToList());

                DisplayName = string.Format("Вещи ({0})", StorageItems.Count);

                NotifyOfPropertyChange(null);
            }
        }

        private void PrepareImages()
        {
            var res = Application.GetResourceStream(new Uri("Assets/default.jpg", UriKind.Relative));
            if (res != null)
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (myIsolatedStorage.FileExists("default.jpg"))
                    {
                        return;
                    }

                    using (
                        IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream("default.jpg",
                            FileMode.Create,
                            myIsolatedStorage))
                    {
                        using (BinaryWriter writer = new BinaryWriter(fileStream))
                        {
                            long length = res.Stream.Length;
                            byte[] buffer = new byte[32];
                            int readCount = 0;
                            using (BinaryReader reader = new BinaryReader(res.Stream))
                            {
                                // read file in chunks in order to reduce memory consumption and increase performance
                                while (readCount < length)
                                {
                                    int actual = reader.Read(buffer, 0, buffer.Length);
                                    readCount += actual;
                                    writer.Write(buffer, 0, actual);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ItemSelected(Items sender)
        {
            _service.UriFor<ItemViewModel>().WithParam(x => x.Id, sender.Id).Navigate();
        }

        public ObservableCollection<Items> StorageItems
        {
            get { return _storageItems; }
            set
            {
                _storageItems = value;
                NotifyOfPropertyChange(() => StorageItems);
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                NotifyOfPropertyChange(() => SelectedIndex);
            }
        }

        public void AddItem()
        {
            _service.UriFor<ItemViewModel>().Navigate();
        }

        public void SearchItem(int index)
        {
            if (string.IsNullOrEmpty(ItemName) && ItemCategory == null && ItemStorage == null)
            {
                SelectedIndex = 1;
                LoadData();
                return;
            }

            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                if (!string.IsNullOrEmpty(ItemName))
                {
                    StorageItems = new ObservableCollection<Items>(context.Items.Where(x => x.Name.Contains(ItemName)).ToList());
                }

                if (ItemCategory != null)
                {
                    StorageItems = StorageItems != null
                        ? new ObservableCollection<Items>(StorageItems.Where(x => x.CategoryId == ItemCategory.Id))
                        : new ObservableCollection<Items>(
                            context.Items.Where(x => x.CategoryId == ItemCategory.Id).ToList());
                }

                if (ItemStorage != null)
                {
                   StorageItems = StorageItems != null
                        ? new ObservableCollection<Items>(StorageItems.Where(x => x.StorageId == ItemStorage.Id))
                        : new ObservableCollection<Items>(
                            context.Items.Where(x => x.StorageId == ItemStorage.Id).ToList()); 
                }

                ItemName = null;
                ItemCategory = null;
                ItemStorage = null;

                SelectedIndex = 0;
            }
        }

        public void RemoveItem(IEnumerable<object> items)
        {
            if (items == null || !items.Any()) return;

            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                context.Items.DeleteAllOnSubmit(context.Items.Where(x => items.Cast<Items>().Select(m => m.Id).Contains(x.Id)));
                context.SubmitChanges();
                LoadData();
            }
        }

        #endregion

        #region Search

        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                NotifyOfPropertyChange(() => ItemName);
            }
        }

        public List<Categories> ItemCategories
        {
            get { return _itemCategories; }
            set
            {
                _itemCategories = value;
                NotifyOfPropertyChange(() => ItemCategories);
            }
        }

        public Categories ItemCategory
        {
            get { return _itemCategory; }
            set
            {
                _itemCategory = value;
                NotifyOfPropertyChange(() => ItemCategory);
            }
        }


        public List<Storages> ItemStorages
        {
            get { return _itemStorages; }
            set
            {
                _itemStorages = value; 
                NotifyOfPropertyChange(() => ItemStorages);
            }
        }

        public Storages ItemStorage
        {
            get { return _itemStorage; }
            set
            {
                _itemStorage = value; 
                NotifyOfPropertyChange(() => ItemStorage);
            }
        }

        #endregion
    }
}
