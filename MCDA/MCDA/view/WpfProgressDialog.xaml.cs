using MCDA.ViewModel;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MCDA
{
    /// <summary>
    /// Interaction logic for WpfProgressDialog.xaml
    /// </summary>
    public partial class WpfProgressDialog : Window
    {
        const int WS_DISABLED = 0x08000000;
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Hides the Cancel Button if true
        /// </summary>
        public bool HideCancelButton { get; set; }
        /// <summary>
        /// Progress bar is indeterminate if true
        /// </summary>
        public bool IsIndeterminate { get; set; }

        public WpfProgressDialog()
        {
            InitializeComponent();

            DataContext = new WpfProgressDialogViewModel();

            Loaded += WpfProgressDialog_Loaded;

        }

        public void SetNativeEnabled(bool enabled)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) &
                ~WS_DISABLED | (enabled ? 0 : WS_DISABLED));
        }

        private void WpfProgressDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // remove alt+f4
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            ProgressBar.IsIndeterminate = IsIndeterminate;
            CancelButton.Visibility = HideCancelButton ? Visibility.Hidden : Visibility.Visible;
        }

        public void ShowDialog(Delegate actionDelegate, params object[] args)
        {
            actionDelegate?.DynamicInvoke(args);
            base.ShowDialog();

            //base.Close();
        }

        /// <summary>
        /// Disabled
        /// </summary>
        /// <param name="e" />
        protected override void OnClosing(CancelEventArgs e)
        {
            //TODO
            //base.OnClosing(e);
            //e.Cancel = true;
        }

    }
}
