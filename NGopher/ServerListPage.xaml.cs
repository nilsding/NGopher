﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NGopher.Gopher;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace NGopher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerListPage : Page
    {
        private List<String> _serverList = new List<string>()
        {
            "gopher.semmel.ch:70",
            "gopher.floodgap.com:70"
        };

        public ServerListPage()
        {
            this.InitializeComponent();
            ServerListView.ItemsSource = _serverList;

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            GopherClient gc = new GopherClient
            {
                Server = "gopher.floodgap.com",
                Port = 70
            };

            var x = await gc.GetDirectoryContents();
            if (x == null) return;
            foreach (var y in x)
            {
                Debug.WriteLine(y);
            }
        }

        private void ServerListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ServerListView.IsEnabled = false;
            var sel = e.ClickedItem as string;
            this.Frame.Navigate(typeof(DirectoryListingPage), sel + "|/");
            ServerListView.IsEnabled = true;
        }

    }
}
