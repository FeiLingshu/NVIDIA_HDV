using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NVIDIA_HDV
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string HTTPSURL = null;
        private readonly string JSURL = null;
        private readonly string JSFILE = null;
        private readonly Regex regex = null;
        private readonly string[] htmllist = null;
        private readonly string[] csslist = null;

        public MainWindow(string httpsurl, string jsurl, string jsfile, Regex regex, string[] htmllist, string[] csslist)
        {
            InitializeComponent();
            this.HTTPSURL = httpsurl ?? string.Empty;
            this.JSURL = jsurl ?? string.Empty;
            this.JSFILE = jsfile ?? string.Empty;
            this.htmllist = htmllist;
            this.regex = regex;
            this.csslist = csslist;
            this.MouseLeftButtonDown += (s, e) => MLBD = e.OriginalSource;
            this.MouseRightButtonDown += (s, e) => MRBD = e.OriginalSource;
            this.TITLE.MouseLeftButtonDown += (s, e) =>
            {
                this.DragMove();
            };
            this.TITLE.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Right && MRBD != e.OriginalSource) e.Handled = true;
            };
            void blockmd(object s, MouseButtonEventArgs e)
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        MLBD = s;
                        break;
                    case MouseButton.Right:
                        MRBD = s;
                        break;
                    default:
                        break;
                }
                e.Handled = true;
            }
            this.CLOSE.PreviewMouseDown += blockmd;
            this.MINI.PreviewMouseDown += blockmd;
            this.HOME.PreviewMouseDown += blockmd;
            this.FLUSH.PreviewMouseDown += blockmd;
            this.BACK.PreviewMouseDown += blockmd;
            this.CLOSE.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.Close();
                e.Handled = true;
            };
            this.MINI.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.WindowState = WindowState.Minimized;
                e.Handled = true;
            };
            this.HOME.IsEnabled = false;
            this.FLUSH.IsEnabled = false;
            this.HOME.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.WebView24WPF.Source = new Uri(HTTPSURL);
                e.Handled = true;
            };
            this.FLUSH.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.WebView24WPF.Reload();
                e.Handled = true;
            };
            this.BACK.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.WebView24WPF.GoBack();
                e.Handled = true;
            };
            this.ContentRendered += (s, e) =>
            {
                IntPtr selfhandle = new WindowInteropHelper(this).Handle;
                App.RedrawWindow(
                    selfhandle,
                    IntPtr.Zero, IntPtr.Zero,
                    App.RDW_INVALIDATE | App.RDW_ALLCHILDREN);
                long style = GetWindowLongPtr(selfhandle, GWL_STYLE);
                if (style != 0)
                {
                    style &= ~WS_MAXIMIZEBOX;
                    SetWindowLongPtr(selfhandle, GWL_STYLE, style);
                    SetWindowPos(selfhandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);
                }
                _ = InitializeWebView();
            };
        }

        private object MLBD = null;
        private object MRBD = null;

        private async Task InitializeWebView()
        {
            await this.WebView24WPF.EnsureCoreWebView2Async();
            var coreWebView = this.WebView24WPF.CoreWebView2;
            coreWebView.AddWebResourceRequestedFilter(JSURL, CoreWebView2WebResourceContext.Script);
            foreach (var html in htmllist)
            {
                coreWebView.AddWebResourceRequestedFilter(html, CoreWebView2WebResourceContext.Document);
            }
            foreach (var css in csslist)
            {
                coreWebView.AddWebResourceRequestedFilter(css, CoreWebView2WebResourceContext.Stylesheet‌);
            }
            coreWebView.WebResourceRequested += OnWebResourceRequested;
            this.WebView24WPF.Source = new Uri(HTTPSURL);
            this.HOME.IsEnabled = true;
            this.FLUSH.IsEnabled = true;
        }

        private async void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (e.Request.Uri == JSURL)
            {
                var deferral = e.GetDeferral();
                try
                {
                    byte[] jsContent;
                    using (FileStream fs = File.Open(JSFILE, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        jsContent = new byte[fs.Length];
                        await fs.ReadAsync(jsContent, 0, jsContent.Length);
                    }
                    var stream = new MemoryStream(jsContent);
                    e.Response = this.WebView24WPF.CoreWebView2.Environment.CreateWebResourceResponse(
                        stream, 200, "OK", "Content-Type: application/javascript; charset=utf-8"
                    );
                }
                finally
                {
                    deferral.Complete();
                }
            }
            else
            {
                if ((regex != null && regex.IsMatch(e.Request.Uri)) || htmllist.Contains(e.Request.Uri))
                {
                    var deferral = e.GetDeferral();
                    try
                    {
                        var owner = (Settings)this.Owner;
                        var data = await owner.GetHtml(e.Request.Uri);
                        var stream = new MemoryStream(data);
                        e.Response = this.WebView24WPF.CoreWebView2.Environment.CreateWebResourceResponse(
                            stream, 200, "OK", "Content-Type: text/html; charset=utf-8"
                        );
                    }
                    catch (Exception) { }
                    finally
                    {
                        deferral.Complete();
                    }
                }
                else if (csslist.Contains(e.Request.Uri))
                {
                    var deferral = e.GetDeferral();
                    try
                    {
                        var owner = (Settings)this.Owner;
                        var data = await owner.GetHtml(e.Request.Uri);
                        var stream = new MemoryStream(data);
                        e.Response = this.WebView24WPF.CoreWebView2.Environment.CreateWebResourceResponse(
                            stream, 200, "OK", "Content-Type: text/css; charset=utf-8"
                        );
                    }
                    catch (Exception) { }
                    finally
                    {
                        deferral.Complete();
                    }
                }
            }
        }



        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long GetWindowLong64(IntPtr hWnd, long nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long GetWindowLong32(IntPtr hWnd, long nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long SetWindowLong64(IntPtr hWnd, int nIndex, long dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long SetWindowLong32(IntPtr hWnd, int nIndex, long dwNewLong);

        private static long GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (Environment.Is64BitProcess)
            {
                return GetWindowLong64(hWnd, nIndex);
            }
            else
            {
                return GetWindowLong32(hWnd, nIndex);
            }
        }
        private static long SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong)
        {
            if (Environment.Is64BitProcess)
            {
                return SetWindowLong64(hWnd, nIndex, dwNewLong);
            }
            else
            {
                return SetWindowLong32(hWnd, nIndex, dwNewLong);
            }
        }

        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_FRAMECHANGED = 0x0020;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
    }
}
