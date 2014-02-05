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
using Microsoft.Phone;

namespace HomeStorage.ViewModels
{
    public class ItemViewModel : Screen
    {
        private Items _item = new Items();
        private string _name;
        private string _desription;
        private string _category;
        private string _storage;
        private WriteableBitmap _image;

        public ItemViewModel()
        {
            this.Activated += ItemViewModel_Activated;
            this.Deactivated += ItemViewModel_Deactivated;
        }

        #region Item info

        public int Id { get; set; }

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
            get
            {
                return _item.Categories == null ? null : _item.Categories.Name;
            }
            set
            {
                _category = value;
                NotifyOfPropertyChange(() => Category);
            }
        }

        public string Storage
        {
            get { return _item.Storages == null ? null : _item.Storages.Name; }
            set
            {
                _storage = value;
                NotifyOfPropertyChange(() => Storage);
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
                            _image.SetSource(memStream);
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
            using (var context = new StorageContext(StorageContext.ConnectionString))
            {
                if (Id > 0)
                {
                    _item = context.Items.FirstOrDefault(x => x.Id == Id);
                }

                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => Description);
                NotifyOfPropertyChange(() => Category);
                NotifyOfPropertyChange(() => Storage);
            }
        }


    }
}
