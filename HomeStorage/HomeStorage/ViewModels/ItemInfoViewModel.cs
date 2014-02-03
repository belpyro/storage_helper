using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace HomeStorage.ViewModels
{
    public class ItemInfoViewModel : Screen
    {
        private string _name;
        private string _description;
        private string _category;
        private Items _item = new Items();

        public ItemInfoViewModel()
        {
            this.Activated += (sender, args) => LoadData();
        }

        public int StorageId { get; set; }

        public int Id { get; set; }

        private void LoadData()
        {
            if (Id <= 0)
            {
                _item = new Items();
                return;
            }

            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                _item = context.Items.FirstOrDefault(x => x.Id == Id);
            }
        }

        public string Name
        {
            get { return _item.Name; }
            set
            {
                _item.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Description
        {
            get { return _item.Description; }
            set
            {
                _item.Description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
                NotifyOfPropertyChange(() => Category);
            }
        }
    }
}
