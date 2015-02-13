#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using FSOManagement.Annotations;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Update.ViewModels;

#endregion

namespace UI.WPF.Modules.Update.Views
{
    /// <summary>
    ///     Interaction logic for ChangelogDialog.xaml
    /// </summary>
    public partial class ChangelogDialog : IDialogControl<object>
    {
        private readonly TaskCompletionSource<object> _tcs;

        public ChangelogDialog([NotNull] IEnumerable<KeyValuePair<Version, string>> changeLog)
        {
            InitializeComponent();

            _tcs = new TaskCompletionSource<object>();

            DataContext = new ChangeLogViewModel(GetChangeLogEnum(changeLog));
        }

        #region IDialogControl<object> Members

        public Task<object> WaitForCompletionAsync()
        {
            return _tcs.Task;
        }

        #endregion

        [NotNull]
        private static IEnumerable<ChangeLogElement> GetChangeLogEnum([NotNull] IEnumerable<KeyValuePair<Version, string>> changeLog)
        {
            return changeLog.OrderByDescending(p => p.Key).Select(p => new ChangeLogElement
            {
                Version = p.Key,
                Content = GetHtmlContent(p.Value)
            }).ToList();
        }

        [NotNull]
        private static string GetHtmlContent([NotNull] string value)
        {
            var doc = XDocument.Load(new StringReader("<xml>" + value + "</xml>"));
            var cdata = doc.DescendantNodes().OfType<XCData>().FirstOrDefault();

            return cdata == null ? "" : cdata.Value;
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            _tcs.TrySetResult(null);
        }
    }
}
