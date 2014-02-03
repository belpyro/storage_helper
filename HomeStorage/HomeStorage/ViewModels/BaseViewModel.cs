using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace HomeStorage.ViewModels
{
    public class BaseViewModel: Conductor<object>
    {

        private IEventAggregator _aggregator;
        private INavigationService _service;

        public BaseViewModel(IEventAggregator aggregator, INavigationService service)
        {
            _aggregator = aggregator;
            _service = service;
            ShowMainPage();
        }

        public void ShowMainPage()
        {
            ActivateItem(new ItemViewModel(_aggregator, _service));
        }

        public void ShowSearchPage()
        {
            
        }

    }
}
