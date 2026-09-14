using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace NVIDIA_HDV
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
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
            void Type_Record(object sender, MouseButtonEventArgs e)
            {
                MLBD = sender;
            }
            void Type_Checked(object sender, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource)
                {
                    RadioButton rb = sender as RadioButton;
                    if (rb.IsChecked == true)
                    {
                        rb.IsChecked = false;
                        e.Handled = true;
                        rb.ReleaseMouseCapture();
                    }
                }
            }
            this.GameReady.PreviewMouseDown += Type_Record;
            this.Studio.PreviewMouseDown += Type_Record;
            this.GameReady.PreviewMouseUp += Type_Checked;
            this.Studio.PreviewMouseUp += Type_Checked;
            Regex numregex = new Regex("[^0-9]");
            Regex numregex_1 = new Regex("[^0-9.]");
            void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
            {
                e.Handled = numregex.IsMatch(e.Text) || !ushort.TryParse($"{((TextBox)sender).Text}{e.Text}", out _);
            }
            void Number_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
            {
                e.Handled = numregex_1.IsMatch(e.Text) || !double.TryParse($"{((TextBox)sender).Text}{e.Text}", out double d) || d > ushort.MaxValue;
            }
            void OnPastingEvent(object sender, DataObjectPastingEventArgs e)
            {
                e.CancelCommand();
            }
            this.Count.PreviewTextInput += Number_PreviewTextInput;
            this.Version.PreviewTextInput += Number_PreviewTextInput_1;
            this.Release.PreviewTextInput += Number_PreviewTextInput;
            DataObject.AddPastingHandler(this.Count, OnPastingEvent);
            DataObject.AddPastingHandler(this.Version, OnPastingEvent);
            DataObject.AddPastingHandler(this.Release, OnPastingEvent);
            void KillFocus(object sender, MouseButtonEventArgs e)
            {
                if (!(e.OriginalSource is TextBox || e.OriginalSource is CheckBox || e.OriginalSource is RadioButton))
                {
                    this.NULLPART.Focus();
                }
            }
            this.MouseLeftButtonUp += KillFocus;
            string HTTPPATH = null;
            string HTTPCACHE = null;
            string[] BETADATA = null;
            this.Loaded += (s, e) =>
            {
                Task.Run(async () =>
                {
                    using (var handler = new HttpClientHandler())
                    {
                        handler.AllowAutoRedirect = true;
                        using (var client = new HttpClient(handler))
                        {
                            client.Timeout = TimeSpan.FromSeconds(10);
                            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                            client.DefaultRequestHeaders.Accept.ParseAdd(
                                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(
                                "en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");
                            bool HTTPSTATE = false;
                            bool BETA = false;
                            var result = FetchHtmlAsync(client, "https://gitee.com/FeiLingshu/NVIDIA_HDV-cache/raw/master/cache");
                            await result;
                            if (result.IsCompleted && !string.IsNullOrEmpty(result.Result.Trim()) && IsValidUrl(result.Result.Trim()))
                            {
                                HTTPPATH = result.Result.Trim();
                                var result_1 = FetchHtmlAsync(client, result.Result.Trim());
                                await result_1;
                                if (result_1.IsCompleted && !string.IsNullOrEmpty(result_1.Result.Trim()))
                                {
                                    HTTPCACHE = result_1.Result.Trim();
                                    HTTPSTATE = true;
                                }
                                else
                                {
                                    if (result_1.IsFaulted) _ = result_1.Exception;
                                }
                            }
                            else
                            {
                                if (result.IsFaulted) _ = result.Exception;
                            }
                            var result_2 = FetchHtmlAsync(client, "https://gitee.com/FeiLingshu/NVIDIA_HDV-cache/raw/master/beta");
                            await result_2;
                            if (result.IsCompleted && !string.IsNullOrEmpty(result.Result.Trim()))
                            {
                                BETADATA = result.Result.Trim().Split('\n');
                                BETA = true;
                            }
                            else
                            {
                                if (result.IsFaulted) _ = result.Exception;
                            }
                            if (HTTPSTATE && BETA)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    this.Net.Foreground = (SolidColorBrush)this.Resources["Green"];
                                    this.Net_Status.Foreground = (SolidColorBrush)this.Resources["Green"];
                                    this.Net_Status.Text = "成功";
                                    this.GO.IsEnabled = true;
                                });
                            }
                            else
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    this.Net.Foreground = (SolidColorBrush)this.Resources["Red"];
                                    this.Net_Status.Foreground = (SolidColorBrush)this.Resources["Red"];
                                    this.Net_Status.Text = "失败";
                                    this.GO.IsEnabled = false;
                                });
                            }
                        }
                    }
                });
            };
            this.ContentRendered += (s, e) =>
            {
                IntPtr selfhandle = new WindowInteropHelper(this).Handle;
                App.RedrawWindow(
                    selfhandle,
                    IntPtr.Zero, IntPtr.Zero,
                    App.RDW_INVALIDATE | App.RDW_ALLCHILDREN);
            };
            this.GO.IsEnabled = false;
            this.GO.MouseLeftButtonUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource)
                {
                    if (string.IsNullOrEmpty(HTTPPATH) || string.IsNullOrEmpty(HTTPCACHE) || BETADATA == null)
                    {
                        this.GO.IsEnabled = false;
                        return;
                    }
                    string JScache = HTTPCACHE;
                    Regex regex = new Regex(@"requestParamaters\s*?=\s*?requestParamaters\.replace\(""numberOfResults=\d*?"",\s*?""numberOfResults=\d*?""\);");
                    Match rmatch = regex.Match(JScache);
                    if (rmatch.Success)
                    {
                        string replace = App.JS;
                        ushort.TryParse(this.Count.Text, out ushort value);
                        replace = replace.Replace("%%VALUE%%", $"{(value == 0 ? 50 : value)}");
                        replace = replace.Replace("%%GRD%%", GameReady.IsChecked == true ? "true" : "false");
                        replace = replace.Replace("%%STD%%", Studio.IsChecked == true ? "true" : "false");
                        replace = replace.Replace("%%SWITCH%%", Standard.IsChecked == true ? "true" : "false");
                        string add = string.Empty;
                        ushort.TryParse(this.Release.Text, out ushort value_r);
                        ushort.TryParse(this.Version.Text, out ushort value_v);
                        if (value_r > 0)
                        {
                            add = $"release={value_r}";
                        }
                        if (value_v > 0)
                        {
                            add = $"version={value_v}";
                        }
                        replace = replace.Replace("%%ADD%%", add);
                        replace = replace.Replace("%%REPLACE%%", "requestParamaters");
                        JScache = JScache.Replace(rmatch.Value.Trim(), rmatch.Value.Trim() + replace);
                        if (BETADATA.Length > 0)
                        {
                            Regex _regex = new Regex(@"DriverSearch:\s*?function\(.*?,\s*?osID,.*?,\s*?dch,.*?\)\s*?\{");
                            Match _rmatch = _regex.Match(JScache);
                            if (_rmatch.Success)
                            {
                                string[] ifstrings = new string[BETADATA.Length];
                                for (int i = 0; i < BETADATA.Length; i++)
                                {
                                    ifstrings[i] = $"osID==\"{BETADATA[i].Trim()}\"";
                                }
                                string idcheck = $"if({string.Join("||", ifstrings)}){{dch=\"1\";}}";
                                JScache = JScache.Replace(_rmatch.Value.Trim(), _rmatch.Value.Trim() + idcheck);
                            }
                        }
                        using (Process self = Process.GetCurrentProcess())
                        {
                            string _ = null;
                            try
                            {
                                _ = $"{Path.GetDirectoryName(self.MainModule.FileName)}\\NVIDIA_HDV.js";
                                bool need = true;
                                if (File.Exists(_))
                                {
                                    MD5 hash = MD5.Create();
                                    string basehash = string.Join(
                                        "", hash.ComputeHash(File.ReadAllBytes(_)).Select(b => b.ToString("X2")).ToArray());
                                    string newhash = string.Join(
                                        "", hash.ComputeHash(new UTF8Encoding(false).GetBytes(JScache)).Select(b => b.ToString("X2")).ToArray());
                                    if (basehash == newhash)
                                    {
                                        need = false;
                                        SystemSounds.Asterisk.Play();
                                        new Notify(this, "JS代码已生成\n\n详细信息：文件未更改\n此页面关闭后将自动进入NVIDIA官网").ShowDialog();
                                    }
                                }
                                if (need)
                                {
                                    File.WriteAllText(_, JScache, new UTF8Encoding(false));
                                    SystemSounds.Asterisk.Play();
                                    new Notify(this, "JS代码已生成\n\n详细信息：文件已成功写入\n此页面关闭后将自动进入NVIDIA官网").ShowDialog();
                                }
                            }
                            catch (Exception exp)
                            {
                                SystemSounds.Hand.Play();
                                new Notify(this, $"JS代码生成失败\n\n文件无法写入\n详细信息：{exp.Message}").ShowDialog();
                                return;
                            }
                            string webview2error = null;
                            this.Hide();
                            try
                            {
                                MainWindow window = new MainWindow("https://www.nvidia.cn/drivers", HTTPPATH, _)
                                {
                                    Owner = this
                                };
                                window.ShowDialog();
                            }
                            catch (Exception exp)
                            {
                                webview2error = exp.Message;
                            }
                            this.Show();
                            this.Activate();
                            if (webview2error != null)
                            {
                                SystemSounds.Hand.Play();
                                new Notify(this, $"WebView2组件出现异常\n\n详细信息：{webview2error}").ShowDialog();
                            }
                            return;
                        }
                    }
                    SystemSounds.Hand.Play();
                    new Notify(this, "JS代码校验失败\n\n请及时向开发者报告此错误").ShowDialog();
                    return;
                }
            };
        }

        private object MLBD = null;
        private object MRBD = null;

        private static async Task<string> FetchHtmlAsync(HttpClient client, string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return false;
            return uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
