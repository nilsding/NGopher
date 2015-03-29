using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
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
    public sealed partial class TextViewPage : Page
    {
        private GopherClient _gopher;
        private string _content;
        private string _selector;

        public TextViewPage()
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
            SelectorTextBlock.Text = _selector.ToUpper();
            var content = await _gopher.MakeTextRequest(_selector);
            _content = content.ToString();
            ContentTextBlock.Text = _content;
        }
    }
}
