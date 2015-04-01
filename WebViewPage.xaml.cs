using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NGopher.Gopher;

namespace NGopher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebViewPage : Page, IFileSavePickerContinuable
    {
        private GopherClient _gopher;
        private string _content;
        private string _selector;

        public WebViewPage()
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
            var x = server.Split(':');
            var y = x[1].Split('|');
            _gopher = new GopherClient
            {
                Server = x[0],
                Port = Convert.ToUInt16(y[0])
            };

            _selector = y[1];
            NavigateTo(_selector);
        }

        private async void NavigateTo(string selector)
        {
            SelectorTextBlock.Text = selector.ToUpper();
            var content = await _gopher.MakeTextRequest(selector);
            _content = content.ToString();
            ContentWebView.NavigateToString(_content);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var fn = _selector.Split('/').Last().Replace(".txt", "");
            var fsp = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = fn
            };
            fsp.FileTypeChoices.Add("HTML Page", new List<string> { ".html", ".htm" });
            fsp.PickSaveFileAndContinue();
        }

        public async void ContinueFileSavePicker(FileSavePickerContinuationEventArgs args)
        {
            StorageFile file = args.File;

            if (file == null) return;  // saving cancelled

            CachedFileManager.DeferUpdates(file);
            await FileIO.WriteTextAsync(file, _content);  // save _content to file
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

        /// <summary>
        /// Handle relative URLs by querying the Gopher server.
        /// </summary>
        private void ContentWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri.OriginalString.StartsWith("about:"))
            {
                args.Cancel = true;
                var url = args.Uri.OriginalString.Remove(0, 6); // 6 = "about:".length
                // let's assume we'll be sent to a HTML page afterwards
                NavigateTo(_selector.Substring(0, _selector.LastIndexOf('/')) + "/" + url);
            }
            /*BackAppBarButton.IsEnabled = ContentWebView.CanGoBack;
            ForwardAppBarButton.IsEnabled = ContentWebView.CanGoForward;*/
        }

        /* TODO: make back/forward work
        private void BackAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentWebView.CanGoBack)
                ContentWebView.GoBack();
            BackAppBarButton.IsEnabled = ContentWebView.CanGoBack;
        }

        private void ForwardAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentWebView.CanGoForward)
                ContentWebView.GoForward();
            BackAppBarButton.IsEnabled = ContentWebView.CanGoForward;
        }
        */
    }
}
