using System;
using System.Collections.Generic;
using Caliburn.Micro;
using HomeStorage.ViewModels;

namespace HomeStorage
{
    public class Bootstrapper: PhoneBootstrapper
    {
        private PhoneContainer _container;

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override void Configure()
        {
            _container = new PhoneContainer();
            _container.RegisterPhoneServices(RootFrame);
            _container.PerRequest<MainPageViewModel>();
            _container.PerRequest<StorageViewModel>();
            _container.PerRequest<ItemViewModel>();
            _container.PerRequest<StorageInfoViewModel>();
            _container.PerRequest<PhotoViewModel>();
        }
    }
}
