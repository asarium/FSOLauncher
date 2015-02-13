#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Launcher.Implementations
{
    internal class ProgressControllerWrapper : IProgressController
    {
        public ProgressControllerWrapper(ProgressDialogController controller)
        {
            _controller = controller;
        }

        private readonly ProgressDialogController _controller;

        #region IProgressController Members

        public string Title
        {
            set { _controller.SetTitle(value); }
        }

        public string Message
        {
            set { _controller.SetMessage(value); }
        }

        public double Progress
        {
            set { _controller.SetProgress(value); }
        }

        public bool IsCanceled
        {
            get { return _controller.IsCanceled; }
        }

        public bool Cancelable
        {
            set { _controller.SetCancelable(value); }
        }

        public bool Indeterminate
        {
            set { _controller.SetIndeterminate(); }
        }

        public Task CloseAsync()
        {
            return _controller.CloseAsync();
        }

        #endregion
    }

    [Export(typeof(IInteractionService))]
    public class MetroInteractionService : MetroWindowController, IInteractionService
    {
        #region IInteractionService Members

        public async Task ShowMessage(MessageType type, string title, string text)
        {
            await Window.ShowMessageAsync(title, text);
        }

        public async Task<TResult> ShowDialog<TResult>(IDialogControl<TResult> control)
        {
            var metroDialog = control as BaseMetroDialog;
            if (metroDialog == null)
            {
                throw new InvalidOperationException("Control is not a metro dialog!");
            }

            await Window.ShowMetroDialogAsync(metroDialog);

            var result = await control.WaitForCompletionAsync();

            await Window.HideMetroDialogAsync(metroDialog);

            return result;
        }

        public Task<string> OpenDirectoryDialog(string title, bool multiple = false)
        {
            var tcs = new TaskCompletionSource<string>();

            Execute(() =>
            {
                var dlg = new VistaFolderBrowserDialog
                {
                    Description = title
                };

                var b = dlg.ShowDialog(Window);

                if (b.HasValue && b.Value)
                {
                    tcs.TrySetResult(dlg.SelectedPath);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            });

            return tcs.Task;
        }

        public Task<IEnumerable<string>> OpenFileDialog(string title, bool multiple, IEnumerable<IFileFilter> filters = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFileDialog(string title, IEnumerable<IFileFilter> filters = null)
        {
            throw new NotImplementedException();
        }

        public async Task<QuestionAnswer> ShowQuestion(MessageType type,
            QuestionType questionType,
            string title,
            string text,
            QuestionSettings settings = null)
        {
            MessageDialogStyle style;
            switch (questionType)
            {
                case QuestionType.YesNo:
                    style = MessageDialogStyle.AffirmativeAndNegative;
                    break;
                case QuestionType.YesNoCancel:
                    style = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("questionType");
            }

            var dialogSettings = new MetroDialogSettings();

            if (settings != null)
            {
                if (settings.CancelText != null)
                {
                    dialogSettings.NegativeButtonText = settings.CancelText;
                }

                if (settings.YesText != null)
                {
                    dialogSettings.AffirmativeButtonText = settings.YesText;
                }

                if (settings.CancelText != null)
                {
                    dialogSettings.FirstAuxiliaryButtonText = settings.CancelText;
                }
            }

            var result = await Window.ShowMessageAsync(title, text, style, dialogSettings);

            switch (result)
            {
                case MessageDialogResult.Negative:
                    return QuestionAnswer.No;
                case MessageDialogResult.Affirmative:
                    return QuestionAnswer.Yes;
                case MessageDialogResult.FirstAuxiliary:
                    return QuestionAnswer.Cancel;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<IProgressController> OpenProgressDialogAsync(string title, string message)
        {
            return new ProgressControllerWrapper(await Window.ShowProgressAsync(title, message));
        }

        #endregion

        private Task Execute(Action action)
        {
            if (Window == null || Window.Dispatcher.CheckAccess())
            {
                action();

                return Task.Delay(0);
            }
            else
            {
                return Window.Dispatcher.InvokeAsync(action).Task;
            }
        }

        private Task Execute(Func<Task> action)
        {
            if (Window == null || Window.Dispatcher.CheckAccess())
            {
                return action();
            }
            else
            {
                return Window.Dispatcher.InvokeAsync(action).Task;
            }
        }

        private Task<TResult> Execute<TResult>(Func<TResult> action)
        {
            if (Window == null || Window.Dispatcher.CheckAccess())
            {
                return Task.FromResult(action());
            }
            else
            {
                return Window.Dispatcher.InvokeAsync(action).Task;
            }
        }
    }
}
