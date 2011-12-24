using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tagger.Wpf.Views
{
    /// <summary>
    /// Interaction logic for HotkeyControl.xaml
    /// </summary>
    public partial class HotkeyControl : UserControl
    {
        public HotkeyControl()
        {
            InitializeComponent();
        }

        /// <remarks>
        /// Original http://stackoverflow.com/questions/2136431/how-do-i-read-custom-keyboard-shortcut-from-user-in-wpf
        /// </remarks>
        private void txtShortcutKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // The text box grabs all input
            e.Handled = true;

            // Fetch the actual shortcut key
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            // Ignore modifier keys
            if (key == Key.LeftShift || 
                key == Key.RightShift || 
                key == Key.LeftCtrl || 
                key == Key.RightCtrl || 
                key == Key.LeftAlt || 
                key == Key.RightAlt || 
                key == Key.LWin || 
                key == Key.RWin) 
            { 
                return;
            }

            // Build the shortcut key name
            var shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) 
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) 
            {
                shortcutText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0) 
            {
                shortcutText.Append("Alt+");
            }
            shortcutText.Append(key.ToString());

            // Update the text box
            txtShortcutKey.Text = shortcutText.ToString();
        }
    }
}
