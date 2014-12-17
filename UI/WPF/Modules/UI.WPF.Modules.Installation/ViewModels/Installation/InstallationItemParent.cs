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
            Children.Select(x => x.ResultObservable).CombineLatest().Select(ResultSelector).BindTo(this, x => x.Result);

            OperationMessage = null;
        }

        public IEnumerable<InstallationItem> Children { get; private set; }

        private static InstallationResult ResultSelector(IList<InstallationResult> children)
        {
            if (children.Any(x => x == InstallationResult.Failed))
            {
                return InstallationResult.Failed;
            }

            if (children.All(x => x == InstallationResult.Successful))
            {
                return InstallationResult.Successful;
            }

            if (children.Any(x => x == InstallationResult.Canceled))
            {
                return InstallationResult.Canceled;
            }

            return InstallationResult.Pending;
        }

        #region Overrides of InstallationItem

        public override async Task Install()
        {
            using (CancellationTokenSource = new CancellationTokenSource())
            {
                CancellationTokenSource.Token.Register(() =>
                {
                    foreach (var installationItem in Children)
                    {
                        if (installationItem.CancellationTokenSource != null && !installationItem.CancellationTokenSource.IsCancellationRequested)
                        {
                            installationItem.CancellationTokenSource.Cancel();
                        }
                    }
                });

                await Task.WhenAll(Children.Select(x => x.Install()));
            }
        }

        #endregion
    }
}
