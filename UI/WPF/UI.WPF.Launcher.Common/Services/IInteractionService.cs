#region Usings

using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows.Controls;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Launcher.Common.Services
{
    public enum MessageType
    {
        Warning,

        Error,

        Information,

        Question
    }

    public enum QuestionType
    {
        YesNo,

        YesNoCancel
    }

    public enum QuestionAnswer
    {
        Yes,

        No,

        Cancel
    }

    public interface IFileFilter
    {
        string Description { get; }

        string Filter { get; }
    }

    public class FileFilter : IFileFilter
    {
        public FileFilter(string description, string filter)
        {
            this.Description = description;
            this.Filter = filter;
        }

        #region IFileFilter Members

        public string Description { get; set; }

        public string Filter { get; set; }

        #endregion
    }

    public interface ICredentialsState
    {
        void SetValid(bool valid);
    }

    public class QuestionSettings
    {
        public string YesText { get; set; }

        public string NoText { get; set; }

        public string CancelText { get; set; }
    }

    public interface IInteractionService
    {
        Task ShowMessage(MessageType type, string title, string text);

        Task<IEnumerable<string>> OpenFileDialog(string title, bool multiple, IEnumerable<IFileFilter> filters = null);

        Task<string> SaveFileDialog(string title, IEnumerable<IFileFilter> filters = null);

        Task<TResult> ShowDialog<TResult>(IDialogControl<TResult> control);

        Task<string> OpenDirectoryDialog(string title, bool multiple = false);

        Task<QuestionAnswer> ShowQuestion(MessageType type, QuestionType questionType, string title, string text, QuestionSettings settings = null);
    }

    public static class InteractionServiceExtensions
    {
        public static Task<IEnumerable<string>> OpenFileDialog(this IInteractionService This, string title, bool multiple,
            params IFileFilter[] filters)
        {
            return This.OpenFileDialog(title, multiple, filters);
        }

        public static Task<string> SaveFileDialog(this IInteractionService This, string title, params IFileFilter[] filters)
        {
            return This.SaveFileDialog(title, filters);
        }
    }
}
