using Utils.Prism;
using Utils.Extensions;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Media;

namespace Tagger.Wpf.ViewModels
{
    public class SettingsModel : ViewModelBase
    {
        #region Fields

        private string m_Text;
        private string m_ColorName;
        private int m_TopOffset;
        private int m_RightOffset;

        #endregion
        
        public SettingsModel()
        {
            CreateTagCommand = new DelegateCommand<object>(CreateTag, CanCreateTag);
            HideSettingsCommand = new DelegateCommand<object>(HideSettings, CanHideSettings);
            ShowSettingsCommand = new DelegateCommand<object>(ShowSettings, CanShowSettings);
            DeleteTagCommand = new DelegateCommand<object>(DeleteTag, CanDeleteTag);
            CancelSettingsCommand = new DelegateCommand<object>(CancelSettings, CanCancelSettings);
        }

        #region Validation

        /// <summary>
        /// Validation for Text property value
        /// </summary>
        private void Validate_Text()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for ColorName property value
        /// </summary>
        private void Validate_ColorName()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for TopOffset property value
        /// </summary>
        private void Validate_TopOffset()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for RightOffset property value
        /// </summary>
        private void Validate_RightOffset()
        {
            Validate(true, "Is always valid");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Text to render in tag
        /// </summary>
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; OnPropertyChanged(this.Property(() => Text)); }
        }

        /// <summary>
        /// Name of the color to use
        /// </summary>
        public string ColorName
        {
            get { return m_ColorName; }
            set { m_ColorName = value; OnPropertyChanged(this.Property(() => ColorName)); }
        }

        /// <summary>
        /// User specified offset from the top of the host window client area
        /// </summary>
        public int TopOffset
        {
            get { return m_TopOffset; }
            set { m_TopOffset = value; OnPropertyChanged(this.Property(() => TopOffset)); }
        }

        /// <summary>
        /// User specified offset from the right of the host window client area
        /// </summary>
        public int RightOffset
        {
            get { return m_RightOffset; }
            set { m_RightOffset = value; OnPropertyChanged(this.Property(() => RightOffset)); }
        }

        #endregion

        #region Command - CreateTag

        /// <summary>
        /// Create tag window for specified host
        /// </summary>
        public DelegateCommand<object> CreateTagCommand { get; private set; }

        /// <summary>
        /// CreateTag command handler
        /// </summary>
        private void CreateTag(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if CreateTag command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanCreateTag(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - DeleteTag

        /// <summary>
        /// Delete tag from host window
        /// </summary>
        public DelegateCommand<object> DeleteTagCommand { get; private set; }

        /// <summary>
        /// DeleteTag command handler
        /// </summary>
        private void DeleteTag(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if DeleteTag command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanDeleteTag(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - HideSettings

        /// <summary>
        /// Hide settings window command
        /// </summary>
        public DelegateCommand<object> HideSettingsCommand { get; private set; }

        /// <summary>
        /// HideSettings command handler
        /// </summary>
        private void HideSettings(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if HideSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanHideSettings(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - ShowSettings

        /// <summary>
        /// Show settings for a tag
        /// </summary>
        public DelegateCommand<object> ShowSettingsCommand { get; private set; }

        /// <summary>
        /// ShowSettings command handler
        /// </summary>
        private void ShowSettings(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if ShowSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanShowSettings(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - CancelSettings

        /// <summary>
        /// Cancel tag and settings, tagging was a mistake user whant to revert
        /// </summary>
        public DelegateCommand<object> CancelSettingsCommand { get; private set; }

        /// <summary>
        /// CancelSettings command handler
        /// </summary>
        private void CancelSettings(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if CancelSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanCancelSettings(object parameter)
        {
            return true;
        }

        #endregion

        public TagModel GetTagModel()
        {
            return new TagModel
            {
                Text = "File browser",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 50
            };
        }
    }
}
