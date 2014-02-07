using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using HomeStorage.Messages;
using Microsoft.Devices;
using Microsoft.Phone;

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

            if (_camera == null) return;
            _camera.Initialized -= CameraInitialized;
            _camera.CaptureImageAvailable -= CameraCompleted;

            _camera.Dispose();
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
            CaptureImage();
        }

        private void CaptureImage()
        {
            if (_camera != null)
            {
                _camera.CaptureImage();
            }
        }

        private void CameraCompleted(object sender, ContentReadyEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (storage.FileExists(FileName))
                        {
                            storage.DeleteFile(FileName);
                        }


                        using (var file = storage.CreateFile(FileName))
                        {
                            var decodedImage = PictureDecoder.DecodeJpeg(e.ImageStream, 256, 256);
                            decodedImage.SaveJpeg(file, 256, 256, 0, 100);
                            e.ImageStream.Dispose();
                        }
                    }

                    _aggregator.Publish(new PhotoSucessfullMessage(FileName));
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
