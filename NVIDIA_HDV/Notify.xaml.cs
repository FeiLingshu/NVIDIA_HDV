using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NVIDIA_HDV
{
    /// <summary>
    /// Notify.xaml 的交互逻辑
    /// </summary>
    public partial class Notify : Window
    {
        public Notify(Window owner, string infos)
        {
            InitializeComponent();
            this.Owner = owner;
            this.InfoStd.Text = infos;
            if (owner != null)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = owner.Left + owner.Width - this.Width - 10;
                this.Top = owner.Top + 30 + 10;
            }
            this.MouseLeftButtonDown += (s, e) => MLBD = e.OriginalSource;
            this.MouseRightButtonDown += (s, e) => MRBD = e.OriginalSource;
            this.CLOSE.PreviewMouseDown += (s, e) =>
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
            };
            this.CLOSE.PreviewMouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && MLBD == e.OriginalSource) this.Close();
                e.Handled = true;
            };
            this.ContentRendered += (s, e) =>
            {
                App.RedrawWindow(
                    new WindowInteropHelper(this).Handle,
                    IntPtr.Zero, IntPtr.Zero,
                    App.RDW_INVALIDATE | App.RDW_ALLCHILDREN);
            };
        }

        private object MLBD = null;
        private object MRBD = null;
    }
}
