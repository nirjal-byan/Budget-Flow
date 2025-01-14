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

        protected override void OnInitialized()
        {
            if (!AuthService.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/login", true);
            }
        }

        protected string CurrentUserId => AuthService.CurrentUser?.Id ?? string.Empty;
    }
}