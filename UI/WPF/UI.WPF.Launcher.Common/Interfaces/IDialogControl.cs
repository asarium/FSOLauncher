using System.Threading.Tasks;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IDialogControl<TResult>
    {
        Task<TResult> WaitForCompletionAsync();
    }
}