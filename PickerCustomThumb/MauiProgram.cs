using Microsoft.Extensions.Logging;

namespace PickerCustomThumb;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            /*.ConfigureMauiHandlers((handlers) =>
            {
#if ANDROID
                handlers.AddHandler(typeof(ExtendedSlider), typeof(PickerCustomThumb.Droid.ExtendedSliderRenderer));
#elif IOS
                handlers.AddHandler(typeof(ExtendedSlider), typeof(PickerCustomThumb.iOS.ExtendedSliderRenderer));
#endif
            })*/
            ;

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}