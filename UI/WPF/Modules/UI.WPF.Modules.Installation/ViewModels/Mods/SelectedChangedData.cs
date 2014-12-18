namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public struct SelectedChangedData
    {
        public SelectedChangedData(PackageViewModel viewModel, bool selected) : this()
        {
            ViewModel = viewModel;
            Selected = selected;
        }

        public PackageViewModel ViewModel { get; private set; }

        public bool Selected { get; private set; }
    }
}
