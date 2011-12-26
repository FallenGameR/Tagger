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

            DataContext = ViewModel;
            App.Current.Exit += delegate { ViewModel.Dispose(); };
        }

        /// <remarks>
        /// Original http://stackoverflow.com/questions/2136431/how-do-i-read-custom-keyboard-shortcut-from-user-in-wpf
        /// </remarks>
        private void txtShortcutKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Fetch the actual shortcut key
            var key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            // Ignore modifier keys
            if (IsModifierKey(key)) { return; }
            
            // Set view model properties
            ViewModel.ModifierKeys = Keyboard.Modifiers;
            ViewModel.Key = (System.Windows.Forms.Keys) KeyInterop.VirtualKeyFromKey(key);

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
            get { return (DataContext == null) ? new HotkeyViewModel() : (HotkeyViewModel)DataContext; }
        }
    }
}
