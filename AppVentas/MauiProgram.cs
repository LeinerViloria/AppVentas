﻿using AppVentas.Services;
using Microsoft.Extensions.Logging;

namespace AppVentas
{
    public static class MauiProgram
    {
        public static IServiceProvider Services { get; private set; } = null!;
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<LocalDbService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();
            Services = app.Services;
            return app;
        }
    }
}
