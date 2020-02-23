using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LearnOCR.Model;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Text;
using System.IO;

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
        private const string TessData = @"d:\tessdata";

        private readonly IDataService _dataService;

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

    }
}