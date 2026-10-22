using BESHStat.OfficeAddin;
using BESHStat.OfficeAddin.Infrastructure;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddOfficeFoundation();
await builder.Build().RunAsync();
