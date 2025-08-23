namespace Sundew.Xaml.Theming;

/// <summary>
/// The theme mode change.
/// </summary>
public sealed class ThemeModeChanged : ThemeUpdatedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeModeChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldThemeMode"></param>
    /// <param name="newThemeMode"></param>
    public ThemeModeChanged(ThemeMode? oldThemeMode, ThemeMode newThemeMode)
    {
        this.OldThemeMode = oldThemeMode;
        this.NewThemeMode = newThemeMode;
    }

    /// <summary>
    /// The old theme mode.
    /// </summary>
    public ThemeMode? OldThemeMode { get; }

    /// <summary>
    /// The new theme mode.
    /// </summary>
    public ThemeMode NewThemeMode { get; }
}