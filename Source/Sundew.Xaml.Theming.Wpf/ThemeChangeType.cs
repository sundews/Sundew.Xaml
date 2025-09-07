namespace Sundew.Xaml.Theming;

using Sundew.DiscriminatedUnions;

[DiscriminatedUnion]
public enum ThemeChangeType
{
    Theme,
    ThemeMode,
}