using System;
using System.Linq.Expressions;

namespace Utils.Reflection
{
    public static class Extensions
    {
        /// <summary>
        /// Get property name from property accessor
        /// </summary>
        /// <example>
        /// 1) OnPropertyChanged( this.Property( () => DefaultExpositionMinSize ) );
        /// 2) lblName.DataBindings.Add( this.Property( () => Name ), this, null );
        /// </example>
        /// <remarks>
        /// Usefull for name refactorings that wouldn't ruin your bindings anymore
        /// </remarks>
        public static string Property<T>(this object obj, Expression<Func<T>> propertyGetter)
        {
            return (propertyGetter.Body as MemberExpression).Member.Name;
        }
    }
}
