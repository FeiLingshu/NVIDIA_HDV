using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
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
        public string DNSpath = null;
        public volatile bool DNSfin = true;

        public Settings()
        {
            InitializeComponent();
            this.Http = GetHttpHost();
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
            Regex htmlregex = null;
            List<string> htmllist = new List<string>();
            List<string> uicsslist = new List<string>();
            List<string> csslist = new List<string>();
            this.Loaded += (s, e) =>
            {
                Task.Run(async () =>
                {
                    bool HTTPSTATE = false;
                    bool BETA = false;
                    try
                    {
                        var result = FetchHtmlAsync(Http, "https://gitee.com/FeiLingshu/NVIDIA_HDV-cache/raw/master/cache");
                        await result;
                        if (result.IsCompleted && !string.IsNullOrEmpty(result.Result.Trim()) && IsValidUrl(result.Result.Trim()))
                        {
                            HTTPPATH = result.Result.Trim();
                            var result_1 = FetchHtmlAsync(Http, result.Result.Trim());
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
                        var result_theme = FetchHtmlAsync(Http, "https://gitee.com/FeiLingshu/NVIDIA_HDV-cache/raw/master/cache_theme");
                        await result_theme;
                        if (result_theme.IsCompleted && !string.IsNullOrEmpty(result_theme.Result.Trim()))
                        {
                            var urls = result_theme.Result.Trim().Split('\n');
                            foreach (var url in urls)
                            {
                                if (!IsValidUrl(url)) continue;
                                if (url.EndsWith("/"))
                                {
                                    htmllist.Add(url);
                                }
                                else if (url.EndsWith("*kaizen"))
                                {
                                    uicsslist.Add(url.Substring(0, url.Length - 7));
                                }
                                else if (url.EndsWith("*"))
                                {
                                    htmlregex = new Regex($"^{url.Substring(0, url.Length - 1)}{@"\d+/$"}");
                                    htmllist.Add($"{url}/");
                                }
                                else
                                {
                                    csslist.Add(url);
                                }
                            }
                        }
                        else
                        {
                            if (result_theme.IsFaulted) _ = result_theme.Exception;
                        }
                        var result_2 = FetchHtmlAsync(Http, "https://gitee.com/FeiLingshu/NVIDIA_HDV-cache/raw/master/beta");
                        await result_2;
                        if (result_2.IsCompleted && !string.IsNullOrEmpty(result_2.Result.Trim()))
                        {
                            BETADATA = result_2.Result.Trim().Split('\n');
                            BETA = true;
                        }
                        else
                        {
                            if (result_2.IsFaulted) _ = result_2.Exception;
                        }
                    }
                    catch (Exception) { }
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
                        double.TryParse(this.Version.Text, out double value_v);
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
                                        new Notify(this, "JS代码已生成\n\n详细信息：文件未更改\n此页面关闭后将自动进入NVIDIA官网", true).ShowDialog();
                                    }
                                }
                                if (need)
                                {
                                    File.WriteAllText(_, JScache, new UTF8Encoding(false));
                                    SystemSounds.Asterisk.Play();
                                    new Notify(this, "JS代码已生成\n\n详细信息：文件已成功写入\n此页面关闭后将自动进入NVIDIA官网", true).ShowDialog();
                                }
                            }
                            catch (Exception exp)
                            {
                                SystemSounds.Hand.Play();
                                new Notify(this, $"JS代码生成失败\n\n文件无法写入\n详细信息：{exp.Message}", true).ShowDialog();
                                return;
                            }
                            string webview2error = null;
                            this.Hide();
                            try
                            {
                                new MainWindow(this, "https://www.nvidia.cn/drivers", HTTPPATH, _, htmlregex, htmllist.ToArray(), uicsslist.ToArray(), csslist.ToArray(), !string.IsNullOrWhiteSpace(DNSpath)).ShowDialog();
                                GC.Collect(2, GCCollectionMode.Forced, blocking: false, compacting: false);
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
                                new Notify(this, $"WebView2组件出现异常\n\n详细信息：{webview2error}", true).ShowDialog();
                            }
                            return;
                        }
                    }
                    SystemSounds.Hand.Play();
                    new Notify(this, "JS代码校验失败\n\n请及时向开发者报告此错误", true).ShowDialog();
                    return;
                }
            };
        }

        private object MLBD = null;
        private object MRBD = null;

        private bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return false;
            return uri.Scheme == Uri.UriSchemeHttps;
        }

        private readonly Dictionary<string, string> BgColorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "#f7f7f7", "#202020" },
            { "#ffffff", "#303030" },
            { "#fff", "#303030" },
            { "#c0c0c0", "#303030" },
            { "#f5f5f5", "#404040" },
            { "#fefefe", "#404040" },
            { "#000000", "#303030" },
            { "#000", "#303030" }
        };

        private readonly Dictionary<string, string> ColorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "#ccc", "#404040" },
            { "#1a1a1a", "#808080" },
            { "#000000", "#C0C0C0" },
            { "#000", "#C0C0C0" },
            { "#333333", "#C0C0C0" }
        };

        private readonly Regex CssRegex = new Regex(
            @"\b(background-color|background|color)\s*:\s*(#[0-9A-Fa-f]{3,6})\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private string Bytes(string origin)
        {
            if (string.IsNullOrEmpty(origin)) return origin;
            return CssRegex.Replace(origin, match =>
            {
                string prop = match.Groups[1].Value;
                string color = match.Groups[2].Value;
                string newColor = null;
                if (prop.Equals("color", StringComparison.OrdinalIgnoreCase))
                {
                    ColorMap.TryGetValue(color, out newColor);
                }
                else
                {
                    BgColorMap.TryGetValue(color, out newColor);
                }
                return (newColor == null) ? match.Value : $"{prop}:{newColor}";
            });
        }

        private readonly Regex[] HtmlRegex = new Regex[6]
        {
            new Regex(
                @"\b(\w+)-light\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            ),
            new Regex(
                @"<body[^>]*(?<=[\s""'])class\s*=\s*[""']",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            ),
            new Regex(
                @"((?<=[\s""'])class\s*=\s*[""'])([^""']*)([""'])",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            ),
            new Regex(
                @"((?<=[\s""'])fill\s*=\s*[""'])black([""']\s*/>)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            ),
            new Regex(
                @"(<g[^>]*(?<=[\s""'])id\s*=\s*[""']nvidia-logo[""'])([^>]*>)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            ),
            new Regex(
                @"(.*)(?<=[\s""'])nv-teaser--bg-white(?=[\s""'])(.*)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        };

        private byte[] HtmlBytes(string origin)
        {
            if (string.IsNullOrEmpty(origin)) return null;
            origin = HtmlRegex[0].Replace(origin, "$1-dark");
            origin = HtmlRegex[1].Replace(origin, "$0nv-dark ");
            origin = HtmlRegex[2].Replace(origin, match =>
            {
                HashSet<string> strings = new HashSet<string>(match.Groups[2].Value.Split(' '), StringComparer.OrdinalIgnoreCase);
                if ((strings.Contains("nav-header") && strings.Contains("desktop-nav"))
                    || strings.Contains("global-footer-container")
                    || strings.Contains("global-footer")
                    /*|| strings.Contains("color-black")*/)
                {
                    return $"{match.Groups[1].Value}nv-dark {match.Groups[2].Value}{match.Groups[3].Value}";
                }
                if (strings.Contains("nv-teaser--bg-white"))
                {
                    return HtmlRegex[5].Replace(match.Value, "$1nv-teaser--bg-black$2");
                }
                return match.Value;
            });
            origin = HtmlRegex[3].Replace(origin, "$1#76B900$2");
            origin = HtmlRegex[4].Replace(origin, "$1 fill=\"#76B900\"$2");
            origin = Bytes(origin);
            return new UTF8Encoding(false).GetBytes(origin);
        }

        private readonly bool UIoverride = false;

        private readonly Regex[] UICssRegex = new Regex[2]
        {
            new Regex(
                @"\b--color-black\s*:\s*(#[0-9A-Fa-f]{3,6})\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            ),
            new Regex(
                @"\b--color-white\s*:\s*(#[0-9A-Fa-f]{3,6})\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        };

        private byte[] UICssBytes(string origin)
        {
            if (string.IsNullOrEmpty(origin)) return null;
            if (UIoverride)
            {
                string color = UICssRegex[0].Match(origin).Groups[1].Value;
                origin = UICssRegex[1].Replace(origin, $"--color-white:{color}");
            }
            return new UTF8Encoding(false).GetBytes(origin);
        }

        private byte[] CssBytes(string origin)
        {
            origin = Bytes(origin);
            return new UTF8Encoding(false).GetBytes(origin);
        }

        public readonly HttpClient Http = null;

        private HttpClient GetHttpHost()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true
            };
            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            //client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("DNT", "1");
            //client.DefaultRequestHeaders.Add("Host", "www.nvidia.cn");
            //client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            //client.DefaultRequestHeaders.Add("Referer", "https://www.nvidia.cn/");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0");
            bool _1 = client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Microsoft Edge\";v=\"120\", \"Chromium\";v=\"120\", \"Not(A:Brand\";v=\"24\"");
            bool _2 = client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            bool _3 = client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            bool _4 = client.DefaultRequestHeaders.TryAddWithoutValidation("sec-gpc", "1");
            return client;
        }

        public async Task<string> FetchHtmlAsync(HttpClient client, string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> GetHttp(string url, CoreWebView2WebResourceContext type, bool isUI = false)
        {
            try
            {
                var result = FetchHtmlAsync(Http, url);
                await result;
                if (result.IsCompleted && !string.IsNullOrEmpty(result.Result.Trim()))
                {
                    switch (type)
                    {
                        case CoreWebView2WebResourceContext.Document:
                            return HtmlBytes(result.Result.Trim());
                        case CoreWebView2WebResourceContext.Stylesheet:
                            return isUI ? UICssBytes(result.Result.Trim()) : CssBytes(result.Result.Trim());
                        default:
                            return new UTF8Encoding(false).GetBytes(result.Result.Trim());
                    }
                }
                else
                {
                    if (result.IsFaulted) _ = result.Exception;
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
