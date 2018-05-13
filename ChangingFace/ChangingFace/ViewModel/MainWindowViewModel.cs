using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace ChangingFace.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private VideoCapture _camera;
        private DispatcherTimer _timer;
        private CascadeClassifier _faceClassifier;

        private BitmapSource _userImage;

        public BitmapSource UserImage
        {
            get { return _userImage; }
            set
            {
                _userImage = value;
                OnPropertyChanged("UserImage");
            }
        }

        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            InitializeFaceDetection();
        }

        private void InitializeFaceDetection()
        {
            _camera = new VideoCapture();
            _faceClassifier = new CascadeClassifier(@"haarcascade_frontalface_alt_tree.xml");
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 30);
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var currentFrameMat = new Mat();
            _camera.Read(currentFrameMat);

            if (!currentFrameMat.IsEmpty)
            {
                var currentFrame = currentFrameMat.ToImage<Bgr, byte>();

                var grayFrame = currentFrameMat.ToImage<Gray, Byte>();

                var detectedFaces = _faceClassifier.DetectMultiScale(grayFrame);

                foreach (var face in detectedFaces)
                {
                    currentFrame.Draw(face, new Bgr(0, double.MaxValue, 0), 3);
                }
                UserImage = ToBitmapSource(currentFrame);
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private BitmapSource ToBitmapSource(IImage image)
        {
            using (Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                var result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                ptr,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(ptr);
                return result;
            }
        }
    }
}
