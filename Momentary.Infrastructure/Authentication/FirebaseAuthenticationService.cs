using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Momentary.Infrastructure.Authentication;

public class FirebaseAuthenticationService : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public FirebaseAuthenticationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _jsRuntime.InvokeVoidAsync("authFunctions.onAuthStateChanged", DotNetObjectReference.Create(this));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return await Task.FromResult(new AuthenticationState(_currentUser));
    }

    public async Task<string> SignInWithGoogle()
    {
        return await _jsRuntime.InvokeAsync<string>("authFunctions.signInWithGoogle");
    }

    public async Task SignOut()
    {
        await _jsRuntime.InvokeVoidAsync("authFunctions.signOut");
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    [JSInvokable]
    public void OnAuthStateChanged(string jsonUser)
    {
        if (string.IsNullOrEmpty(jsonUser))
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else
        {
            var user = JsonConvert.DeserializeObject<FirebaseUser>(jsonUser);
            if (user != null)
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Uid),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("photoUrl", user.PhotoUrl),
                }, "Firebase");
                _currentUser = new ClaimsPrincipal(identity);
            }
        }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

public record FirebaseUser(string Uid, string Email, string DisplayName, string PhotoUrl);