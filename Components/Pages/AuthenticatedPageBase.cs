using Microsoft.AspNetCore.Components;
using BudgetFlow.Services;

namespace BudgetFlow.Components.Pages
{
    public class AuthenticatedPageBase : ComponentBase
    {
        [Inject]
        protected AuthenticationService AuthService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        protected bool IsAuthenticated => AuthService.IsAuthenticated;
        protected string CurrentUserId => AuthService.CurrentUser?.Id ?? string.Empty;
        protected bool ShowRedirectMessage { get; private set; }

        protected override void OnInitialized()
        {
            if (!IsAuthenticated)
            {
                ShowRedirectMessage = true;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (ShowRedirectMessage)
            {
                await Task.Delay(2000); // Delay for 2 seconds to show the message
                NavigationManager.NavigateTo("/login", true);
            }
        }
    }
}
