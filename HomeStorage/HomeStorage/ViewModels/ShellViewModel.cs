using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        private INavigationService _service;

        #region Items

        public ShellViewModel(INavigationService service)
        {
            this.Activated += ShellViewModel_Activated;
            this.Deactivated += ShellViewModel_Deactivated;
            _service = service;
        }

        void ShellViewModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            this.Activated -= ShellViewModel_Activated;
            this.Deactivated -= ShellViewModel_Deactivated;
        }

        void ShellViewModel_Activated(object sender, ActivationEventArgs e)
        {
            PrepareImages();

            //try load data
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                context.CreateIfNotExists();

                StorageItems = new ObservableCollection<Items>(context.Items);
                ItemCategories = new List<Categories>(context.Categories);
            }
        }

        private void PrepareImages()
        {
            var res = Application.GetResourceStream(new Uri("Assets/default.jpg", UriKind.Relative));
            if (res != null)
            {
                    using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
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


        public void AddItem(int index)
        {

        }

        public void SearchItem(int index)
        {
            if (string.IsNullOrEmpty(ItemName) && ItemCategory == null)
            {
                SelectedIndex = 1;
                return;
            }

            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                //TODO change algoritm
                StorageItems = ItemCategory != null ? new ObservableCollection<Items>(context.Items.Where(x => x.Categories.Id == ItemCategory.Id)) : new ObservableCollection<Items>(context.Items.Where(x => x.Name.Contains(ItemName)));

                ItemName = null;
                ItemCategory = null;

                SelectedIndex = 0;
            }
        }

        public void RemoveItem(int index)
        {

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

        #endregion
    }
}
