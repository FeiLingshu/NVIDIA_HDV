using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NVIDIA_HDV.Form2.API;

namespace NVIDIA_HDV
{
    public partial class Form2 : Form
    {
        private Point offset = Point.Empty;

        public Form2(Form parent, string data)
        {
            InitializeComponent();
            this.label2.Text = data;
            this.Shown += (sender, e) =>
            {
                Point p = parent.Location;
                Point c = this.Location;
                offset.X = p.X - c.X;
                offset.Y = p.Y - c.Y;
            };
            this.LocationChanged += (sender, e) =>
            {
                if (offset == Point.Empty) return;
                Point _ = this.Location;
                _.Offset(offset);
                parent.Location = _;
            };
            this.label1.MouseEnter += (sender, e) =>
            {
                this.label1.ForeColor = Color.LemonChiffon;
            };
            this.label1.MouseLeave += (sender, e) =>
            {
                this.label1.ForeColor = Color.DodgerBlue;
            };
            this.label1.Click += (sender, e) =>
            {
                string URL = "https://www.nvidia.cn/drivers/";
                string _ = GetDefaultBrowserPath_Windows();
                if (string.IsNullOrEmpty(_))
                {
                    try
                    {
                        Process.Start(URL);
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        Process.Start(_, $"--new-window \"{URL}\"");
                        Task.Run(() =>
                        {
                            try
                            {
                                Stopwatch timecount = Stopwatch.StartNew();
                                AutoResetEvent blocker = new AutoResetEvent(false);
                                while (timecount.Elapsed.TotalSeconds <= 3)
                                {
                                    IntPtr hwnd = GetForegroundWindow();
                                    if (hwnd != IntPtr.Zero)
                                    {
                                        GetWindowThreadProcessId(hwnd, out uint PID);
                                        if (PID != 0)
                                        {
                                            IntPtr phandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, PID);
                                            if (phandle != IntPtr.Zero)
                                            {
                                                StringBuilder path = new StringBuilder(32768);
                                                uint scount = (uint)path.Capacity;
                                                QueryFullProcessImageName(phandle, 0, path, ref scount);
                                                string p = path.ToString().Trim();
                                                path.Clear();
                                                if (!string.IsNullOrEmpty(p))
                                                {
                                                    if (p == _)
                                                    {
                                                        blocker.WaitOne(TimeSpan.FromMilliseconds(500), false);
                                                        SendF12Key();
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    blocker.WaitOne(TimeSpan.FromMilliseconds(15.6), false);
                                };
                            }
                            catch (Exception) { }
                        });
                    }
                    catch (Exception) { }
                }
            };
        }

        private static string GetDefaultBrowserPath_Windows()
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command"))
            {
                if (key?.GetValue(null) is string command)
                {
                    string[] parts = command.Split('"');
                    if (parts.Length > 1)
                    {
                        return parts[1].Trim();
                    }
                }
            }
            return null;
        }

        public static void SendF12Key()
        {
            const ushort VK_F12 = 0x7B;
            var inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_TYPE.KEYBOARD,
                    union = new INPUT.Union
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = VK_F12,
                            dwFlags = 0
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT_TYPE.KEYBOARD,
                    union = new INPUT.Union
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = VK_F12,
                            dwFlags = KEYEVENTF.KEYUP
                        }
                    }
                }
            };
            _ = SendInput(
                (uint)inputs.Length,
                inputs,
                Marshal.SizeOf<INPUT>()
            );
        }



        public class API
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct INPUT
            {
                public INPUT_TYPE type;

                public Union union;

                [StructLayout(LayoutKind.Explicit)]
                public struct Union
                {
                    [FieldOffset(0)] public MOUSEINPUT mi;
                    [FieldOffset(0)] public KEYBDINPUT ki;
                    [FieldOffset(0)] public HARDWAREINPUT hi;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MOUSEINPUT
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public MOUSEEVENTF dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct KEYBDINPUT
            {
                public ushort wVk;
                public ushort wScan;
                public KEYEVENTF dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct HARDWAREINPUT
            {
                public uint uMsg;
                public ushort wParamL;
                public ushort wParamH;
            }

            public enum INPUT_TYPE : uint
            {
                MOUSE = 0,
                KEYBOARD = 1,
                HARDWARE = 2
            }

            [Flags]
            public enum MOUSEEVENTF : uint
            {
                MOVE = 0x0001,
                LEFTDOWN = 0x0002,
                LEFTUP = 0x0004,
                RIGHTDOWN = 0x0008,
                RIGHTUP = 0x0010,
                MIDDLEDOWN = 0x0020,
                MIDDLEUP = 0x0040,
                VIRTUALDESK = 0x4000,
                ABSOLUTE = 0x8000
            }

            [Flags]
            public enum KEYEVENTF : uint
            {
                KEYUP = 0x0002,
                SCANCODE = 0x0008,
                UNICODE = 0x0004
            }

            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint SendInput(
                uint nInputs,
                [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
                int cbSize
            );

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            public const uint PROCESS_QUERY_INFORMATION = 0x0400;

            public const uint PROCESS_VM_READ = 0x0100;

            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool QueryFullProcessImageName(
                IntPtr hProcess,
                uint dwFlags,
                StringBuilder lpExeName,
                ref uint lpdwSize);

            [DllImport("kernel32.dll")]
            public static extern bool CloseHandle(IntPtr hObject);
        }
    }
}
