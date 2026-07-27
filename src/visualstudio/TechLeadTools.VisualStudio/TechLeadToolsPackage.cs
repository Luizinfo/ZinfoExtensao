using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TechLeadTools.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(
        "TechLeadTools",
        "Compartilhe trechos de código e navegue até a origem com o protocolo TLT.",
        "0.1.3")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    public sealed class TechLeadToolsPackage : AsyncPackage
    {
        public const string PackageGuidString = "F63364BB-6966-4B27-A02D-5D9CC42D07B4";

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await TechLeadToolsCommands.InitializeAsync(this);
        }
    }
}
