using Microsoft.AspNetCore.Components;
using BudgetFlow.Services;

namespace BudgetFlow.Components.Pages
{
    // Base class for pages that require user authentication
    public class AuthenticatedPageBase : ComponentBase
    {
        // Injected AuthenticationService to manage authentication state
        [Inject]
        protected AuthenticationService AuthService { get; set; } = default!;

        // Injected NavigationManager for navigating between pages
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        // Override OnInitialized to check user authentication on page load
        protected override void OnInitialized()
        {
            // If the user is not authenticated, redirect to the login page
            if (!AuthService.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/login", true);
            }
        }

        // Property to get the current user's ID
        // If the current user is null, return an empty string
        protected string CurrentUserId => AuthService.CurrentUser?.Id ?? string.Empty;
    }
}
