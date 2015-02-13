#region Usings

using System.Collections.Generic;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;

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

    public interface IProgressController
    {
        string Title { set; }

        string Message { set; }

        double Progress { set; }

        bool IsCanceled { get; }

        bool Cancelable { set; }

        bool Indeterminate { set; }

        [NotNull]
        Task CloseAsync();
    }
}

public interface IInteractionService
{
    [NotNull]
    Task ShowMessage(MessageType type, [NotNull] string title, [NotNull] string text);

    [NotNull]
    Task<IEnumerable<string>> OpenFileDialog([NotNull] string title, bool multiple, [CanBeNull] IEnumerable<IFileFilter> filters = null);

    [NotNull]
    Task<string> SaveFileDialog([NotNull] string title, [CanBeNull] IEnumerable<IFileFilter> filters = null);

    [NotNull]
    Task<TResult> ShowDialog<TResult>([NotNull] IDialogControl<TResult> control);

    [NotNull]
    Task<string> OpenDirectoryDialog([NotNull] string title, bool multiple = false);

    [NotNull]
    Task<QuestionAnswer> ShowQuestion(MessageType type,
        QuestionType questionType,
        [NotNull] string title,
        [NotNull] string text,
        [CanBeNull] QuestionSettings settings = null);

    [NotNull]
    Task<IProgressController> OpenProgressDialogAsync([NotNull] string title, [NotNull] string message);
}

public static class InteractionServiceExtensions
{
    public static Task<IEnumerable<string>> OpenFileDialog(this IInteractionService This, string title, bool multiple, params IFileFilter[] filters)
    {
        return This.OpenFileDialog(title, multiple, filters);
    }

    public static Task<string> SaveFileDialog(this IInteractionService This, string title, params IFileFilter[] filters)
    {
        return This.SaveFileDialog(title, filters);
    }
}
