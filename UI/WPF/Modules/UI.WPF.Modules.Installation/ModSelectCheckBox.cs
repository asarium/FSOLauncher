#region Usings

using System.Windows.Controls;

#endregion

namespace UI.WPF.Modules.Installation
{
    public class ModSelectCheckBox : CheckBox
    {
        #region Overrides of ToggleButton

        protected override void OnToggle()
        {
            IsChecked = IsChecked != true;
        }

        #endregion
    }
}
