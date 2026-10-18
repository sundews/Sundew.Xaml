# Sundew.Xaml

## Controls:
* Border - A custom four-sided border, allowing more control over corner transitions. (WPF, Avalonia)

### Examples:
![Examples](Source/Resources/Examples.png)


### Known issues/Todo:
* CornerRadius 0 is not properly supported: a corner is never drawn tighter than the thicker of the two sides meeting
  it, so the corners of a square border are rounded by that thickness rather than left square.
* Performance optimizations

### Avalonia notes:
* `Border` derives from `Decorator` rather than from Avalonia's own `Border`, because the latter seals its rendering.
  `Background`, `BorderThickness` and `CornerRadius` are registered with `AddOwner`, so they are the very same
  `AvaloniaProperty` instances and existing styles and theme resources still apply.
* Avalonia has no `GuidelineSet`, so the guidelines the calculator produces are only used by WPF. Avalonia gets the
  same crisp edges a different way: the border measures its own scale and position all the way to the `TopLevel`, so
  that it rounds against the scale it is actually drawn at rather than `RenderScaling` alone, and nudges itself by up
  to half a device pixel so that its edges land on pixel boundaries. This matters under anything that scales the
  border from above, a zoom through a `LayoutTransformControl` say, which is applied after the border has drawn
  itself and would otherwise leave every edge half covering a pixel. A rotated or skewed border has no pixel grid to
  line up with and is left alone.
* A see through corner is drawn under an opacity mask rather than `PushOpacity`, because that one multiplies each
  slice of the swept blend on its own instead of compositing the corner as a whole, which shows the slices.
* WPF clips a child to the space it was arranged into whenever the child wanted more, so content that does not fit is
  cut off at the border. Avalonia has no such layout clip, so the border applies the same thing itself and content
  that overflows is cut off on both platforms rather than being drawn over the border and beyond it.
  `ClipContentToCornerRadius` then decides whether that clip follows the corners or is the plain content rectangle.

## Theming (WPF):
* ThemeManager - A theme manager with support for themes, theme modes and automatic synchronization with system theme.
* ThemeResourceDictionary - A caching resource dictionary for managing themes.
* ThemeModeResourceDictionary - A caching resource dictionary for managing theme modes.

## Controls.Overlays (WPF):
* OverlayWindow - A Window for overlays.
* SubtitleOverlay - A Window for displaying subtitles.
