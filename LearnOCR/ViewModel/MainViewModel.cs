using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LearnOCR.Model;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace LearnOCR.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public const string TessData = @"d:\tessdata";

        private readonly IDataService _dataService;
        public ObservableCollection<string> Actions { get; set; }

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }
            set
            {
                Set(ref _welcomeTitle, value);
            }
        }
        /// <summary>
        /// The <see cref="BitmapSrc" /> property's name.
        /// </summary>
        public const string BitmapSrcPropertyName = "BitmapSrc";
        private BitmapSource _bitmapSrc = null;
        /// <summary>
        /// Sets and gets the BitmapSrc property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public BitmapSource BitmapSrc
        {
            get
            {
                return _bitmapSrc;
            }
            set
            {
                Set(BitmapSrcPropertyName, ref _bitmapSrc, value);
                Mat img = BitmapSourceConverter.ToMat(BitmapSrc);
                Cv2.CvtColor(img, SourceMat, ColorConversionCodes.BGR2GRAY);
                PixelWidth = img.Cols;
                PixelHeight = img.Rows;
            }
        }

        /// <summary>
        /// The <see cref="BitmapRoi" /> property's name.
        /// </summary>
        public const string BitmapRoiPropertyName = "BitmapRoi";

        private BitmapSource _bitmapRoi = null;

        /// <summary>
        /// Sets and gets the BitmapRoi property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public BitmapSource BitmapRoi
        {
            get
            {
                return _bitmapRoi;
            }
            set
            {
                Set(BitmapRoiPropertyName, ref _bitmapRoi, value);
            }
        }

        /// <summary>
        /// The <see cref="PixelWidth" /> property's name.
        /// </summary>
        public const string PixelWidthPropertyName = "PixelWidth";

        private int _pixelWidth = 2000;

        /// <summary>
        /// Sets and gets the PixelWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int PixelWidth
        {
            get
            {
                return _pixelWidth;
            }
            set
            {
                Set(PixelWidthPropertyName, ref _pixelWidth, value);
            }
        }

        /// <summary>
        /// The <see cref="PixelHeight" /> property's name.
        /// </summary>
        public const string PixelHeightPropertyName = "PixelHeight";

        private int _pixelHeight = 2000;

        /// <summary>
        /// Sets and gets the PixelHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int PixelHeight
        {
            get
            {
                return _pixelHeight;
            }
            set
            {
                Set(PixelHeightPropertyName, ref _pixelHeight, value);
            }
        }

        /// <summary>
        /// The <see cref="SourceMat" /> property's name.
        /// </summary>
        public const string SourceMatPropertyName = "SourceMat";

        private Mat _sourceMat = new Mat();

        /// <summary>
        /// Sets and gets the SourceMat property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Mat SourceMat
        {
            get
            {
                return _sourceMat;
            }
            set
            {
                Set(SourceMatPropertyName, ref _sourceMat, value);
            }
        }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });
            Actions = new ObservableCollection<string> { "Set Digits", "Outer Contour", "Ocr" };
        }
        private RelayCommand _RecognizeCommand;

        /// <summary>
        /// Gets the RecognizeCommand.
        /// </summary>
        public RelayCommand RecognizeCommand
        {
            get
            {
                return _RecognizeCommand
                    ?? (_RecognizeCommand = new RelayCommand(
                    () =>
                    {
                        Run();
                    },
                    () => true));
            }
        }
        /// <summary>
        /// The <see cref="Content" /> property's name.
        /// </summary>
        public const string ContentPropertyName = "Content";

        private string _Content = "";

        /// <summary>
        /// Sets and gets the Content property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                Set(ContentPropertyName, ref _Content, value);
            }
        }
        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
        ///        
        private void Run()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "*.png|*.png|*.jpg|*.jpg";
            if (dialog.ShowDialog() == true)
            {
                using (var fs = new FileStream(dialog.FileName, FileMode.Open))
                using (var tesseract = OCRTesseract.Create(TessData, "eng", "0123456789-"))
                {
                    Mat mat = Mat.FromStream(fs, ImreadModes.Grayscale);
                    Cv2.GaussianBlur(mat,mat, new Size(5, 5), 0);
                    tesseract.Run(mat,
                        out var outputText, out var componentRects, out var componentTexts, out var componentConfidences,ComponentLevels.TextLine);
                    string data = "(";
                    data+=outputText+")\n";
                    for (int i=0;i< componentRects.Length; i++)
                    {
                        data+=$"({componentTexts[i]}) apperred at {componentRects[i]} with confidence {componentConfidences[i]}\n";
                    }
                    Content = data;
                }

            }
        }
        private void Test()
        {
            //BitmapSourceConverter.ToMat()
        }

    }
}