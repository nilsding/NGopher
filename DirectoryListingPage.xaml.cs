using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
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
    public sealed partial class DirectoryListingPage : Page
    {
        private GopherClient _gopher;
        private string _selector = "/";

        public DirectoryListingPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
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
            SelectorTextBlock.Text = _selector.ToUpper();
            LoadDirectoryList();
        }

        private async void LoadDirectoryList()
        {
            var contents = await _gopher.GetDirectoryContents(_selector);
            DirectoryListView.ItemsSource = contents;
        }

        private async void DirectoryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            DirectoryListView.IsEnabled = false;
            var item = e.ClickedItem as GopherItem;
            if (item != null)
                switch (item.Type)
                {
                    case GopherItem.TYPE_FILE:
                        this.Frame.Navigate(typeof (TextViewPage),
                            string.Format("{0}:{1}|{2}", item.Host, item.Port, item.Selector));
                        break;
                    case GopherItem.TYPE_DIRECTORY:
                        this.Frame.Navigate(typeof(DirectoryListingPage),
                            string.Format("{0}:{1}|{2}", item.Host, item.Port, item.Selector));
                        break;
                    case GopherItem.TYPE_PHONEBOOK:
                    case GopherItem.TYPE_ERROR:
                    case GopherItem.TYPE_BINHEX:
                    case GopherItem.TYPE_PC_DOS_BIN:
                    case GopherItem.TYPE_UUENCODE:
                        await new MessageDialog(String.Format("{0} is not implemented yet.", item.FriendlyName)).ShowAsync();
                        break;
                    case GopherItem.TYPE_INDEXSEARCH:
                        var dlg = new SearchDialog(item);
                        var result = await dlg.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            this.Frame.Navigate(typeof(DirectoryListingPage),
                                string.Format("{0}:{1}|{2}\t{3}", item.Host, item.Port, item.Selector, dlg.SearchText));
                        }
                        break;
                    case GopherItem.TYPE_TELNET:
                    case GopherItem.TYPE_BINARY:
                    case GopherItem.TYPE_REDUNDANT:
                    case GopherItem.TYPE_TN3270:
                        await new MessageDialog(String.Format("{0} is not implemented yet.", item.FriendlyName)).ShowAsync();
                        break;
                    case GopherItem.TYPE_GIF:  // fallthrough
                    case GopherItem.TYPE_PNG:  // fallthrough
                    case GopherItem.TYPE_IMAGE:
                        this.Frame.Navigate(typeof(ImageViewPage),
                            string.Format("{0}:{1}|{2}", item.Host, item.Port, item.Selector));
                        break;
                    case GopherItem.TYPE_HTML:
                        var url = item.Selector.TrimStart('/');
                        if (url.ToUpper().StartsWith("URL:"))
                        {
                            var uri = new Uri(url.Substring(4));
                            await Windows.System.Launcher.LaunchUriAsync(uri);
                            break;
                        }
                        this.Frame.Navigate(typeof (WebViewPage),
                            string.Format("{0}:{1}|{2}", item.Host, item.Port, item.Selector));
                        break;
                    case GopherItem.TYPE_INFO:
                        await new MessageDialog(item.UserName).ShowAsync();
                        break;
                    case GopherItem.TYPE_AUDIO:
                        await new MessageDialog(String.Format("{0} is not implemented yet.", item.FriendlyName)).ShowAsync();
                        break;
                    default:
                        await new MessageDialog(String.Format("Unknown type: '{0}'.", item.Type)).ShowAsync();
                        break;
                }
            DirectoryListView.IsEnabled = true;
        }
    }
}
