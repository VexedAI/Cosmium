using Volo.Abp.Application.Services;
using Cosmium.Web.Localization;

namespace Cosmium.Web.Services;

/* Inherit your application services from this class. */
public abstract class WebAppService : ApplicationService
{
    protected WebAppService()
    {
        LocalizationResource = typeof(WebResource);
    }
}