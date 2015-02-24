#region Usings

using System;
using System.Collections.Generic;
using System.Windows.Input;
using FSOManagement.Annotations;
using ReactiveUI;
using UI.WPF.Modules.Update.Views;

#endregion

namespace UI.WPF.Modules.Update.ViewModels
{
    public class ChangeLogStatus
    {
        public ChangeLogStatus([NotNull] IEnumerable<KeyValuePair<Version, string>> releaseNotes, [NotNull] IInteractionService interactionService)
        {
            OpenChangeLogCommand =
                ReactiveCommand.CreateAsyncTask(
                    async _ => await interactionService.ShowDialog(new ChangelogDialog(releaseNotes)).ConfigureAwait(false));
        }

        [NotNull]
        public ICommand OpenChangeLogCommand { get; private set; }
    }
}
