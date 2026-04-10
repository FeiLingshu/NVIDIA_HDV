using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NVIDIA_HDV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            radioButton1.AutoCheck = false;
            radioButton2.AutoCheck = false;
            this.radioButton1.Click += RadioButton1_Click;
            this.radioButton2.Click += RadioButton2_Click;
            this.button1.Click += Button1_Click;
        }

        private volatile bool HTTPSTATE = false;

        private string HTTPPATH = string.Empty;

        private string HTTPCACHE = string.Empty;

        private void Form1_Load(object sender, EventArgs e)
        {
            FetchHtmlAsync("https://gitee.com/FeiLingshu/NVIDIA_HDV-cache/raw/master/cache").ContinueWith(result =>
            {
                if (result.IsCompleted && !string.IsNullOrEmpty(result.Result.Trim()) && IsValidUrl(result.Result.Trim()))
                {
                    HTTPPATH = result.Result.Trim();
                    FetchHtmlAsync(result.Result.Trim()).ContinueWith(result_1 =>
                    {
                        if (result_1.IsCompleted && !string.IsNullOrEmpty(result_1.Result.Trim()))
                        {
                            HTTPCACHE = result_1.Result.Trim();
                            HTTPSTATE = true;
                            this.Invoke(new Action(() =>
                            {
                                this.label1.ForeColor = Color.Green;
                                this.label2.ForeColor = Color.Green;
                                this.label2.Text = "成功";
                                this.button1.BackColor = Color.Green;
                                this.button1.ForeColor = Color.LightGray;
                            }));
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.label1.ForeColor = Color.Red;
                                this.label2.ForeColor = Color.Red;
                                this.label2.Text = "失败";
                                this.button1.BackColor = Color.Red;
                                this.button1.ForeColor = Color.LightGray;
                            }));
                        }
                    });
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        this.label1.ForeColor = Color.Red;
                        this.label2.ForeColor = Color.Red;
                        this.label2.Text = "失败";
                        this.button1.BackColor = Color.Red;
                        this.button1.ForeColor = Color.LightGray;
                    }));
                }
            });
        }

        private void RadioButton1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton1.Checked = false;
            }
            else
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
            }
        }

        private void RadioButton2_Click(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton2.Checked = false;
            }
            else
            {
                radioButton2.Checked = true;
                radioButton1.Checked = false;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (HTTPSTATE)
            {
                string JScache = HTTPCACHE;
                Regex regex = new Regex(@"requestParamaters=requestParamaters\.replace\(""numberOfResults=\d*?"",""numberOfResults=\d*?""\);");
                Match rmatch = regex.Match(JScache);
                if (rmatch.Success)
                {
                    string replace = JS;
                    replace = replace.Replace("%%VALUE%%", $"{numericUpDown1.Value}");
                    replace = replace.Replace("%%GRD%%", radioButton1.Checked ? "true" : "false");
                    replace = replace.Replace("%%STD%%", radioButton2.Checked ? "true" : "false");
                    replace = replace.Replace("%%SWITCH%%", checkBox1.Checked ? "true" : "false");
                    string add = string.Empty;
                    if (numericUpDown3.Value != 0)
                    {
                        add = $"release={numericUpDown3.Value}";
                    }
                    if (numericUpDown2.Value != 0)
                    {
                        add = $"version={numericUpDown2.Value}";
                    }
                    replace = replace.Replace("%%ADD%%", add);
                    replace = replace.Replace("%%REPLACE%%", "requestParamaters");
                    JScache = JScache.Replace(rmatch.Value.Trim(), rmatch.Value.Trim() + replace);
                    string path = HTTPPATH.Replace("https:/", string.Empty).Replace("/", "\\");
                    using (Process self = Process.GetCurrentProcess())
                    {
                        try
                        {
                            string _ = $"{Path.GetDirectoryName(self.MainModule.FileName)}{path}";
                            Directory.CreateDirectory(Path.GetDirectoryName(_));
                            if (File.Exists(_))
                            {
                                MD5 hash = MD5.Create();
                                string basehash = string.Join(
                                    "", hash.ComputeHash(File.ReadAllBytes(_)).Select(b => b.ToString("X2")).ToArray());
                                string newhash = string.Join(
                                    "", hash.ComputeHash(new UTF8Encoding(false).GetBytes(JScache)).Select(b => b.ToString("X2")).ToArray());
                                if (basehash == newhash)
                                {
                                    new Form2(this, "跳过：文件未更改").ShowDialog();
                                    return;
                                }
                            }
                            File.WriteAllText(_, JScache, new UTF8Encoding(false));
                        }
                        catch (Exception)
                        {
                            new Form2(this, "失败：文件写入异常").ShowDialog();
                            return;
                        }
                        new Form2(this, "成功：文件写入完成").ShowDialog();
                        return;
                    }
                }
                new Form2(this, "失败：JS代码校验失败").ShowDialog();
                return;
            }
        }

        private static readonly HttpClient _client = new HttpClient();

        private static async Task<string> FetchHtmlAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return false;
            return uri.Scheme == Uri.UriSchemeHttps;
        }

        #region Static Resource
        private const string JS = "if(%%SWITCH%%){requestParamaters=requestParamaters.replace(/dch=1/g,\"dch=0\");}if(%%GRD%%){requestParamaters=requestParamaters.replace(/isWHQL=\\d/g,\"isWHQL=1\");requestParamaters=requestParamaters.replace(/isWHQL=null/g,\"isWHQL=1\");requestParamaters=requestParamaters.replace(/upCRD=\\d/g,\"upCRD=0\");requestParamaters=requestParamaters.replace(/upCRD=null/g,\"upCRD=0\");}if(%%STD%%){requestParamaters=requestParamaters.replace(/isWHQL=\\d/g,\"isWHQL=0\");requestParamaters=requestParamaters.replace(/isWHQL=null/g,\"isWHQL=0\");requestParamaters=requestParamaters.replace(/upCRD=\\d/g,\"upCRD=1\");requestParamaters=requestParamaters.replace(/upCRD=null/g,\"upCRD=1\");}let replacestd=\"numberOfResults=%%VALUE%%\";let addstd=\"%%ADD%%\";if(addstd!=\"\"){replacestd=addstd+'&'+replacestd;}requestParamaters=requestParamaters.replace(/numberOfResults=\\d+/g,replacestd);console.log(\"NVIDIA_HDV 已替换查询参数 ->\",requestParamaters);";
        // if(%%SWITCH%%){requestParamaters=requestParamaters.replace(/dch=1/g,"dch=0");}if(%%GRD%%){requestParamaters=requestParamaters.replace(/isWHQL=\d/g,"isWHQL=1");requestParamaters=requestParamaters.replace(/isWHQL=null/g,"isWHQL=1");requestParamaters=requestParamaters.replace(/upCRD=\d/g,"upCRD=0");requestParamaters=requestParamaters.replace(/upCRD=null/g,"upCRD=0");}if(%%STD%%){requestParamaters=requestParamaters.replace(/isWHQL=\d/g,"isWHQL=0");requestParamaters=requestParamaters.replace(/isWHQL=null/g,"isWHQL=0");requestParamaters=requestParamaters.replace(/upCRD=\d/g,"upCRD=1");requestParamaters=requestParamaters.replace(/upCRD=null/g,"upCRD=1");}let replacestd="numberOfResults=%%VALUE%%";let addstd="%%ADD%%";if(addstd!=""){replacestd=addstd+'&'+replacestd;}requestParamaters=requestParamaters.replace(/numberOfResults=\d+/g,replacestd);console.log("NVIDIA_HDV 已替换查询参数 ->",requestParamaters);
        #endregion
    }
}
