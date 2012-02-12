using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using Utils.Diagnostics;
using Utils.Extensions;

namespace Utils.Prism
{
    /// <summary>
    /// Base class for all ViewModels. Features:
    /// - safe proparty changes notifications 
    /// - property values conventions-based validation
    /// </summary>
    /// <example>
    /// [Property changed notification]
    /// public string SomeProperty
    /// { 
    ///     get { return m_SomeProperty; }
    ///     set
    ///     {
    ///         m_SomeProperty = value;
    ///         OnPropertyChanged( this.Property( ( ) => SomeProperty ) );
    ///     }
    /// }
    /// [Validation notification]
    /// private void Validate_SomeProperty()
    /// {
    ///     Validate( SomeProperty >= MinValue, "Property must be greater or equal to min value" );
    ///     Validate( MaxValue >= SomeProperty, "Property must be less or equal to max value" );
    /// }
    /// </example>
    /// <remarks>
    /// See http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
    /// </remarks>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        #region Constants

        /// <summary>
        /// Method prefix to use for validator methods.
        /// Convention used is: {ValidateMethodPrefix}{PropertyName}
        /// </summary>
        private static readonly string ValidateMethodPrefix = "Validate_";

        #endregion

        #region Constructor

        public ViewModelBase()
        {
            Errors = new Dictionary<string, IList<string>>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add an validation error to detected errors dictionary
        /// </summary>
        /// <param name="property">Validated property name</param>
        /// <param name="info">Error details</param>
        protected void AddValidationError(string property, string info)
        {
            if (!Errors.ContainsKey(property))
            {
                Errors[property] = new List<string>();
            }
            Errors[property].Add(info);
            OnErrorCollectionChanged();
        }

        /// <summary>
        /// Remove all validation errors for a particular property
        /// </summary>
        /// <param name="property">Property name</param>
        protected void RemoveAllValidationErrors(string property)
        {
            if (Errors.ContainsKey(property))
            {
                Errors.Remove(property);
                OnErrorCollectionChanged();
            }
        }

        /// <summary>
        /// Trigger for changed validation error collection event
        /// </summary>
        /// <remarks>
        /// Usually children classes recalculate all known Command.CanExecute here
        /// </remarks>
        protected virtual void OnErrorCollectionChanged()
        {
            OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Trigger for property changed event
        /// </summary>
        /// <param name="propertyName">Name of the changed property</param>
        protected void OnPropertyChanged(string propertyName)
        {
            CheckPropertyExist(propertyName);
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Trigger for changed conditions of any hosted command
        /// </summary>
        protected void OnDelegateCommandsCanExecuteChanged()
        {
            var commands = 
                from info in GetType().GetProperties()
                where info.PropertyType == typeof(DelegateCommand<object>)
                select (DelegateCommand<object>) info.GetValue(this, null);

            commands.Action( c => c.RaiseCanExecuteChanged() );
        }

        /// <summary>
        /// Making sure that property with such name exist
        /// </summary>
        /// <remarks>
        /// Helpfull for detecting binding typos and property renamings in XAML
        /// </remarks>
        /// <param name="propertyName">Property name</param>
        [DebuggerStepThrough]
        protected void CheckPropertyExist(string propertyName)
        {
            var property = TypeDescriptor.GetProperties(this)[propertyName];

            Check.Require(property != null, string.Format(
                "Current ViewModel doesn't have {0} property. " +
                "Most likely you've renamed a property name and haven't updated corresponding binding. " +
                "Please specify correct property name in the binding.",
                propertyName));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// True if disposed method was already called
        /// </summary>
        private bool m_WasDisposed = false;

        /// <summary>
        /// Finilizer that would be called if Dispose method hasn't been called
        /// </summary>
        /// <remarks>
        /// Do not redifine in descendants
        /// </remarks>
        ~ViewModelBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Implementing MS recommended pattern for IDisposable
        /// </summary>
        /// <remarks>
        /// Do not redifine in descendants
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        /// <param name="wasCalledFromDisposeMethod">true is was called from IDisposable.Dispose</param>
        private void Dispose(bool wasCalledFromDisposeMethod)
        {
            // Cleanup must happen only once
            if (m_WasDisposed) return;
            m_WasDisposed = true;

            // We can dispose managed resources only if we were called from Dispose, not from the finilizer
            if (wasCalledFromDisposeMethod)
            {
                OnDisposeManaged();
            }

            // We can dispose unmanaged resources both from finilizer and Dispose
            OnDisposeUnmanaged();

#if DEBUG
            // Log dispose call in debug output
            Debug.WriteLine(string.Format(
                "{0} ({1}, {2}) {3}",
                DisplayName ?? "ViewModel",
                GetType().Name,
                GetHashCode(),
                wasCalledFromDisposeMethod ? "Disposed" : "Finalized"));
#endif
        }

        /// <summary>
        /// Cleanup all managed resources (e.g. event handlers)
        /// </summary>
        /// <remarks>
        /// Override in descendants
        /// </remarks>
        protected virtual void OnDisposeManaged()
        {
        }

        /// <summary>
        /// Cleanup all unmanaged resources
        /// </summary>
        /// <remarks>
        /// Override in descendants
        /// </remarks>
        protected virtual void OnDisposeUnmanaged()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event that fired when a property value is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        #region IDataErrorInfo Members

        /// <summary>
        /// Error message used in BindingGroup
        /// (not supported)
        /// </summary>
        public string Error
        {
            get { return null; }
        }

        /// <summary>
        /// Errors belonging to particular properties
        /// </summary>
        public string this[string name]
        {
            get
            {
                ValidateProperty(name);

                var builder = new StringBuilder();
                if (Errors.ContainsKey(name))
                {
                    foreach (var error in Errors[name])
                    {
                        builder.AppendLine(error);
                    }
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Validate property with specified name
        /// </summary>
        /// <remarks>
        /// Validation methods use {ValidateMethodPrefix}{PropertyName} convention.
        /// </remarks>
        private void ValidateProperty(string propertyName)
        {
            CheckPropertyExist(propertyName);

            var validator = GetType().GetMethod(ValidateMethodPrefix + propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (validator != null)
            {
                RemoveAllValidationErrors(propertyName);
                validator.Invoke(this, null);
            }
        }

        /// <summary>
        /// Check validation statement for a property
        /// </summary>
        /// <param name="assert">Validation statement</param>
        /// <param name="errorInfo">Validation statement description</param>
        /// <remarks>
        /// This method could only be called from methods starting with {ValidateMethodPrefix}
        /// Checked property name is calculated from the caller method name.
        /// </remarks>
        protected void Validate(bool assert, string errorInfo)
        {
            var callerName = new StackTrace().GetFrame(1).GetMethod().Name;
            var property = callerName.Substring(ValidateMethodPrefix.Length);

            if (!assert)
            {
                AddValidationError(property, errorInfo);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Validation errors for each property.
        /// Data structure: Property name -> Error List
        /// </summary>
        public IDictionary<string, IList<string>> Errors { get; private set; }

        #endregion
    }
}
