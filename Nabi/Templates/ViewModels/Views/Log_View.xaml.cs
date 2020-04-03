using ICSharpCode.AvalonEdit;
using MLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nabi.Templates.ViewModels.Views
{
    /// <summary>
    /// Log_View.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Log_View : UserControl
    {
        public static TextEditor log;
        public Log_View()
        {
            InitializeComponent();
            log = LogTE;

            var appearance = ServiceLocator.ServiceContainer.Instance.GetService<IAppearanceManager>();
            if (appearance.ThemeName.Contains("Dark"))
            {
                using (System.IO.Stream s = typeof(Log_View).Assembly.GetManifestResourceStream("Nabi.Resource.white.xshd"))
                {
                    if (s == null)
                        throw new InvalidOperationException("Could not find embedded resource");
                    using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s))
                    {
                        Log_View.log.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                    }
                }
            }
        }

        public static void clearLog()
        {
            log.Clear();
        }

        public static void printLog(string s)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                log.AppendText(s + '\n');
                log.SelectionStart = log.Document.Text.Length;
                log.ScrollToLine(log.CaretOffset);
            }));
        }
    }
}
