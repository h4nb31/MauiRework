namespace MauiApplication;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = new Window(new MainPage()) { Title = "Posadmin" };

#if WINDOWS
        window.MinimumHeight = 500;
        window.MinimumWidth = 500;
#endif

        return window;

    }
}