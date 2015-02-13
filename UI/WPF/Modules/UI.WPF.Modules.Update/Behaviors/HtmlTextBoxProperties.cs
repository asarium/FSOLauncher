#region Usings

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using HtmlToXamlConvert;

#endregion

namespace UI.WPF.Modules.Update.Behaviors
{
    public static class HtmlTextBoxProperties
    {
        public static readonly DependencyProperty HtmlTextProperty = DependencyProperty.RegisterAttached("HtmlText",
            typeof(string),
            typeof(HtmlTextBoxProperties),
            new UIPropertyMetadata("", OnHtmlTextChanged));

        public static string GetHtmlText(TextBlock wb)
        {
            return wb.GetValue(HtmlTextProperty) as string;
        }

        public static void SetHtmlText(TextBlock wb, string html)
        {
            wb.SetValue(HtmlTextProperty, html);
        }

        private static void OnHtmlTextChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            // Go ahead and return out if we set the property on something other than a textblock, or set a value that is not a string. 
            var txtBox = depObj as TextBlock;
            if (txtBox == null)
            {
                return;
            }
            if (!(e.NewValue is string))
            {
                return;
            }
            var html = e.NewValue as string;
            InlineCollection xamLines;
            try
            {
                var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, false);
                xamLines = ((Paragraph) ((Section) XamlReader.Parse(xaml)).Blocks.FirstBlock).Inlines;
            }
            catch
            {
                // There was a problem parsing the html, return out. 
                return;
            }
            // Create a copy of the Inlines and add them to the TextBlock. 
            var newLines = new Inline[xamLines.Count];
            xamLines.CopyTo(newLines, 0);
            txtBox.Inlines.Clear();
            foreach (var l in newLines)
            {
                txtBox.Inlines.Add(l);
            }
        }
    }
}
