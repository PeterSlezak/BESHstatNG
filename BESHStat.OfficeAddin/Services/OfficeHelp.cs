using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public sealed class OfficeHelp(IOfficeWorkspaceBrowser browser)
{
    public ProductVersionDescriptor Version { get; } = ProductVersionDescriptor.FromVersion(typeof(OfficeHelp).Assembly.GetName().Version);
    public HelpLaunchTarget Home => HelpLaunchTarget.CreateHome(Version);
    public HelpLaunchTarget Settings => HelpLaunchTarget.Create(HelpDestination.ForPageKey("GlobalSettings"), Version);
    public HelpLaunchTarget Method(string id) => HelpLaunchTarget.Create(HelpDestination.ForAnalysisId(id), Version);
    public Task<string> OpenAsync(HelpLaunchTarget target) => browser.OpenHelpAsync(HelpLaunchTarget.SafeOpenUrl(target.Url));
}
