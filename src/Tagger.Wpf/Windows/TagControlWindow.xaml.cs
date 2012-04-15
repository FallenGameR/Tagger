using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Tagger.Wpf.Windows
{
    /// <summary>
    /// Settings window that sets up how tag looks like
    /// </summary>
    /// <remarks>
    /// Only view-related code here
    /// </remarks>
    public partial class TagControlWindow : Window
    {
        /// <summary>
        /// Color names resource
        /// </summary>
        /// <remarks>
        /// Is not moved to XAML since in C# it's way more readable to call Select from LINQ
        /// </remarks>
        public static string[] ColorNames = typeof(Colors).GetProperties().Select( pi => pi.Name).ToArray();

        /// <summary>
        /// Initializes settings window
        /// </summary>
        public TagControlWindow()
        {
            InitializeComponent();

            // Set focus and select all tag text on settings dialog shown
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler( (sender, e) =>
            {
                if ((bool)e.NewValue)
                {
                    this.SelectTagText();
                }
            });
        }

        /// <summary>
        /// Workaround for late data binding for the settings dialog shown for the first time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextTxt_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            this.SelectTagText();
        }

        /// <summary>
        /// Focus and select all text of the tag to make it easily editable
        /// </summary>
        private void SelectTagText()
        {
            this.TextTxt.Focus();
            this.TextTxt.SelectAll();
        }
    }
}
