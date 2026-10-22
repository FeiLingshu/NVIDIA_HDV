using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NVIDIA_DNS
{
    public class Program
    {
        private const string HOSTS_FILE_PATH = @"C:\Windows\System32\drivers\etc\hosts";

        private static readonly TimeSpan frame = TimeSpan.FromMilliseconds(1000D / 30D);

        public static async Task Main(string[] args)
        {
            Console.Title = "NVIDIA_DNS · FeiLingshu";
            ConsoleColor ccb = Console.BackgroundColor = ConsoleColor.Black;
            ConsoleColor ccf = Console.ForegroundColor = ConsoleColor.White;
            AppDomain.CurrentDomain.UnhandledException += (s, e) => OnExceptions(e.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (s, e) => OnExceptions(e.Exception);
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += (s, e) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(Console.CursorLeft == 0 ? null : "\n");
                Console.WriteLine(AllBreak);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" Break");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($">_");
                Console.ForegroundColor = ccf;
                Console.WriteLine("检测到终止信号 [^C]");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" 按任意键退出...");
                Console.ReadKey(true);
                Environment.Exit(unchecked((int)0xC000013A));
            };
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.Write(" NVIDIA_DNS");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(">_");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"NVIDIA 站点 IP 调优模块");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" · ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("本模块仅提供向 hosts 文件写入配置的功能");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" · ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("功能基于 ip138 网页接口，请不要频繁调用，否则可能导致设备 IP 被 CDN 限制访问");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" · ");
            Console.ForegroundColor = ccf;
            Console.Write("由");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" FeiLingshu ");
            Console.ForegroundColor = ccf;
            Console.WriteLine("为 NVIDIA_HDV 开发");
            await Task.Delay(frame);
            if (args.Length == 4
                && !string.IsNullOrWhiteSpace(args[0] = args[0].Replace(" ", string.Empty)) && IPAddress.TryParse(args[0], out _)
                && !string.IsNullOrWhiteSpace(args[1] = args[1].Replace(" ", string.Empty))
                && !string.IsNullOrWhiteSpace(args[2] = args[2].Replace(" ", string.Empty)) && IPAddress.TryParse(args[2], out _)
                && !string.IsNullOrWhiteSpace(args[3] = args[3].Replace(" ", string.Empty)))
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write(" NVIDIA_DNS");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(">_");
                Console.ForegroundColor = ccf;
                Console.WriteLine($"读取到传入参数");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" · ");
                Console.ForegroundColor = ccf;
                Console.Write($"{args[0],-17}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{args[1]}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" · ");
                Console.ForegroundColor = ccf;
                Console.Write($"{args[2],-17}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{args[3]}");
                await Task.Delay(frame);
                try
                {
                    FileInfo hosts = new FileInfo(HOSTS_FILE_PATH);
                    if (hosts.Exists)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(" NVIDIA_DNS");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(">_");
                        Console.ForegroundColor = ccf;
                        Console.Write($"识别到");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(" hosts ");
                        Console.ForegroundColor = ccf;
                        Console.WriteLine($"文件路径：{HOSTS_FILE_PATH}");
                        await Task.Delay(frame);
                    }
                    else
                    {
                        throw new FileNotFoundException("未能识别到 hosts 文件。");
                    }
                    if (hosts.IsReadOnly) hosts.IsReadOnly = false;
                    string[] lines = File.ReadAllLines(HOSTS_FILE_PATH, Encoding.Default);
                    StringBuilder sb = new StringBuilder();
                    string line1 = $"{args[0],-17}{args[1]}";
                    string line2 = $"{args[2],-17}{args[3]}";
                    foreach (string line in lines)
                    {
                        string tline = line.Trim();
                        if (!string.IsNullOrEmpty(tline) && !tline.StartsWith("#"))
                        {
                            List<string> parts = tline.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if (parts.Count == 2)
                            {
                                if (parts[1].Equals(args[1], StringComparison.OrdinalIgnoreCase)
                                    || parts[1].Equals(args[3], StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }
                            else if (parts.Count > 2)
                            {
                                List<int> indexlist = new List<int>(parts.Count - 1);
                                for (int i = 1; i < parts.Count; i++)
                                {
                                    if (parts[i].Equals(args[1], StringComparison.OrdinalIgnoreCase)
                                        || parts[i].Equals(args[3], StringComparison.OrdinalIgnoreCase))
                                    {
                                        indexlist.Add(i);
                                    }
                                }
                                if (indexlist.Count > 0)
                                {
                                    if (indexlist.Count == parts.Count - 1) continue;
                                    sb.Append(parts[0]);
                                    for (int i = 1; i < parts.Count; i++)
                                    {
                                        if (!indexlist.Contains(i)) sb.Append($" {parts[i]}");
                                    }
                                    sb.AppendLine();
                                    continue;
                                }
                            }
                        }
                        sb.AppendLine(line);
                    }
                    sb.AppendLine(line1);
                    sb.AppendLine(line2);
                    File.WriteAllText(HOSTS_FILE_PATH, sb.ToString(), Encoding.Default);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                throw new ArgumentException("命令行参数无效。");
            }
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(" NVIDIA_DNS");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(">_");
            Console.ForegroundColor = ccf;
            Console.Write($"已将数据写入");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" hosts ");
            Console.ForegroundColor = ccf;
            Console.WriteLine($"文件");
            await Task.Delay(frame);
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C ipconfig /flushdns",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(" NVIDIA_DNS");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(">_");
                        Console.ForegroundColor = ccf;
                        Console.WriteLine($"已成功刷新 DNS 缓存");
                    }
                    else
                    {
                        throw new Win32Exception(process.ExitCode, "刷新 DNS 缓存失败。");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            Console.WriteLine();
            Console.WriteLine(AllBreak);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(" Note");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(">_");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"即使刷新了 DNS 缓存，新写入的 IP 信息可能依旧无法加载到当前实例\n       ");
            Console.WriteLine($"如果网络条件未改善，请重启 NVIDIA_HDV");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" 按任意键退出...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        private static readonly string AllBreak = " ================";

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void OnExceptions(Exception exception)
        {
            exception = exception.GetBaseException();
            exception = (exception is RuntimeWrappedException wrappedEx) ? (Exception)wrappedEx.WrappedException : exception;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(Console.CursorLeft == 0 ? null : "\n");
            Console.WriteLine(AllBreak);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" Crash");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($">_");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[{0}] {1}\n -> {2}\n{3}",
                exception.GetType().Name,
                exception.Message,
                exception.Source,
                exception.StackTrace);
            Console.Write(" 按任意键退出...");
            Console.ReadKey(true);
            Environment.Exit(Marshal.GetHRForException(exception));
        }
    }
}
