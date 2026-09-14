using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NVIDIA_HDV
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, GUID, out bool createnew);
            if (createnew)
            {
                ThreadPool.RegisterWaitForSingleObject(ProgramStarted, OnProgramStarted, null, -1, false);
            }
            else
            {
                ProgramStarted.Set();
                Environment.Exit(0);
            }
            Settings settings = new Settings();
            this.MainWindow = settings;
            settings.Show();
        }

        public readonly string GUID = "1046BAC7-A184-41AC-9161-90AA94E299F8";
        private EventWaitHandle ProgramStarted;
        private void OnProgramStarted(object state, bool timeout)
        {
            MessageBox.Show(
                "已有应用程序实例正在运行",
                "NVIDIA_HDV",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.DefaultDesktopOnly);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception_throw(e.ExceptionObject as Exception);
            if (!e.IsTerminating)
            {
                Environment.Exit(0);
            }
        }
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception_throw(e.Exception);
            e.Handled = true;
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Exception_throw(e.Exception);
            e.SetObserved();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Exception_throw(Exception e)
        {
            string estd =
                "[应用程序内部异常]"
                + $"\n\n根命名空间:{e.Source}"
                + $"\n方法体:{e.TargetSite}";
            if (e is AggregateException ae && ae.InnerException != null)
            {
                estd +=
                    $"\nInnerException:{e.InnerException.GetType().Name}"
                    + $"\n    根命名空间:{e.InnerException.Source}"
                    + $"\n    方法体:{e.InnerException.TargetSite}"
                    + $"\n    详细信息:\n        {e.InnerException.Message}"
                    + $"{(Regex.IsMatch(e.InnerException.Message, @"\n\z") ? string.Empty : "\n")}"
                    + "    位置:";
                if (string.IsNullOrEmpty(e.InnerException.StackTrace)
                    || !e.InnerException.StackTrace.Contains("\n"))
                {
                    estd += $"\n        {e.InnerException.StackTrace.Trim()}";
                }
                else
                {
                    foreach (string st in e.InnerException.StackTrace.Split('\n'))
                    {
                        estd += $"\n        {st.Trim()}";
                    }
                }
                estd += "\n\nNVIDIA_HDV - Exceptions Processed By FeiLingshu";
            }
            else
            {
                estd +=
                    $"\n详细信息:{e.GetType().Name}\n    {e.Message}"
                    + $"{(Regex.IsMatch(e.Message, @"\n\z") ? string.Empty : "\n")}"
                    + "位置:";
                if (string.IsNullOrEmpty(e.StackTrace)
                    || !e.StackTrace.Contains("\n"))
                {
                    estd += $"\n    {e.StackTrace.Trim()}";
                }
                else
                {
                    foreach (string st in e.StackTrace.Split('\n'))
                    {
                        estd += $"\n    {st.Trim()}";
                    }
                }
                estd += "\n\nNVIDIA_HDV - Exceptions Processed By FeiLingshu";
            }
            MessageBox.Show(
                estd,
                "NVIDIA_HDV",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.DefaultDesktopOnly);
            Environment.Exit(0);
        }



        public const uint RDW_INVALIDATE = 0x0001;
        public const uint RDW_ALLCHILDREN = 0x0080;
        [DllImport("user32.dll")]
        public extern static bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flag);



        #region Static Resource
        public const string JS = "if(%%SWITCH%%){requestParamaters=requestParamaters.replace(/dch=1/g,\"dch=0\");}else{requestParamaters=requestParamaters.replace(/dch=\\d/g,\"dch=1\");requestParamaters=requestParamaters.replace(/dch=null/g,\"dch=1\");}if(%%GRD%%){requestParamaters=requestParamaters.replace(/isWHQL=\\d/g,\"isWHQL=1\");requestParamaters=requestParamaters.replace(/isWHQL=null/g,\"isWHQL=1\");requestParamaters=requestParamaters.replace(/upCRD=\\d/g,\"upCRD=0\");requestParamaters=requestParamaters.replace(/upCRD=null/g,\"upCRD=0\");}if(%%STD%%){requestParamaters=requestParamaters.replace(/isWHQL=\\d/g,\"isWHQL=0\");requestParamaters=requestParamaters.replace(/isWHQL=null/g,\"isWHQL=0\");requestParamaters=requestParamaters.replace(/upCRD=\\d/g,\"upCRD=1\");requestParamaters=requestParamaters.replace(/upCRD=null/g,\"upCRD=1\");}let replacestd=\"numberOfResults=%%VALUE%%\";let addstd=\"%%ADD%%\";if(addstd!=\"\"){replacestd=addstd+'&'+replacestd;}requestParamaters=requestParamaters.replace(/numberOfResults=\\d+/g,replacestd);console.log(\"NVIDIA_HDV 已替换查询参数 ->\",requestParamaters);";
        // if(%%SWITCH%%){requestParamaters=requestParamaters.replace(/dch=1/g,"dch=0");}else{requestParamaters=requestParamaters.replace(/dch=\d/g,"dch=1");requestParamaters=requestParamaters.replace(/dch=null/g,"dch=1");}if(%%GRD%%){requestParamaters=requestParamaters.replace(/isWHQL=\d/g,"isWHQL=1");requestParamaters=requestParamaters.replace(/isWHQL=null/g,"isWHQL=1");requestParamaters=requestParamaters.replace(/upCRD=\d/g,"upCRD=0");requestParamaters=requestParamaters.replace(/upCRD=null/g,"upCRD=0");}if(%%STD%%){requestParamaters=requestParamaters.replace(/isWHQL=\d/g,"isWHQL=0");requestParamaters=requestParamaters.replace(/isWHQL=null/g,"isWHQL=0");requestParamaters=requestParamaters.replace(/upCRD=\d/g,"upCRD=1");requestParamaters=requestParamaters.replace(/upCRD=null/g,"upCRD=1");}let replacestd="numberOfResults=%%VALUE%%";let addstd="%%ADD%%";if(addstd!=""){replacestd=addstd+'&'+replacestd;}requestParamaters=requestParamaters.replace(/numberOfResults=\d+/g,replacestd);console.log("NVIDIA_HDV 已替换查询参数 ->",requestParamaters);
        #endregion
    }
}
