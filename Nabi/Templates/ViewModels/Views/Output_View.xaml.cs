using ICSharpCode.AvalonEdit;
using MLib.Interfaces;
using Nabi.ViewModels.Base;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Nabi.Templates.ViewModels.Views
{
    /// <summary>
    /// Output_View.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Output_View : UserControl
    {
        public static TextEditor output;
        public Output_View()
        {
            InitializeComponent();
            output = OutputTE;

            IAppearanceManager appearance = ServiceLocator.ServiceContainer.Instance.GetService<IAppearanceManager>();
            if (appearance.ThemeName.Contains("Dark"))
            {
                using (System.IO.Stream s = typeof(Output_View).Assembly.GetManifestResourceStream("Nabi.Resource.white.xshd"))
                {
                    if (s == null)
                        throw new InvalidOperationException("Could not find embedded resource");
                    using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s))
                    {
                        Output_View.output.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                    }
                }
            }
        }

        public static void printOutput(string s)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                output.AppendText(s + '\n');
                try
                {
                    output.SelectionStart = output.Document.Text.Length;
                    output.ScrollToLine(output.CaretOffset);
                }
                catch (Exception ex) { }
                // error will be caused due to multiprocessing, but i dont know how to handle it. So just bob it.
            }));   
        }

        public static void showError(int pos)
        {
            int lineno = output.Document.GetLineByNumber(pos).LineNumber;
            printOutput((lineno + 1) + "번째 줄에서 에러가 발생했습니다.\n");

            // 에러난 줄 하이라이팅 기능 추가
            // DevPython 참조 393줄
        }

        public static void clearOutput()
        {
            try
            {
                output.Clear();
            }
            catch (NullReferenceException e)
            {
            }
        }
    }
}
