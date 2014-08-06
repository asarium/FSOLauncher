namespace UI.WPF.Modules.Update.ViewModels
{
    public class SuccessfullStatus
    {
        public SuccessfullStatus(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}
