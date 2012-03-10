namespace Utils.Prism
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(Action))]
    public class StaticMethodExtension : MarkupExtension
    {
        // {d:StaticMethod Tagger.RegistrationManager.GlobalHotkeyHandle}
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

            var query =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.FullName == match.Groups["type"].Value
                let method = type.GetMethod(match.Groups["method"].Value, BindingFlags.Public | BindingFlags.Static)
                where method.ReturnType == typeof(void)
                select Delegate.CreateDelegate(typeof(Action), method, true);

            this.Function = (Action)query.FirstOrDefault();
            return this.Function;
        }
    }
}
