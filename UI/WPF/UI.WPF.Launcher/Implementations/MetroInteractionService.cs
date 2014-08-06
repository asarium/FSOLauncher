#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Launcher.Implementations
{
    [Export(typeof(IInteractionService))]
    public class MetroInteractionService : IInteractionService
    {
        private static MetroWindow Window
        {
            get
            {
                if (Application.Current.MainWindow == null)
                {
                    return null;
                }

                var metroWindow = Application.Current.MainWindow as MetroWindow;

                if (metroWindow != null)
                {
                    return metroWindow;
                }

                throw new InvalidOperationException("ApplicationMain window is not a metro window!");
            }
        }

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
                var dlg = new VistaFolderBrowserDialog {Description = title};

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

        public Task<QuestionAnswer> ShowQuestion(MessageType type, QuestionType questionType, string title, string text,
            QuestionSettings settings = null)
        {
            throw new NotImplementedException();
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
