using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ChangingFace.Model;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.Util;

namespace ChangingFace.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private VideoCapture _camera;
        private DispatcherTimer _timer;
        private CascadeClassifier _faceClassifier;
        private byte[] detectedFaceBytes;
        private Image<Gray, byte> detectedFaceForRecognizer;
        private const string _databasePath = @"Faces.db3";
        private const string _faceRecognizerPath = @"Recognizer.yaml";
        private IDataStoreAccess _dataStoreAccess;
        private EigenFaceRecognizer _faceRecognizer;


        private string _username;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        private string _recognizedFace;

        public string RecognizedFace
        {
            get { return _recognizedFace; }
            set
            {
                _recognizedFace = value;
                OnPropertyChanged("RecognizedFace");
            }
        }

        private SimpleCommand _addCommand;

        public SimpleCommand AddCommand
        {
            get
            {
                return _addCommand ??
                    (_addCommand = new SimpleCommand(
                        obj =>
                                {
                                    var result = _dataStoreAccess.SaveFace(Username, detectedFaceBytes);
                                    MessageBox.Show(result, "Результат сохранения", MessageBoxButton.OK);
                                },
                        obj =>
                                {
                                    var result = false;
                                    if (!string.IsNullOrEmpty(Username))
                                    {
                                        result = true;
                                    }
                                    
                                    return result;
                                }));
            }
        }

        private SimpleCommand _trainCommand;

        public SimpleCommand TrainCommand
        {
            get
            {
                return _trainCommand ??
                    (_trainCommand = new SimpleCommand(
                        obj =>
                        {
                            TrainRecognizer();
                        }));
            }
        }

        private SimpleCommand _recognizeCommand;

        public SimpleCommand RecognizeCommand
        {
            get
            {
                return _recognizeCommand ??
                    (_recognizeCommand = new SimpleCommand(
                        obj =>
                        {
                            Recognize();
                        }));
            }
        }

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

        private BitmapSource _detectedFace;

        public BitmapSource DetectedFace
        {
            get { return _detectedFace; }
            set
            {
                _detectedFace = value;
                OnPropertyChanged("DetectedFace");
            }
        }

        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            _dataStoreAccess = new DataStoreAccess(_databasePath);
            if (File.Exists(_faceRecognizerPath))
            {
                _faceRecognizer = new EigenFaceRecognizer();
                _faceRecognizer.Read(_faceRecognizerPath);
            }
            else
            {
                _faceRecognizer = new EigenFaceRecognizer();
            }
            InitializeFaceDetection();
        }

        private void Recognize()
        {
            _faceRecognizer.Read(_faceRecognizerPath);
            var result = _faceRecognizer.Predict(detectedFaceForRecognizer);
            if (result.Label != 0)
            {
                RecognizedFace = _dataStoreAccess.GetUserName(result.Label);
            }
        }

        private void TrainRecognizer()
        {
            var allFaces = _dataStoreAccess.GetFaces("ALL_USERS").ToList();
            if (allFaces != null)
            {
                var faceImages = new Image<Gray, byte>[allFaces.Count()];
                var faceUserIds = new int[allFaces.Count()];
                for (int i = 0; i < allFaces.Count(); i++)
                {
                    //var stream = new MemoryStream();
                    //stream.Write(allFaces[i].Image, 0, allFaces[i].Image.Length);
                    var faceImage = new Image<Gray, byte>(100, 100);
                    faceImage.Bytes = allFaces[i].Image;
                    faceImages[i] = faceImage;
                    faceUserIds[i] = allFaces[i].UserId;
                }
                _faceRecognizer.Train(faceImages, faceUserIds);
                _faceRecognizer.Write(_faceRecognizerPath);
            }
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
            Image<Bgr, byte> detectedFace = null;
            _camera.Read(currentFrameMat);

            if (!currentFrameMat.IsEmpty)
            {
                var currentFrame = currentFrameMat.ToImage<Bgr, byte>();

                var grayFrame = currentFrameMat.ToImage<Gray, Byte>();

                var detectedFaces = _faceClassifier.DetectMultiScale(grayFrame);

                foreach (var face in detectedFaces)
                {
                    currentFrame.Draw(face, new Bgr(0, double.MaxValue, 0), 3);
                    detectedFace = currentFrame.GetSubRect(face);
                }
                UserImage = ToBitmapSource(currentFrame);
                if (detectedFace != null)
                {
                    detectedFaceForRecognizer = detectedFace.Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);
                    detectedFaceBytes = detectedFaceForRecognizer.Bytes;
                    DetectedFace = ToBitmapSource(detectedFace);
                }
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
