using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MvvmLight1.Model;
using GalaSoft.MvvmLight.Views;
using System.IO;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System;
using System.Text;

namespace MvvmLight1.ViewModel
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// See http://www.mvvmlight.net
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
		private readonly IDataService _dataService;
        private readonly IDialogService2 _dialogService2;
  
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
        private RelayCommand _ExecuteCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand ExecuteCommand
        {
            get
            {
                return _ExecuteCommand
                    ?? (_ExecuteCommand = new RelayCommand(
                    () =>
                    {
                        //_dialogService2.ShowMessage("message", "title");
                        _dialogService2.ShowMessage($"Application.Current.Dispatcher==App.Current.Dispatcher?{System.Windows.Application.Current.Dispatcher == App.Current.Dispatcher}","App");
                        _dialogService2.ShowMessage($"Application==App?{typeof(System.Windows.Application) == typeof(App)}", "App");
                        BitmapSource src = GetBitmapSource(@"D:\mia中文\HonJangWithMia.jpg");
                        string hash = GetHash(src, "hash1.txt", "1.jpg");
                        string hash2= GetHash2(src, "hash2.txt", "2.jpg");
                        string hash3 = GetHash3(src, "hash3.txt", "3.jpg");
                        _dialogService2.ShowMessage(hash, "" + (hash == hash2)+ (hash == hash3));
                    }));
            }
        }
		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel(IDataService dataService, IDialogService2 dialogService2)
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
            _dialogService2 = dialogService2;
        }

        
        /// <summary>
        /// Convert a stream of image to BitmapImage
        /// </summary>
        /// <param name="stream">content of image with type Stream</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage Stream2BitmapImage(Stream stream)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = stream;
            bi.CacheOption = BitmapCacheOption.OnLoad; // let stream free
            bi.EndInit();
            return bi;
        }
        /// <summary>
        /// Encode BitmapSource src with encoder to MemoryStream
        /// </summary>
        /// <param name="src">BitmapSource to be encoded</param>
        /// <param name="encoder">encoder</param>
        /// <returns>Stream with content of image encoded by encoder</returns>
        public static MemoryStream BitmapSource2Stream(BitmapSource src, BitmapEncoder encoder)
        {
            encoder.Frames.Add(BitmapFrame.Create(src));
            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);
            ms.Position = 0;
            return ms;
        }
        public static BitmapSource GetBitmapSource(string file)
        {
            using (FileStream fs=new FileStream(file,FileMode.Open))
            {
                return Stream2BitmapImage(fs);
            }
        }
        public BitmapSource GetBitmapSource0(string file)
        {
            BitmapImage img = null;
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                MemoryStream myMs = new MemoryStream();
                //fs.CopyTo(myMs);
                //fs.Position = 0;
                //myMs.Position = 0;  // rewind myMs is necessary
                img = new BitmapImage();
                img.BeginInit();
                //img.StreamSource = myMs;
                img.StreamSource = fs;  // Why can not use filestream
                img.CacheOption = BitmapCacheOption.OnLoad; //如此 fs才有辦法脫身
                //img.UriSource = new Uri(file, UriKind.RelativeOrAbsolute);
                img.EndInit();
            }
            return img;
        }
        public static string GetHash(BitmapSource b, string hashfile = null, string jpgfile = null)
        {
            MemoryStream ms = new MemoryStream();
            JpegBitmapEncoder jpg = new JpegBitmapEncoder();
            jpg.Frames.Add(BitmapFrame.Create(b));
            jpg.Save(ms);
            if (jpgfile != null)
            {
                ms.Position = 0;
                using (Stream stm = File.Create(jpgfile))
                {
                    byte[] data = ms.ToArray();
                    stm.Write(data, 0, data.Length);
                }
            }
            ms.Position = 0;
            SHA256 sha = SHA256.Create();
            byte[] hashArray = sha.ComputeHash(ms);
            string hash = BitConverter.ToString(hashArray).Replace("-", "");
            if (hashfile != null)
            {
                StreamWriter sw = new StreamWriter(hashfile, false, Encoding.UTF8);
                sw.Write(hash);
                sw.Close();
            }
            return hash;
        }
        public static string GetHash2(BitmapSource b, string hashfile = null, string jpgfile = null)
        {
            MemoryStream ms = new MemoryStream();
            JpegBitmapEncoder jpg = new JpegBitmapEncoder();
            jpg.Frames.Add(BitmapFrame.Create(b));
            jpg.Save(ms);
            byte[] data = ms.ToArray();
            if (jpgfile != null)
            {
                using (Stream stm = File.Create(jpgfile))
                {
                    stm.Write(data, 0, data.Length);
                }
            }
            SHA256 sha = SHA256.Create();
            byte[] hashArray = sha.ComputeHash(data);
            string hash = BitConverter.ToString(hashArray).Replace("-", "");
            if (hashfile != null)
            {
                StreamWriter sw = new StreamWriter(hashfile, false, Encoding.UTF8);
                sw.Write(hash);
                sw.Close();
            }
            return hash;
        }
        public static string GetHash3(BitmapSource b, string hashfile = null, string jpgfile = null)
        {
            MemoryStream ms = BitmapSource2Stream(b,new JpegBitmapEncoder());
            byte[] data = ms.ToArray();
            if (jpgfile != null)
            {
                using (Stream stm = File.Create(jpgfile))
                {
                    stm.Write(data, 0, data.Length);
                }
            }
            SHA256 sha = SHA256.Create();
            byte[] hashArray = sha.ComputeHash(data);
            string hash = BitConverter.ToString(hashArray).Replace("-", "");
            if (hashfile != null)
            {
                StreamWriter sw = new StreamWriter(hashfile, false, Encoding.UTF8);
                sw.Write(hash);
                sw.Close();
            }
            return hash;
        }

    }
}