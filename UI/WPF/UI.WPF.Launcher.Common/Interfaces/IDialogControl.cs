using System.Threading.Tasks;
using FSOManagement.Annotations;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IDialogControl<TResult>
    {
        [NotNull]
        Task<TResult> WaitForCompletionAsync();
    }
}