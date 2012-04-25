//-----------------------------------------------------------------------
// <copyright file="StaticMethodExtension.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Prism
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;

    /// <summary>
    /// XAML markup extensions that allows to bind to static methods
    /// </summary>
    /// <example>
    /// {d:StaticMethod Tagger.RegistrationManager.GlobalHotkeyHandle}
    /// </example>
    /// <remarks>
    /// Original taken from a Stack Overflow answer
    /// </remarks>
    [MarkupExtensionReturnType(typeof(Action))]
    public class StaticMethodExtension : MarkupExtension
    {
        /// <summary>
        /// Delegate that points to the static method being used. 
        /// </summary>
        private Action function;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticMethodExtension"/> class.
        /// </summary>
        /// <param name="methodPath">
        /// Path to the static method that is being used.
        /// </param>
        public StaticMethodExtension(string methodPath)
        {
            this.MethodPath = methodPath;
        }

        /// <summary>
        /// Gets or sets path to the static method that is being used.
        /// </summary>
        [ConstructorArgument("methodPath")]
        public string MethodPath { get; set; }

        /// <summary>
        /// Provides an object that is provided as the value of the target property for this markup extension. 
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied. </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.function != null)
            {
                return this.function;
            }

            var match = Regex.Match(this.MethodPath, @"^(?<type>.+)\.(?<method>[^\.]+)$");
            if (!match.Success)
            {
                throw new ArgumentException("Failed to parse method path. Use full path to the method, without parentesis at the end.");
            }

            var query =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.FullName == match.Groups["type"].Value
                let method = type.GetMethod(match.Groups["method"].Value, BindingFlags.Public | BindingFlags.Static)
                where method.ReturnType == typeof(void)
                select Delegate.CreateDelegate(typeof(Action), method, true);

            this.function = (Action)query.FirstOrDefault();
            return this.function;
        }
    }
}
