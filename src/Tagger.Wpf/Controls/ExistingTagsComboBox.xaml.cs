using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tagger.Controls
{
    /// <summary>
    /// Interaction logic for ExistingTagsComboBox.xaml
    /// </summary>
    public partial class ExistingTagsComboBox : UserControl
    {
        public ExistingTagsComboBox()
        {
            InitializeComponent();        
            
            // Setup existing tags combobox behaviour
            this.comboBox.DropDownOpened += delegate
            {
                this.comboBox.ItemsSource = RegistrationManager.GetExistingTags().ToList();
            };

            this.comboBox.DropDownClosed += delegate
            {
                this.comboBox.SelectedValue = null;
                this.comboBox.ItemsSource = null;
            };

            this.comboBox.PreviewKeyDown += (sender, args) =>
            {
                var isSpace = args.Key == Key.Space;
                var isDown = (args.Key == Key.Down) || (args.Key == Key.PageDown);
                var isOpened = this.comboBox.IsDropDownOpen;

                if (isSpace || (!isOpened && isDown))
                {
                    this.comboBox.IsDropDownOpen = !this.comboBox.IsDropDownOpen;
                }
            };

            this.comboBox.SelectionChanged += delegate
            {
                var tagLabel = (TagLabel)this.comboBox.SelectedValue;
                this.TagViewModel.Text = tagLabel.Text;
                this.TagViewModel.Color = tagLabel.Color;
            };
        }

        private TagViewModel TagViewModel
        {
            get { return (TagViewModel)this.DataContext; }
        }
    }
}
