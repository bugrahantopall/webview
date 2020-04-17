using Microsoft.Toolkit.Win32.UI.Controls;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WebView2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isFullScreen;
        private bool processExitedAttached;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseBack_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WebView1 != null && this.WebView1.CanGoBack;
        }

        private void BrowseBack_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WebView1?.GoBack();
        }

        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WebView1 != null && this.WebView1.CanGoForward;
        }

        private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void GoToPage_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var result = (Uri)new WebBrowserUriTypeConverter().ConvertFromString(this.Url.Text);
            //this.WebView1.Source = result;
            InitiliazeScript().ContinueWith((res) => { ControlReturnValue(res.Result); });
        }

        private void BrowseForward_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.WebView1?.GoForward();
        }

        private void Url_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && this.WebView1 != null)
            {
                var result =
                    (Uri)new WebBrowserUriTypeConverter().ConvertFromString(
                        this.Url.Text);
                this.WebView1.Source = result;
            }
        }

        private void WebView1_OnNavigationCompleted(object sender, WebViewControlNavigationCompletedEventArgs e)
        {
            this.Url.Text = e.Uri?.ToString() ?? string.Empty;
            this.Title = this.WebView1.DocumentTitle;
            if (!e.IsSuccess)
            {
                MessageBox.Show($"Could not navigate to {e.Uri?.ToString() ?? "NULL"}", $"Error: {e.WebErrorStatus}", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void WebView1_OnNavigationStarting(object sender, WebViewControlNavigationStartingEventArgs e)
        {
            //this.TryAttachProcessExitedEventHandler();
            this.Title = $"Waiting for {e.Uri?.Host ?? string.Empty}";
            this.Url.Text = e.Uri?.ToString() ?? string.Empty;
        }

        private void WebView1_OnPermissionRequested(object sender, WebViewControlPermissionRequestedEventArgs e)
        {
            if (e.PermissionRequest.State == WebViewControlPermissionState.Allow)
            {
                return;
            }

            var msg = $"Allow {e.PermissionRequest.Uri.Host} to access {e.PermissionRequest.PermissionType}?";

            var response = MessageBox.Show(msg, "Permission Request", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

            if (response == MessageBoxResult.Yes)
            {
                if (e.PermissionRequest.State == WebViewControlPermissionState.Defer)
                {
                    this.WebView1.GetDeferredPermissionRequestById(e.PermissionRequest.Id)?.Allow();
                }
                else
                {
                    e.PermissionRequest.Allow();
                }

            }
            else
            {
                if (e.PermissionRequest.State == WebViewControlPermissionState.Defer)
                {
                    this.WebView1.GetDeferredPermissionRequestById(e.PermissionRequest.Id)?.Deny();
                }
                else
                {
                    e.PermissionRequest.Deny();
                }
            }
        }

        private void WebView1_OnScriptNotify(object sender, WebViewControlScriptNotifyEventArgs e)
        {
            MessageBox.Show(e.Value, e.Uri?.ToString() ?? string.Empty);
        }

        private void WebView1_OnContainsFullScreenElementChanged(object sender, object e)
        {
            void EnterFullScreen()
            {
                this.WindowState = WindowState.Normal;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;
            }

            void LeaveFullScreen()
            {
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowState = WindowState.Normal;
            }

            // Toggle
            this.isFullScreen = !this.isFullScreen;

            if (this.isFullScreen)
            {
                EnterFullScreen();
            }
            else
            {
                LeaveFullScreen();
            }
        }


        private void WebView1_UnviewableContentIdentified(object sender, WebViewControlUnviewableContentIdentifiedEventArgs e)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(e.Uri, "deneme.mpeg");
            }
        }

        private void TryAttachProcessExitedEventHandler()
        {
            if (!this.processExitedAttached && this.WebView1?.Process != null)
            {
                this.WebView1.Process.ProcessExited += (o, a) =>
                {
                    // WebView has encountered and error and was terminated
                    this.Close();
                };

                this.processExitedAttached = true;
            }
        }

        private void WebView1_DOMContentLoaded(object sender, WebViewControlDOMContentLoadedEventArgs e)
        {
            //InitiliazeScript().ContinueWith((res) => { ControlReturnValue(res.Result); });
            //InitiliazeScriptNonAsync();
        }

        private void InitiliazeScriptNonAsync()
        {
            string result = WebView1.InvokeScript("EmlakFunc");
            MessageBox.Show(result);
        }

        private async Task<bool> InitiliazeScript()
        {
            string xxx = await WebView1.InvokeScriptAsync("EmlakFunc");

            if (string.IsNullOrEmpty(xxx))
                return true;
            return false;
        }

        public void Deneme()
        {
            MessageBox.Show("dışarıdan func çağırıldı!");
        }

        private async void ControlReturnValue(bool result)
        {
            if (result)
                MessageBox.Show("oldu");
            else
                MessageBox.Show("olmadı");

        }

        private void WebView1_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("sayfa yüklendi");
        }

        private void WebView1_FrameDOMContentLoaded(object sender, WebViewControlDOMContentLoadedEventArgs e)
        {
            MessageBox.Show("frame yüklendi");

        }
    }
}
