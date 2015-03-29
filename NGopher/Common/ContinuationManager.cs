using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NGopher.Common
{
    // https://code.msdn.microsoft.com/windowsapps/File-picker-sample-9f294cba

    public class ContinuationManager
    {
        private IContinuationActivatedEventArgs args = null;
        private bool handled = false;
        private Guid id = Guid.Empty;

        internal void Continue(IContinuationActivatedEventArgs args)
        {
            Continue(args, Window.Current.Content as Frame);
        }

        internal void Continue(IContinuationActivatedEventArgs args, Frame rootFrame)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (this.args != null && !handled)
                throw new InvalidOperationException("can't set args more than once");

            this.args = args;
            this.handled = false;
            this.id = Guid.NewGuid();

            if (rootFrame == null) return;

            switch (args.Kind)
            {
                case ActivationKind.PickSaveFileContinuation:
                    var fspp = rootFrame.Content as IFileSavePickerContinuable;
                    if (fspp != null)
                    {
                        fspp.ContinueFileSavePicker(args as FileSavePickerContinuationEventArgs);
                    }
                    break;
            }
        }

        internal void MarkAsStale()
        {
            this.handled = true;
        }

        public IContinuationActivatedEventArgs ContinationArgs
        {
            get
            {
                if (handled) return null;
                MarkAsStale();
                return args;
            }
        }

        public IContinuationActivatedEventArgs GetContinuationArgs(bool includeStaleArgs)
        {
            if (!includeStaleArgs && handled) return null;
            MarkAsStale();
            return args;
        }
    }
}

interface IFileSavePickerContinuable
{
    void ContinueFileSavePicker(FileSavePickerContinuationEventArgs args);
}