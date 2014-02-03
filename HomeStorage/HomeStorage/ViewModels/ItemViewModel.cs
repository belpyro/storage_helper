using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace HomeStorage.ViewModels
{
    public class ItemViewModel : Screen
    {
        private IEventAggregator _aggregator;
        private INavigationService _service;

        public ItemViewModel(IEventAggregator aggregator, INavigationService service)
        {
            _aggregator = aggregator;
            _service = service;
            this.Activated += (sender, args) => LoadData();
        }

        public int StorageId
        {
            get { return _storageId; }
            set
            {
                _storageId = value;
            }
        }

        private void LoadData()
        {
            if (StorageId <= 0)
            {
                return;
            }

            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                StorageItems = new ObservableCollection<Items>(context.Items.Where(x => x.StorageId == StorageId));
            }
        }

        private ObservableCollection<Items> _storageItems;
        private int _storageId;

        public ObservableCollection<Items> StorageItems
        {
            get { return _storageItems; }
            set
            {
                _storageItems = value;
                NotifyOfPropertyChange(() => StorageItems);
            }
        }

        public void AddItem()
        {
            _service.UriFor<ItemInfoViewModel>().WithParam(x => x.StorageId, StorageId).Navigate();
        }
    }
}
