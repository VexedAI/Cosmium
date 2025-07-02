using Microsoft.Extensions.Localization;
using Cosmium.Web.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Cosmium.Web;

[Dependency(ReplaceServices = true)]
public class WebBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<WebResource> _localizer;

    public WebBrandingProvider(IStringLocalizer<WebResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}