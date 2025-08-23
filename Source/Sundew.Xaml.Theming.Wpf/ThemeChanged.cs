namespace Sundew.Xaml.Theming;

/// <summary>
/// The theme change.
/// </summary>
public sealed class ThemeChanged : ThemeUpdatedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeUpdatedEventArgs"/> class.
    /// </summary>
    /// <param name="oldTheme"></param>
    /// <param name="newTheme"></param>
    public ThemeChanged(Theme? oldTheme, Theme newTheme)
    {
        this.OldTheme = oldTheme;
        this.NewTheme = newTheme;
    }

    /// <summary>
    /// The old theme.
    /// </summary>
    public Theme? OldTheme { get; }

    /// <summary>
    /// The new theme.
    /// </summary>
    public Theme NewTheme { get; }
}