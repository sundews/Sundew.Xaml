namespace Sundew.Xaml.Uno.Development.Tester;

using global::Uno.UI.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWebAssembly()
            .Build();

        await host.RunAsync();
    }
}