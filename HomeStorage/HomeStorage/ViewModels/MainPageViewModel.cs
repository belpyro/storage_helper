using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;

namespace HomeStorage.ViewModels
{
    public class MainPageViewModel : PropertyChangedBase
    {
        private ObservableCollection<Storages> _storageItems;

        private readonly INavigationService _service;

        private StorageContext _context;
        private ObservableCollection<Categories> _categoryItems;

        public MainPageViewModel(INavigationService service)
        {
            _service = service;
            _service.Navigated += _service_Navigated;
            LoadData();
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).CommandParameter;

            if (item is Storages)
            {
                using (var context =new StorageContext(StorageContext.ConnectionString))
                {

                    context.Storages.DeleteOnSubmit(context.Storages.First(x => x.Id == ((Storages) item).Id));
                    context.SubmitChanges();
                } 
            }

            LoadData();
        }

        private void EditItem(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).CommandParameter;

            if (item is Storages)
            {
               GoToStoragePage((Storages) item); 
            }
        }

        void _service_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                LoadData();
            }
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

        public void GetMenu(FrameworkElement element)
        {
            var menu = ContextMenuService.GetContextMenu(element);

            if (menu == null)
            {
                menu = new ContextMenu();
            }
            else
            {
                menu.Items.Clear();
            }

            var editLink = new MenuItem { Header = "редактировать" };
            editLink.Click += EditItem;

            var deleteLink = new MenuItem { Header = "удалить" };
            deleteLink.Click += DeleteItem;

            menu.Items.Add(editLink);
            menu.Items.Add(deleteLink);

            editLink.CommandParameter = element.DataContext;
            deleteLink.CommandParameter = element.DataContext;

            ContextMenuService.SetContextMenu(element, menu);

            menu.IsOpen = true;
        }
    }
}