#region Usings

using System.Windows;
using System.Windows.Controls;

#endregion

namespace UI.WPF.Modules.Mods.Views
{
    /// <summary>
    ///     Interaction logic for ModImageDisplayControl.xaml
    /// </summary>
    public partial class ModImageDisplayControl : UserControl
    {
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register("ImagePath", typeof(string),
            typeof(ModImageDisplayControl), new PropertyMetadata(default(string)));

        public ModImageDisplayControl()
        {
            this.InitializeComponent();
        }

        public string ImagePath
        {
            get { return (string) GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }
    }
}
