using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
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
        private readonly Settings settings = null;
        private readonly string HTTPSURL = null;
        private readonly string JSURL = null;
        private readonly string JSFILE = null;
        private readonly Regex regex = null;
        private readonly string[] htmllist = null;
        private readonly string[] uicsslist = null;
        private readonly string[] csslist = null;

        private volatile bool closeflag = false;

        public MainWindow(Window owner, string httpsurl, string jsurl, string jsfile, Regex regex, string[] htmllist, string[] uicsslist, string[] csslist, bool DNS = false)
        {
            InitializeComponent();
            this.Owner = owner;
            if (owner is Settings settings)
            {
                this.settings = settings;
            }
            else
            {
                throw new ArgumentException("WebView2 组件所在容器接收了无效的参数。");
            }
            this.HTTPSURL = httpsurl ?? string.Empty;
            this.JSURL = jsurl ?? string.Empty;
            this.JSFILE = jsfile ?? string.Empty;
            this.htmllist = htmllist;
            this.uicsslist = uicsslist;
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
            this.Closing += (s, e) =>
            {
                if (this.WebView24WPF.CoreWebView2 == null || !closeflag)
                {
                    e.Cancel = true;
                    return;
                }
                cts.Cancel();
                try
                {
                    this.WebView24WPF.CoreWebView2.WebResourceRequested -= OnWebResourceRequested;
                    this.WebView24WPF.Stop();
                    this.WebView24WPF.Dispose();
                }
                catch (Exception) { }
            };
            this.CLOSE.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource)
                {
                    this.Close();
                }
                e.Handled = true;
            };
            this.MINI.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.WindowState = WindowState.Minimized;
                e.Handled = true;
            };
            this.HOME.IsEnabled = false;
            this.FLUSH.IsEnabled = false;
            this.BACK.IsEnabled = false;
            this.HOME.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.WebView24WPF.CoreWebView2.Navigate(HTTPSURL);
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
                ReIP = DNS & this.settings.DNSfin;
                _ = InitializeWebView().ContinueWith(t =>
                {
                    throw t.Exception;
                }, TaskContinuationOptions.OnlyOnFaulted);
            };
        }

        private object MLBD = null;
        private object MRBD = null;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private async Task InitializeWebView()
        {
            await this.WebView24WPF.EnsureCoreWebView2Async();
            // await new SemaphoreSlim(0, 1).WaitAsync(); //...
            var coreWebView = this.WebView24WPF.CoreWebView2;
            if (ReIP)
            {
                void __wv2nvcn(object s, CoreWebView2WebResourceResponseReceivedEventArgs e) => OnWebResourceResponseReceived(0, e);
                void __wv2gfcn(object s, CoreWebView2WebResourceResponseReceivedEventArgs e) => OnWebResourceResponseReceived(1, e);
                this.WebView24WPF.IsHitTestVisible = false;
                this.WebView24WPF_Mask.Visibility = Visibility.Visible;
                int retry;
                retry = 0;
                bool r0 = false;
                do
                {
                    IPs[0].Clear();
                    Count[0] = 0;
                    cts_singles[0].Dispose();
                    cts_singles[0] = new CancellationTokenSource();
                    coreWebView.WebResourceResponseReceived += __wv2nvcn;
                    if (retry == 0)
                    {
                        coreWebView.Navigate(Hosts[0]);
                    }
                    else
                    {
                        coreWebView.Reload();
                    }
                    r0 = await IPWaiter.WaitAsync(10000);
                    coreWebView.WebResourceResponseReceived -= __wv2nvcn;
                    if (r0) cts_singles[0].Cancel();
                    await Task.Delay(500);
                    if (!string.IsNullOrWhiteSpace(FinalIP[0])) break;
                    retry++;
                } while (retry < 3);
                retry = 0;
                bool r1 = false;
                do
                {
                    IPs[1].Clear();
                    Count[1] = 0;
                    cts_singles[1].Dispose();
                    cts_singles[1] = new CancellationTokenSource();
                    coreWebView.WebResourceResponseReceived += __wv2gfcn;
                    if (retry == 0)
                    {
                        coreWebView.Navigate(Hosts[1]);
                    }
                    else
                    {
                        coreWebView.Reload();
                    }
                    r1 = await IPWaiter.WaitAsync(10000);
                    coreWebView.WebResourceResponseReceived -= __wv2gfcn;
                    if (r1) cts_singles[1].Cancel();
                    await Task.Delay(500);
                    if (!string.IsNullOrWhiteSpace(FinalIP[1])) break;
                    retry++;
                } while (retry < 3);
                coreWebView.Navigate("about:blank");
                if (string.IsNullOrWhiteSpace(FinalIP[0]) || string.IsNullOrWhiteSpace(FinalIP[1]))
                {
                    new Notify(this, $"IP 调优数据获取失败\n\n错误类型：{(r0 ? 0 : 1) + (r1 ? 0 : 2)}-{((IPs[0].Count == 0) ? 1 : 0) + ((IPs[1].Count == 0) ? 2 : 0)}\n本次实例将不再执行调优", false).ShowDialog();
                }
                else
                {
                    new Notify(this, $"已获取 IP 调优数据\n\nwww.nvidia.cn     ->  {FinalIP[0],15}\ngfwsl.geforce.cn  ->  {FinalIP[1],15}\n----------------\n数据将自动写入 hosts 文件，请确认杀毒软件不会拦截该操作", false).ShowDialog();
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = this.settings.DNSpath,
                        Arguments = $"\"{FinalIP[0]}\" \"www.nvidia.cn\" \"{FinalIP[1]}\" \"gfwsl.geforce.cn\"",
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    };
                    using (Process process = new Process { StartInfo = startInfo })
                    {
                        IntPtr handle = new WindowInteropHelper(this).Handle;
                        EnableWindow(handle, false);
                        process.Start();
                        await Task.Factory.StartNew(() =>
                        {
                            process.WaitForExit();
                        }, TaskCreationOptions.LongRunning);
                        Console.WriteLine($"[NVIDIA_DNS] -> {process.ExitCode}");
                        EnableWindow(handle, true);
                    }
                }
                this.settings.DNSfin = false;
                coreWebView.Reload();
                this.WebView24WPF_Mask.Visibility = Visibility.Collapsed;
                this.WebView24WPF.IsHitTestVisible = true;
            }
            closeflag = true;
            coreWebView.AddWebResourceRequestedFilter(JSURL, CoreWebView2WebResourceContext.Script);
            foreach (var html in htmllist)
            {
                coreWebView.AddWebResourceRequestedFilter(html, CoreWebView2WebResourceContext.Document);
            }
            foreach (var uicss in uicsslist)
            {
                coreWebView.AddWebResourceRequestedFilter(uicss, CoreWebView2WebResourceContext.Stylesheet‌);
            }
            foreach (var css in csslist)
            {
                coreWebView.AddWebResourceRequestedFilter(css, CoreWebView2WebResourceContext.Stylesheet‌);
            }
            coreWebView.WebResourceRequested += OnWebResourceRequested;
            coreWebView.Navigate(HTTPSURL);
            this.HOME.IsEnabled = true;
            this.FLUSH.IsEnabled = true;
            this.BACK.IsEnabled = true;
        }

        private bool ReIP = false;
        private readonly string[] Hosts = new string[2] { "https://site.ip138.com/www.nvidia.cn/", "https://site.ip138.com/gfwsl.geforce.cn/" };
        private readonly HashSet<string>[] IPs = new HashSet<string>[2] { new HashSet<string>(64, StringComparer.OrdinalIgnoreCase), new HashSet<string>(64, StringComparer.OrdinalIgnoreCase) };
        private readonly int[] Count = new int[2] { 0, 0 };
        private readonly string[] FinalIP = new string[2] { null, null };
        private readonly SemaphoreSlim Single = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource[] cts_singles = new CancellationTokenSource[2] { new CancellationTokenSource(), new CancellationTokenSource() };
        private readonly Regex jsonpreg = new Regex(@"^\w+\((.*)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly SemaphoreSlim IPWaiter = new SemaphoreSlim(0, 1);

        private async Task CacheIPs(int index, string response)
        {
            await Single.WaitAsync();
            try
            {
                if (cts_singles[index].IsCancellationRequested) return;
                if (string.IsNullOrWhiteSpace(response)) return;
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                object json = serializer.DeserializeObject(response);
                if (json is Dictionary<string, object> _json
                    && _json.TryGetValue("status", out object status)
                    && status is bool _status && _status
                    && _json.TryGetValue("data", out object data)
                    && data != null && data is object[] _data)
                {
                    foreach (var d in _data)
                    {
                        if (d is Dictionary<string, object> _d
                            && _d.TryGetValue("ip", out object ip)
                            && ip is string _ip)
                        {
                            IPs[index].Add(_ip);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"未知错误：{ex.Message}"); }
            finally { Single.Release(); }
        }

        private async Task FindIP(int index, string response)
        {
            await Single.WaitAsync();
            try
            {
                if (cts_singles[index].IsCancellationRequested) return;
                if (string.IsNullOrWhiteSpace(response)) return;
                Match match = jsonpreg.Match(response);
                if (match.Success)
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    object json = serializer.DeserializeObject(match.Groups[1].Value);
                    if (json is Dictionary<string, object> _json
                        && _json.TryGetValue("ret", out object ret)
                        && ret is string _ret && string.Equals(_ret, "ok", StringComparison.OrdinalIgnoreCase)
                        && _json.TryGetValue("ip", out object ip)
                        && ip is string _ip
                        && _json.TryGetValue("data", out object data)
                        && data != null && data is object[] _data)
                    {
                        Count[index]++;
                        foreach (var d in _data)
                        {
                            if (FinalIP[index] == null
                                && d is string _d && string.Equals(_d.Trim(), "美国")
                                && IPs[index].Contains(_ip))
                            {
                                FinalIP[index] = _ip;
                                cts_singles[index].Cancel();
                                IPWaiter.Release();
                                return;
                            }
                        }
                        if (IPs[index].Count != 0 && Count[index] == IPs[index].Count)
                        {
                            cts_singles[index].Cancel();
                            IPWaiter.Release();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"未知错误：{ex.Message}"); }
            finally { Single.Release(); }
        }

        private async void OnWebResourceResponseReceived(int index, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            try
            {
                string requestUri = e.Request.Uri;
                string method = e.Request.Method;
                if (method == "GET")
                {
                    if ((requestUri.StartsWith("https://site.ip138.com/domain/read.do?domain=www.nvidia.cn", StringComparison.OrdinalIgnoreCase)
                        || requestUri.StartsWith("https://site.ip138.com/domain/read.do?domain=gfwsl.geforce.cn", StringComparison.OrdinalIgnoreCase))
                        && e.Response.Headers.GetHeader("Content-Type").StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        if (cts_singles[index].IsCancellationRequested) return;
                        using (Stream contentStream = await e.Response.GetContentAsync())
                        {
                            if (contentStream != null)
                            {
                                using (StreamReader reader = new StreamReader(contentStream, Encoding.UTF8))
                                {
                                    _ = CacheIPs(index, await reader.ReadToEndAsync());
                                }
                            }
                        }
                    }
                    else if (requestUri.StartsWith("https://api.ipshudi.com/query/?", StringComparison.OrdinalIgnoreCase)
                        && e.Response.Headers.GetHeader("Content-Type").StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream contentStream = await e.Response.GetContentAsync())
                        {
                            if (contentStream != null)
                            {
                                using (StreamReader reader = new StreamReader(contentStream, Encoding.UTF8))
                                {
                                    _ = FindIP(index, await reader.ReadToEndAsync());
                                }
                            }
                        }
                    }
                    else { return; }
                }
            }
            catch (COMException) { Console.WriteLine("WebView2 -> 底层COM异常，由导航中断触发，属正常现象"); }
            catch (Exception) { }
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
                        fs.Close();
                    }
                    if (jsContent != null)
                    {
                        var stream = new MemoryStream(jsContent);
                        if (cts.Token.IsCancellationRequested) return;
                        e.Response = this.WebView24WPF.CoreWebView2.Environment.CreateWebResourceResponse(
                            stream, 200, "OK", "Content-Type: application/javascript; charset=utf-8"
                        );
                    }
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
                        var data = await this.settings.GetHttp(e.Request.Uri, CoreWebView2WebResourceContext.Document);
                        if (data != null)
                        {
                            var stream = new MemoryStream(data);
                            if (cts.Token.IsCancellationRequested) return;
                            e.Response = this.WebView24WPF.CoreWebView2.Environment.CreateWebResourceResponse(
                                stream, 200, "OK", "Content-Type: text/html; charset=utf-8"
                            );
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        deferral.Complete();
                    }
                }
                else if (uicsslist.Contains(e.Request.Uri))
                {
                    var deferral = e.GetDeferral();
                    try
                    {
                        var data = await this.settings.GetHttp(e.Request.Uri, CoreWebView2WebResourceContext.Stylesheet, true);
                        if (data != null)
                        {
                            var stream = new MemoryStream(data);
                            if (cts.Token.IsCancellationRequested) return;
                            e.Response = this.WebView24WPF.CoreWebView2.Environment.CreateWebResourceResponse(
                                stream, 200, "OK", "Content-Type: text/css; charset=utf-8"
                            );
                        }
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
                        var data = await this.settings.GetHttp(e.Request.Uri, CoreWebView2WebResourceContext.Stylesheet);
                        if (data != null)
                        {
                            var stream = new MemoryStream(data);
                            if (cts.Token.IsCancellationRequested) return;
                            e.Response = this.WebView24WPF.CoreWebView2.Environment.CreateWebResourceResponse(
                                stream, 200, "OK", "Content-Type: text/css; charset=utf-8"
                            );
                        }
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



        [DllImport("user32.dll")]
        private extern static bool EnableWindow(IntPtr hWnd, bool bEnable);
    }
}
