using BudgetFlow.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace BudgetFlow
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Register UserService as a Singleton
            builder.Services.AddSingleton<AuthenticationService>();

            builder.Services.AddScoped<BalanceService>();
            builder.Services.AddSingleton<AddTransactionService>();
            builder.Services.AddSingleton<AddDebtService>();
            builder.Services.AddScoped<TransactionHistoryService>();
            builder.Services.AddSingleton<DebtHistoryService>();
            builder.Services.AddMudServices();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
