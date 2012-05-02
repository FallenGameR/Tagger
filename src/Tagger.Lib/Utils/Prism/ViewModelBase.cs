//-----------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Prism
{
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

    /// <summary>
    /// Base class for all ViewModels. Features:
    /// - safe proparty changes notifications 
    /// - property values conventions-based validation
    /// </summary>
    /// <example>
    /// [Property changed notification]
    /// public string SomeProperty
    /// { 
    /// get { return m_SomeProperty; }
    /// set
    /// {
    /// m_SomeProperty = value;
    /// OnPropertyChanged( this.Property( ( ) =&gt; SomeProperty ) );
    /// }
    /// }
    /// [Validation notification]
    /// private void Validate_SomeProperty()
    /// {
    /// Validate( SomeProperty &gt;= MinValue, "Property must be greater or equal to min value" );
    /// Validate( MaxValue &gt;= SomeProperty, "Property must be less or equal to max value" );
    /// }
    /// </example>
    /// <remarks>
    /// See http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
    /// </remarks>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        /// <summary>
        /// Method prefix to use for validator methods.
        /// Convention used is: {ValidateMethodPrefix}{PropertyName}
        /// </summary>
        private const string ValidateMethodPrefix = "Validate_";

        /// <summary>
        /// True if disposed method was already called
        /// </summary>
        private bool wasDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref = "ViewModelBase" /> class.
        /// </summary>
        protected ViewModelBase()
        {
            this.Errors = new Dictionary<string, IList<string>>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        /// <remarks>
        /// Do not redifine in descendants
        /// </remarks>
        ~ViewModelBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event that fired when a property value is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Gets or sets user-friendly name for current view model object.
        /// </summary>
        /// <remarks>
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </remarks>
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Gets error message used in BindingGroup
        /// </summary>
        /// <remarks>
        /// (not supported)
        /// </remarks>
        public string Error
        {
            get { return null; }
        }

        /// <summary>
        /// Gets validation errors for each property.
        /// </summary>
        /// <remarks>
        /// Data structure: Property name -> Error List
        /// </remarks>
        public IDictionary<string, IList<string>> Errors { get; private set; }

        /// <summary>
        /// Errors belonging to particular properties
        /// </summary>
        /// <param name="name">Name of the checked property</param>
        /// <returns>Error corresponding to the property</returns>
        public string this[string name]
        {
            get
            {
                this.ValidateProperty(name);

                var builder = new StringBuilder();
                if (this.Errors.ContainsKey(name))
                {
                    foreach (string error in this.Errors[name])
                    {
                        builder.AppendLine(error);
                    }
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Implementing MS recommended pattern for IDisposable
        /// </summary>
        /// <remarks>
        /// Do not redifine in descendants
        /// </remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Add an validation error to detected errors dictionary
        /// </summary>
        /// <param name="property">
        /// Validated property name
        /// </param>
        /// <param name="info">
        /// Error details
        /// </param>
        protected void AddValidationError(string property, string info)
        {
            if (!this.Errors.ContainsKey(property))
            {
                this.Errors[property] = new List<string>();
            }

            this.Errors[property].Add(info);
            this.OnErrorCollectionChanged();
        }

        /// <summary>
        /// Making sure that property with such name exist
        /// </summary>
        /// <remarks>
        /// Helpfull for detecting binding typos and property renamings in XAML
        /// </remarks>
        /// <param name="propertyName">
        /// Property name
        /// </param>
        [DebuggerStepThrough]
        protected void CheckPropertyExist(string propertyName)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(this)[propertyName];

            var info = "Current ViewModel doesn't have {0} property. "
                       + "Most likely you've renamed a property name and haven't updated corresponding binding. "
                       + "Please specify correct property name in the binding.";

            Check.Require(property != null, string.Format(info, propertyName));
        }

        /// <summary>
        /// Trigger for changed conditions of any hosted command
        /// </summary>
        protected void OnDelegateCommandsCanExecuteChanged()
        {
            var commands = 
                from info in this.GetType().GetProperties()
                where info.PropertyType == typeof(DelegateCommand<object>)
                select (DelegateCommand<object>)info.GetValue(this, null);

            commands.Action(c => c.RaiseCanExecuteChanged());
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

        /// <summary>
        /// Trigger for changed validation error collection event
        /// </summary>
        /// <remarks>
        /// Usually children classes recalculate all known Command.CanExecute here
        /// </remarks>
        protected virtual void OnErrorCollectionChanged()
        {
            this.OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Trigger for property changed event
        /// </summary>
        /// <param name="propertyName">
        /// Name of the changed property
        /// </param>
        protected void OnPropertyChanged(string propertyName)
        {
            this.CheckPropertyExist(propertyName);
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Remove all validation errors for a particular property
        /// </summary>
        /// <param name="property">
        /// Property name
        /// </param>
        protected void RemoveAllValidationErrors(string property)
        {
            if (this.Errors.ContainsKey(property))
            {
                this.Errors.Remove(property);
                this.OnErrorCollectionChanged();
            }
        }

        /// <summary>
        /// Check validation statement for a property
        /// </summary>
        /// <param name="assert">
        /// Validation statement
        /// </param>
        /// <param name="errorInfo">
        /// Validation statement description
        /// </param>
        /// <remarks>
        /// This method could only be called from methods starting with {ValidateMethodPrefix}
        /// Checked property name is calculated from the caller method name.
        /// </remarks>
        protected void Validate(bool assert, string errorInfo)
        {
            string callerName = new StackTrace().GetFrame(1).GetMethod().Name;
            string property = callerName.Substring(ValidateMethodPrefix.Length);

            if (!assert)
            {
                this.AddValidationError(property, errorInfo);
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        /// <param name="wasCalledFromDisposeMethod">
        /// true is was called from IDisposable.Dispose
        /// </param>
        private void Dispose(bool wasCalledFromDisposeMethod)
        {
            // Cleanup must happen only once
            if (this.wasDisposed)
            {
                return;
            }

            this.wasDisposed = true;

            // We can dispose managed resources only if we were called from Dispose, not from the finilizer
            if (wasCalledFromDisposeMethod)
            {
                this.OnDisposeManaged();
            }

            // We can dispose unmanaged resources both from finilizer and Dispose
            this.OnDisposeUnmanaged();

#if DEBUG
            // Log dispose call in debug output
            Debug.WriteLine(string.Format(
                "{0} ({1}, {2}) {3}", 
                this.DisplayName ?? "ViewModel", 
                GetType().Name, 
                GetHashCode(), 
                wasCalledFromDisposeMethod ? "Disposed" : "Finalized"));
#endif
        }

        /// <summary>
        /// Validate property with specified name
        /// </summary>
        /// <param name="propertyName">
        /// The property Name.
        /// </param>
        /// <remarks>
        /// Validation methods use {ValidateMethodPrefix}{PropertyName} convention.
        /// </remarks>
        private void ValidateProperty(string propertyName)
        {
            this.CheckPropertyExist(propertyName);

            MethodInfo validator = this
                .GetType()
                .GetMethod(
                    ValidateMethodPrefix + propertyName, 
                    BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (validator != null)
            {
                this.RemoveAllValidationErrors(propertyName);
                validator.Invoke(this, null);
            }
        }
    }
}