using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace HomeStorage.ViewModels
{
    public class MainPageViewModel : PropertyChangedBase
    {
        private ObservableCollection<Storages> _storageItems;

        private INavigationService _service;

        //= new ObservableCollection<StorageViewModel>()
        //{
        //    new StorageViewModel(){Name = "test"},
        //    new StorageViewModel(){Name = "test2"},
        //    new StorageViewModel(){Name = "test3"},
        //};

        private StorageContext _context;
        private ObservableCollection<Categories> _categoryItems;

        public MainPageViewModel(INavigationService service)
        {
            _service = service;
            LoadData();
        }

        private void LoadData()
        {
            using (_context = new StorageContext(StorageContext.ConnectionString))
            {
                _context.CreateIfNotExists();

                //_context.Storages.InsertOnSubmit(new Storages() { Name = "test 1" });
                //_context.Storages.InsertOnSubmit(new Storages() { Name = "test 2" });
                //_context.SubmitChanges();  

                //_context.Categories.InsertOnSubmit(new Categories() { Name = "test 1" });
                //_context.Categories.InsertOnSubmit(new Categories() { Name = "test 2" });
                //_context.SubmitChanges();

                StorageItems = new ObservableCollection<Storages>(_context.Storages);
                CategoryItems = new ObservableCollection<Categories>(_context.Categories);
            }
        }

        public ObservableCollection<Storages> StorageItems
        {
            get
            {
                return _storageItems;
            }
            set
            {
                _storageItems = value;
                NotifyOfPropertyChange(() => StorageItems);
            }
        }

        public ObservableCollection<Categories> CategoryItems
        {
            get { return _categoryItems; }
            set
            {
                _categoryItems = value;
                NotifyOfPropertyChange(() => CategoryItems);
            }
        }

        public void GoToStoragePage(Storages o)
        {
           _service.UriFor<StorageInfoViewModel>().WithParam(x => x.Id, o.Id).Navigate();
        }
    }
}