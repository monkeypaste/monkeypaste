using Avalonia.Web.Blazor;

namespace HelloWorld.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        WebAppBuilder.Configure<HelloWorld.App>()
            .SetupWithSingleViewLifetime();
    }
}