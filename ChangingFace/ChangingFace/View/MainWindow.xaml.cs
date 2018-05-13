using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Microsoft.Win32;

namespace ChangingFace.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private VideoCapture _camera;
        //private DispatcherTimer _timer;
        //private CascadeClassifier _classifier;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel.MainWindowViewModel();
        }

        //private void InitializeCamera()
        //{
        //    _camera = new VideoCapture();
        //}

        //private void button_Click(object sender, RoutedEventArgs e)
        //{
        //    _camera = new VideoCapture();
        //    double fps = _camera.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
        //    _classifier = new CascadeClassifier(@"haarcascade_frontalface_alt_tree.xml");
        //    _timer = new DispatcherTimer();
        //    _timer.Tick += _timer_Tick;
        //    _timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 30);
        //    _timer.Start();
        //}

        //private void _timer_Tick(object sender, EventArgs e)
        //{
        //    var currentFrameMat = new Mat();
        //    _camera.Read(currentFrameMat);
            
        //    if (!currentFrameMat.IsEmpty)
        //    {
        //        var currentFrame = currentFrameMat.ToImage<Bgr, byte>();

        //        var grayFrame = currentFrameMat.ToImage<Gray, Byte>();

        //        var detectedFaces = _classifier.DetectMultiScale(grayFrame);

        //        foreach (var face in detectedFaces)
        //        {
        //            currentFrame.Draw(face, new Bgr(0, double.MaxValue, 0), 3);
        //        }
        //        userImage.Source = ToBitmapSource(currentFrame);
        //    }
        //}

        ///// <summary>
        ///// Delete a GDI object
        ///// </summary>
        ///// <param name="o">The poniter to the GDI object to be deleted</param>
        ///// <returns></returns>
        //[DllImport("gdi32")]
        //private static extern int DeleteObject(IntPtr o);
        
        //private BitmapSource ToBitmapSource(IImage image)
        //{
        //    using (Bitmap source = image.Bitmap)
        //    {
        //        IntPtr ptr = source.GetHbitmap();

        //        var result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //                        ptr,
        //                        IntPtr.Zero,
        //                        Int32Rect.Empty,
        //                        BitmapSizeOptions.FromEmptyOptions());
        //        DeleteObject(ptr);
        //        return result;
        //    }
        //}
    }
}
