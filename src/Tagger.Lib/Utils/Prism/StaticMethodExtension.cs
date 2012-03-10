namespace Utils.Prism
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Markup;
    using System.Reflection;

    [MarkupExtensionReturnType(typeof(Action))]
    public class StaticMethodExtension : MarkupExtension
    {
        // {d:StaticMethod Program.Method1}
        public StaticMethodExtension(string method)
        {
            this.Method = method;
        }

        [ConstructorArgument("method")]
        public string Method { get; set; }

        private Action Function;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Function != null) { return this.Function; }

            int index = Method.IndexOf('.');
            if (index < 0)
            {
                throw new ArgumentException("MarkupExtensionBadStatic");
            }

            string qualifiedTypeName = this.Method.Substring(0, index);
            if (qualifiedTypeName == string.Empty)
            {
                throw new ArgumentException("MarkupExtensionBadStatic");
            }

            var service = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
            if (service == null)
            {
                throw new ArgumentException("MarkupExtensionNoContext");
            }

            var memberType = service.Resolve(qualifiedTypeName);
            var str = this.Method.Substring(index + 1, (this.Method.Length - index) - 1);
            if (str == string.Empty)
            {
                throw new ArgumentException("MarkupExtensionBadStatic");
            }

            var reflectedFunc = memberType.GetMethod(str, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

            if (reflectedFunc != null)
            {
                if (reflectedFunc.ReturnType == typeof(void))
                {
                    var v = Delegate.CreateDelegate(typeof(Action), reflectedFunc, true);
                    Function = (Action)v;
                }

            }

            return Function;
        }
    }
}
