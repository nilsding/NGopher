using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NGopher.Gopher;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NGopher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageViewPage : Page, IFileSavePickerContinuable
    {
        private GopherClient _gopher;
        private string _selector;
        private byte[] _content;
        private BitmapImage _image;

        public ImageViewPage()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) return;

            var server = e.Parameter as string;
            var x = server.Split('\x01');
            var y = x[1].Split('\x02');
            _gopher = new GopherClient
            {
                Server = x[0],
                Port = Convert.ToUInt16(y[0])
            };
            _selector = y[1];
            SelectorTextBlock.Text = _selector.ToUpper();
            _content = await _gopher.MakeBinaryRequest(_selector);
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(_content);
                    writer.StoreAsync().GetResults();
                }
                _image = new BitmapImage();
                await _image.SetSourceAsync(ms);
                ContentImage.Source = _image;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var fn = _selector.Split('/').Last();
            var fsp = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fn
            };
            fsp.FileTypeChoices.Add("Image file", new List<string> { ".gif", ".png", ".jpg", ".jpeg", ".bmp" });
            fsp.PickSaveFileAndContinue();
        }

        public async void ContinueFileSavePicker(FileSavePickerContinuationEventArgs args)
        {
            StorageFile file = args.File;

            if (file == null) return;  // saving cancelled

            CachedFileManager.DeferUpdates(file);
            await FileIO.WriteBytesAsync(file, _content);  // save _content to file
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            if (status == FileUpdateStatus.Complete)
            {
                // successfully saved file
            }
            else
            {
                // oh no
            }
        }
    }
}
