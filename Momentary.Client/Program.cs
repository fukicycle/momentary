using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Momentary.Application.Posts;
using Momentary.Client;
using Momentary.Domain.Aggregates.Post;
using Momentary.Infrastructure.Authentication;
using Momentary.Infrastructure.Repositories;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// DI設定
builder.Services.AddScoped<IPostRepository, FirebasePostRepository>();
builder.Services.AddScoped<PostApplicationService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<FirebaseAuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<FirebaseAuthenticationService>());

await builder.Build().RunAsync();
