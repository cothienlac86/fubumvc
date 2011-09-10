using System.Reflection;
using Bottles;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Diagnostics.Features.Html.Preview;
using FubuMVC.Diagnostics.Tests.Features.Html;

namespace FubuMVC.Diagnostics.Tests
{
    public class ObjectMother
    {
        public static FubuRegistry DiagnosticsRegistry()
        {
            return new FubuDiagnosticsRegistry();
        }

        public static BehaviorGraph DiagnosticsGraph()
        {
            PackageRegistry.LoadPackages(f => { });
            return DiagnosticsRegistry()
                    .BuildGraph();
        }

        public static HtmlConventionsPreviewContext BasicPreviewContext()
        {
            return new HtmlConventionsPreviewContext(typeof(SampleContextModel).FullName, 
                typeof(SampleContextModel), new SampleContextModel(), new PropertyInfo[0]);
        }
    }
}