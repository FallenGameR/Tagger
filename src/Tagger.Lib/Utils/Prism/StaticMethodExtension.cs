namespace Utils.Prism
{
    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(Action))]
    public class StaticMethodExtension : MarkupExtension
    {
        // {d:StaticMethod Program.Method1}
        public StaticMethodExtension(string methodPath)
        {
            this.MethodPath = methodPath;
        }

        [ConstructorArgument("methodPath")]
        public string MethodPath { get; set; }

        private Action Function;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Function != null) { return this.Function; }

            var match = Regex.Match(this.MethodPath, @"^(?<type>.+)\.(?<method>[^\.]+)$");
            if (!match.Success)
            {
                throw new ArgumentException("Failed to parse method path. Use full path to the method, without parentesis at the end.");
            }

            var typeResolver = (IXamlTypeResolver)serviceProvider.GetService(typeof(IXamlTypeResolver));
            var methodInfo = typeResolver
                .Resolve(match.Groups["type"].Value)
                .GetMethod(
                    match.Groups["method"].Value, 
                    BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

            if ((methodInfo != null) && (methodInfo.ReturnType == typeof(void)))
            {
                this.Function = (Action)Delegate.CreateDelegate(typeof(Action), methodInfo, true);
            }

            return this.Function;
        }
    }
}
