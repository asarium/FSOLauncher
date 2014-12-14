#region Usings

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public class InstallationItemParent : InstallationItem
    {
        public InstallationItemParent(string title, IEnumerable<InstallationItem> children)
        {
            Title = title;
            Children = children.ToList();

            Children.Select(x => x.ProgressObservable).CombineLatest().Select(list => list.Average()).BindTo(this, x => x.Progress);
            Children.Select(x => x.IndeterminateObservable).CombineLatest().Select(list => list.All(b => b)).BindTo(this, x => x.Indeterminate);

            OperationMessage = null;
            CancellationTokenSource = new CancellationTokenSource();
            CancellationTokenSource.Token.Register(() =>
            {
                foreach (var installationItem in Children.Where(installationItem => !installationItem.CancellationTokenSource.IsCancellationRequested)
                    )
                {
                    installationItem.CancellationTokenSource.Cancel();
                }
            });
        }

        public IEnumerable<InstallationItem> Children { get; private set; }

        #region Overrides of InstallationItem

        public override Task Install()
        {
            return Task.WhenAll(Children.Select(x => x.Install()));
        }

        #endregion
    }
}
