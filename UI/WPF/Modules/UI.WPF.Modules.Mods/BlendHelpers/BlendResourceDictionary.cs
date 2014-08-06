#region Usings

using System;
using System.ComponentModel;
using System.Windows;

#endregion

namespace UI.WPF.Modules.Mods.BlendHelpers
{
    public class BlendResourceDictionary : ResourceDictionary
    {
        public static bool IsInDesignMode
        {
            get
            {
                return
                    (bool)
                        DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(DependencyObject))
                            .Metadata.DefaultValue;
            }
        }

        public new Uri Source
        {
            get { return base.Source; }
            set
            {
                if (!IsInDesignMode)
                {
                    return;
                }

                base.Source = value;
            }
        }
    }
}
