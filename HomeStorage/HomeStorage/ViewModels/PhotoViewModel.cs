using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using HomeStorage.Messages;
using Microsoft.Devices;

namespace HomeStorage.ViewModels
{
    public class PhotoViewModel : Screen
    {
        private IEventAggregator _aggregator;
        private INavigationService _service;
        private VideoBrush _cameraBackground;
        private PhotoCamera _camera;

        public PhotoViewModel(IEventAggregator aggregator, INavigationService service)
        {
            _aggregator = aggregator;
            _service = service;
            CameraBackground = new VideoBrush();

            this.Activated += PhotoViewModel_Activated;
            this.Deactivated += PhotoViewModel_Deactivated;
        }

        void PhotoViewModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            this.Activated -= PhotoViewModel_Activated;
            this.Deactivated -= PhotoViewModel_Deactivated;
            CameraButtons.ShutterKeyPressed -= CameraButtons_ShutterKeyPressed;

            if (_camera != null)
            {
                _camera.Initialized -= CameraInitialized;
                _camera.CaptureImageAvailable -= CameraCompleted;

                _camera.Dispose();
            }
        }

        void PhotoViewModel_Activated(object sender, ActivationEventArgs e)
        {
            _camera = new PhotoCamera(CameraType.Primary);
            CameraBackground.SetSource(_camera);

            _camera.Initialized += CameraInitialized;
        }

        private void CameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            _camera.CaptureImageAvailable += CameraCompleted;
            CameraButtons.ShutterKeyPressed += CameraButtons_ShutterKeyPressed;
        }

        void CameraButtons_ShutterKeyPressed(object sender, EventArgs e)
        {
            if (_camera != null)
            {
                _camera.CaptureImage();
            }
        }

        private void CameraCompleted(object sender, ContentReadyEventArgs e)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(FileName))
                {
                    storage.DeleteFile(FileName);
                }

                using (var file = storage.CreateFile(FileName))
                {
                    e.ImageStream.CopyTo(file);
                    e.ImageStream.Dispose();
                }
            }

            _aggregator.Publish(new PhotoSucessfullMessage(FileName));
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _service.UriFor<ItemViewModel>().WithParam(x => x.Id, Id).Navigate();
            });
        }

        public VideoBrush CameraBackground
        {
            get { return _cameraBackground; }
            set
            {
                _cameraBackground = value;
                NotifyOfPropertyChange(() => CameraBackground);
            }
        }

        public string FileName { get; set; }

        public int Id { get; set; }
    }
}
