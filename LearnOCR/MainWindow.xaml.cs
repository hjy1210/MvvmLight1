using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LearnOCR.ViewModel;
using Microsoft.Win32;
using OpenCvSharp.Extensions;
using OpenCvSharp.Text;

namespace LearnOCR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;
        bool dragging;
        Point start;
        Point end;
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainViewModel;
            image1.RenderTransform = new ScaleTransform(200.0 / 96, 200.0 / 96);
            image2.RenderTransform = new ScaleTransform(100.0 / 96, 100.0 / 96);
        }

        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            // MessageBox.Show(viewModel.PixelHeight.ToString());
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "*.jpg|*.jpg";
            if (dialog.ShowDialog() == true)
            {
                viewModel.BitmapSrc = new BitmapImage(new Uri(dialog.FileName));
                viewModel.PixelHeight = viewModel.BitmapSrc.PixelHeight;
                viewModel.PixelWidth = viewModel.BitmapSrc.PixelWidth;
                image1.RenderTransform= new ScaleTransform(viewModel.BitmapSrc.DpiX / 96, viewModel.BitmapSrc.DpiY / 96);
            }
        }

        private void canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            dragging = true;
            canvas.CaptureMouse();
            start = e.GetPosition(canvas);
            //tbxData.Text = $"{p.X}, {p.Y}";
        }

        void GetRoi(Rectangle r, out int left, out int top, out int right, out int bottom)
        {
            left = (int)((double)r.GetValue(Canvas.LeftProperty));
            top = (int)((double)r.GetValue(Canvas.TopProperty));
            right = (int)r.Width+left;
            bottom = (int)r.Height+top;
        }

        private void canvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            end = e.GetPosition(canvas);
            //tbxData.Text = $"{p.X}, {p.Y}";
            dragging = false;
            canvas.ReleaseMouseCapture();
        }

        private void DoOcr()
        {
            if (canvas.Children.Count >= 2 && (canvas.Children[1] as Rectangle) != null)
            {
                Rectangle rect = canvas.Children[1] as Rectangle;
                GetRoi(rect, out int left, out int top, out int right, out int bottom);
                OpenCvSharp.Mat m = new OpenCvSharp.Mat();
                if (top < bottom && left < right && left >= 0 && right < viewModel.SourceMat.Cols && top >= 0 && bottom < viewModel.SourceMat.Rows)
                {
                    viewModel.SourceMat.SubMat(top, bottom, left, right).CopyTo(m);
                    OpenCvSharp.Cv2.HConcat(new OpenCvSharp.Mat[] { m, m, m, m }, m);
                    viewModel.BitmapRoi = BitmapSourceConverter.ToBitmapSource(m);
                    using (var tesseract = OCRTesseract.Create(MainViewModel.TessData, "eng", "0123456789-"))
                    {
                        OpenCvSharp.Cv2.GaussianBlur(m, m, new OpenCvSharp.Size(5, 5), 0);
                        tesseract.Run(m,
                            out var outputText, out var componentRects, out var componentTexts, out var componentConfidences, ComponentLevels.TextLine);
                        string data = "(";
                        data += outputText + ")\n";
                        for (int i = 0; i < componentRects.Length; i++)
                        {
                            data += $"({componentTexts[i]}) apperred at {componentRects[i]} with confidence {componentConfidences[i]}\n";
                        }
                        tbxData.Text = outputText;
                    }
                }
            }
        }

        private void canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (dragging)
            {
                end = e.GetPosition(canvas);
                canvas.Children.RemoveRange(1, canvas.Children.Count - 1);
                Rectangle rect = new Rectangle();
                double left = Math.Min(start.X, end.X);
                double right = Math.Max(start.X, end.X);
                double top = Math.Min(start.Y, end.Y);
                double bottom = Math.Max(start.Y, end.Y);
                rect.Stroke = Brushes.Blue;
                rect.Width = right - left;
                rect.Height = bottom - top;
                Canvas.SetTop(rect, top);
                Canvas.SetLeft(rect, left);
                canvas.Children.Add(rect);
            }
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            if (cbbActions.SelectedIndex == 2 && (canvas.Children[1] as Rectangle) != null)
                DoOcr();
        }
    }
}