//-----------------------------------------------------------------------
// <copyright file="Reflection.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Extensions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Extension class for reflection logic
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Get property name from property accessor
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="obj">Extended class - any object</param>
        /// <param name="propertyGetter">Expression that evaluates to property get</param>
        /// <example>
        /// 1) OnPropertyChanged( this.Property( () =&gt; DefaultExpositionMinSize ) );
        /// 2) lblName.DataBindings.Add( this.Property( () =&gt; Name ), this, null );
        /// </example>
        /// <remarks>
        /// Usefull for mitigating property name refactorings. 
        /// Rename refactorings would not break your bindings anymore.
        /// </remarks>
        /// <returns>
        /// Property name.
        /// </returns>
        public static string Property<T>(this object obj, Expression<Func<T>> propertyGetter)
        {
            return ((MemberExpression)propertyGetter.Body).Member.Name;
        }
    }
}
