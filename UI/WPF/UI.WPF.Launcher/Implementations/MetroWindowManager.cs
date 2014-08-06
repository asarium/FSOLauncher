#region Usings

#region Usings

using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls;

#endregion

#endregion

// Copied from https://github.com/ziyasal/Caliburn.Metro

namespace UI.WPF.Launcher.Implementations
{
    [Export(typeof(IWindowManager))]
    public sealed class MetroWindowManager : WindowManager
    {
        private MetroWindow Window { get; set; }

        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            MetroWindow window = null;
            Window inferOwnerOf;
            if (view is MetroWindow)
            {
                window = CreateCustomWindow(view, true);
                inferOwnerOf = InferOwnerOf(window);
                if (inferOwnerOf != null && isDialog)
                {
                    window.Owner = inferOwnerOf;
                }
            }

            if (window == null)
            {
                window = CreateCustomWindow(view, false);
            }

            window.SetValue(View.IsGeneratedProperty, true);
            inferOwnerOf = InferOwnerOf(window);
            if (inferOwnerOf != null)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                inferOwnerOf.Show();

                window.Owner = inferOwnerOf;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            return window;
        }

        private static MetroWindow CreateCustomWindow(object view, bool windowIsView)
        {
            MetroWindow result;
            if (windowIsView)
            {
                result = view as MetroWindow;
            }
            else
            {
                result = new MetroWindow {Content = view};
            }

            return result;
        }
    }
}
