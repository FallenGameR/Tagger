using System.Windows.Controls;
using System.Windows.Input;
using Tagger.ViewModels;

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

            this.DataContext = this.ViewModel;
            App.Current.Exit += delegate { this.ViewModel.Dispose(); };
        }

        /// <remarks>
        /// Original http://stackoverflow.com/questions/2136431/how-do-i-read-custom-keyboard-shortcut-from-user-in-wpf
        /// </remarks>
        /// <remarks>
        /// Buggy output on Ctrl+Shift+Z
        /// </remarks>
        private void txtShortcutKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Fetch the actual shortcut key
            var key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            // Ignore modifier keys
            if (IsModifierKey(key)) { return; }
            
            // Set view model properties
            this.ViewModel.ModifierKeys = Keyboard.Modifiers;
            this.ViewModel.Key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);

            // The text box grabs all input
            e.Handled = true;
        }

        private static bool IsModifierKey(Key key)
        {
            return key == Key.LeftShift 
                || key == Key.RightShift 
                || key == Key.LeftCtrl 
                || key == Key.RightCtrl 
                || key == Key.LeftAlt 
                || key == Key.RightAlt 
                || key == Key.LWin 
                || key == Key.RWin;
        }

        private HotkeyViewModel ViewModel
        {
            get { return (this.DataContext == null) ? new HotkeyViewModel() : (HotkeyViewModel)DataContext; }
        }
    }
}
