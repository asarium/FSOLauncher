namespace UI.WPF.Modules.Update.ViewModels
{
    public class ErrorStatus
    {
        public string Message
        {
            get;
            private set;
        }

        public ErrorStatus(string message)
        {
            Message = message;
        }
    }
}