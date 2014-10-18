#region Usings

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FSOManagement.Annotations;

#endregion

namespace UI.WPF.Modules.Mods.Views
{
    /// <summary>
    ///     Interaction logic for ModImageDisplayControl.xaml
    /// </summary>
    public partial class ModImageDisplayControl : UserControl
    {
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register("ImageSource",
            typeof(ImageSource),
            typeof(ModImageDisplayControl),
            new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty LoadingImageProperty = DependencyProperty.Register("LoadingImage",
            typeof(bool),
            typeof(ModImageDisplayControl),
            new PropertyMetadata(default(bool)));

        public ModImageDisplayControl()
        {
            this.InitializeComponent();
        }

        public bool LoadingImage
        {
            get { return (bool) GetValue(LoadingImageProperty); }
            set { SetValue(LoadingImageProperty, value); }
        }

        [CanBeNull]
        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }
    }
}
