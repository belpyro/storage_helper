using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.ApplicationModel.Store;
using Caliburn.Micro;
using Microsoft.Devices;

namespace HomeStorage.ViewModels
{
    public class PhotoViewModel : Screen
    {
        private VideoBrush _cameraBrush;
        private INavigationService _service;
        private PhotoCamera _camera;

        public PhotoViewModel(INavigationService service)
        {
            _service = service;
            _cameraBrush = new VideoBrush();
            this.Deactivated += PhotoViewModel_Deactivated;
            SetPhoto();
        }


        void PhotoViewModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            _camera.Initialized -= _camera_Initialized;
            _camera.CaptureImageAvailable -= _camera_CaptureImageAvailable;
            CameraButtons.ShutterKeyPressed -= ButtonPressed;
            _camera.Dispose();
        }

        private void SetPhoto()
        {
            if (Camera.IsCameraTypeSupported(CameraType.Primary))
            {
                _camera = new PhotoCamera(CameraType.Primary);
                _camera.Initialized += _camera_Initialized;
                _camera.CaptureCompleted += _camera_CaptureCompleted;
                _camera.CaptureImageAvailable += _camera_CaptureImageAvailable;

                CameraBrush.SetSource(_camera);
                NotifyOfPropertyChange(() => CameraBrush);
            }
        }

        void _camera_CaptureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            try
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();

                using (var stream = store.CreateFile(FileName))
                {
                    e.ImageStream.CopyTo(stream);
                }

            }
            finally
            {
                e.ImageStream.Close(); 
                Deployment.Current.Dispatcher.BeginInvoke(() => _service.GoBack());
            }
        }

        private void ButtonPressed(object sender, EventArgs e)
        {
            _camera.CaptureImage();
        }

        void _camera_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            //if (e.Succeeded)
            //{

            //}
        }

        void _camera_Initialized(object sender, CameraOperationCompletedEventArgs e)
        {
            CameraButtons.ShutterKeyPressed += ButtonPressed;
        }


        public VideoBrush CameraBrush
        {
            get { return _cameraBrush; }
            set
            {
                _cameraBrush = value;
                NotifyOfPropertyChange(() => CameraBrush);
            }
        }

        public string FileName { get; set; }
    }
}
