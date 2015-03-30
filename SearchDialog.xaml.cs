using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NGopher.Gopher;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NGopher
{
    public sealed partial class SearchDialog : ContentDialog
    {
        public string SearchText { get; private set; }

        public SearchDialog()
        {
            this.InitializeComponent();
        }

        public SearchDialog(GopherItem item)
        {
            this.InitializeComponent();
            InfoTextBlock.Text = item.UserName;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SearchText = SearchTextBox.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
