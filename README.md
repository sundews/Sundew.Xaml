# Sundew.Xaml

## Controls:
* Border - A custom four-sided border, allowing more control over corner transitions.
  (WPF, Avalonia, WinUI, Uno, MAUI)

### Examples:
![Examples](Source/Resources/Examples.png)


### Known issues/Todo:
* CornerRadius 0 is not properly supported: a corner is never drawn tighter than the thicker of the two sides meeting
  it, so the corners of a square border are rounded by that thickness rather than left square.
* Performance optimizations
* The Avalonia tester’s `--render` writes a blank image. It renders the view detached from a top level, which
  leaves it unstyled and undrawn. The Avalonia captures in the table below were taken another way.

### How the drawing is shared:
The WinUI, Uno and MAUI libraries paint from one set of source files under `Source/Shared`. `BorderRenderer`
holds the whole of how a border is painted, `IBorderDrawingSession` is the handful of fills a backend has to
provide, and `Paint`, `PaintStop`, `PaintColor` and `CornerBlend` are what it paints with. None of them knows
about XAML or about any one graphics library, so the Skia session is shared too, and only `BrushResolver`, which
turns one framework’s brushes into a `Paint`, is written per framework.

Colours cross that seam as `PaintColor` rather than any framework’s own type, which is what lets the same files
serve `Windows.UI.Color` on WinUI and Uno and the float channels of `Microsoft.Maui.Graphics.Color` on MAUI.

### WinUI and Uno notes:
* One package, `Sundew.Xaml.Controls.WinUI`, serves both WinUI 3 and every Uno target, because Uno implements the
  same `Microsoft.UI.Xaml` API. It cross targets `net9.0-windows10.0.19041.0`, which is WinUI 3 proper and also the
  Windows head of an Uno app, and `net9.0`, which is every other Uno target.
* Neither WinUI nor Uno lets a control paint itself the way WPF and Avalonia do, so the border is a `Panel` holding a
  drawing surface behind a plain `Border` that carries the content. The surface is the only part that differs:
  Win2D on Windows, Skia (`SKCanvasElement`) everywhere else. Both run the same `Border` over `IBorderDrawingSession`,
  and `BorderRenderer` holds the whole of how a border is painted, free of XAML and of any one graphics library.
  A further backend is a matter of implementing `IBorderDrawingSession` and nothing else.
* WinUI only lets `Clip` be a rectangle, so the rounded part of `ClipContentToCornerRadius` is left to the corner
  radius of the plain border holding the content, which both WinUI and Uno clip to natively.
* Neither WinUI nor Uno honours a `TypeConverter`, on the type or on the property, so `Stops`, `CornerStops` and
  `SideBrushes` carry `CreateFromString` as well, which both do honour. The attribute only exists where WinUI or Uno
  is referenced, so `Stops` and `CornerStops` live in `Source/Shared` and each platform library compiles its own
  copy. Sharing them as an assembly instead would put the same type name in two assemblies of one reference graph,
  which neither XAML compiler can resolve.
* `Background` deliberately hides `Panel.Background`, which a panel would otherwise paint as a plain rectangle with
  square corners behind the rounded border.
* WinUI has no `LayoutTransform` and its brushes have no `RelativeTransform`, so the tester sheet zooms with a render
  transform and its one rotated background gradient runs straight across instead.

### MAUI notes:
* One backend serves every MAUI target. MAUI offers a Skia canvas on all of them, Windows included, so unlike
  the WinUI and Uno library there is nothing to switch on: `SKCanvasView` is the surface everywhere. Win2D would
  work on the Windows target, since that target is WinUI 3, but it would buy little, because Win2D and Skia
  already agree to 0.65% of pixels, and it would cost a handler for one target alone. The seam is kept all the
  same: a Win2D surface would be an `IBorderSurface` and a drawing session, and nothing else would change.
* The border paints its own `Fill` rather than the inherited `Background`. MAUI paints a `VisualElement`
  background itself, as a plain rectangle behind the whole control, which would show square corners outside the
  rounded border. `Padding` is likewise `ContentPadding`, because a MAUI `Layout` already has a `Padding` and it
  would inset the drawing surface along with the content.
* MAUI clips to any shape, so `ClipContentToCornerRadius` is a rounded clip stated directly, where WinUI has to
  leave the rounded part to the corner radius of a plain border holding the content.
* The border is told the device pixel scale by its own surface, which reads it off the canvas it has just been
  handed, rather than from the display. WinUI takes it from the `XamlRoot`, but MAUI has no equivalent that is
  reliable from inside a control, and the canvas knows it exactly, rounding included.
* A MAUI corner radius is ordered top left, top right, bottom left, bottom right, where WinUI goes round the
  other way, so the inner radius the content is clipped to is reordered on the way out of the calculator.

### Development testers:
* `Sundew.Xaml.Wpf.Development.Tester`, `Sundew.Xaml.Avalonia.Development.Tester`,
  `Sundew.Xaml.Uno.Development.Tester` and `Sundew.Xaml.Maui.Development.Tester` are all in the solution. WinUI has
  no AnyCPU platform, so the solution maps its own Any CPU onto that project's x64.
* The Uno tester is one Uno single project with four heads: `net10.0-desktop`, `net10.0-android`, `net10.0-ios` and
  `net10.0-browserwasm`. The last three need the android, ios and wasm-tools workloads, which a plain build agent
  does not have, so the main CI job passes `-p:SundewUnoAllHeads=false` and builds the desktop head alone. Build
  one head with `dotnet build Source/Sundew.Xaml.Uno.Development.Tester -f net10.0-android`.
* Running the Android head needs one Android SDK that has both the platform the build compiles against and a system
  image for the emulator to boot. Where Visual Studio and Android Studio have each installed their own, neither has
  both, and the AVD only resolves its image under the SDK it was created in. Point `ANDROID_HOME` and
  `ANDROID_SDK_ROOT` at that one and add the missing platform to it:
  `sdkmanager --sdk_root=<that sdk> "platforms;android-36" "build-tools;36.0.0"`.
* The Avalonia tester has four heads in one project: `net10.0`, `net10.0-android`, `net10.0-ios` and
  `net10.0-browser`. The last three need the android, ios and wasm-tools workloads, so the main CI job passes
  `-p:SundewAvaloniaAllHeads=false` there too. The control library itself needed no change for any of them: it
  uses only core Avalonia drawing, so the Skia renderer Avalonia uses off the desktop draws it the same way.
* The browser head needs three things beyond its target framework. Without `RuntimeIdentifier` set to
  `browser-wasm` it still compiles, and still reports success, but produces plain assemblies and no app bundle
  at all. `WasmMainJSPath` deploys `wwwroot/main.js`, while `index.html` only comes along because
  `WasmExtraFilesToDeploy` names it. And `dotnet run`, which is also what F5 starts, hands the bundle to
  WasmAppHost, which refuses it with "no perHostConfigs found" unless a runtimeconfig template declares a
  browser host; `UserRuntimeConfig` points at the one under `Platforms/Browser` so the other heads keep theirs.
* Start the browser head with `dotnet run -f net10.0-browser`, or from Visual Studio by picking that framework
  in the debug dropdown and pressing F5. Either way WasmAppHost serves the bundle and prints the URL it is on.
* Each head brings its own entry point under `Platforms`, and `Program.cs` at the project root belongs to the
  desktop head alone, which is why every other head removes it. `App` itself is shared: the desktop head gets a
  window, and Android, iOS and the browser get the very same `MainView` as a single view.
* The MAUI tester is one MAUI single project with four heads: `net10.0-android`, `net10.0-ios`,
  `net10.0-maccatalyst` and `net10.0-windows10.0.19041.0`. CI passes `-p:SundewMauiAllHeads=false` to build the
  Windows head alone. The control library has no such switch, because a package has to carry every framework it
  supports, which is why the main CI job installs the MAUI workloads before it can build the solution at all.
* A second CI job, `heads`, installs the android, wasm-tools and maui workloads and builds the Uno Android and
  WebAssembly heads, the Avalonia Android and browser heads and the MAUI Android head. It runs on main and
  release only, because
  installing those workloads costs a few minutes, and it packs and publishes nothing: `-p:SppDisable=true` keeps
  Sundew.Packaging.Publish out of it, so the job only answers whether the heads still compile. Both iOS heads are
  left out. They do compile on Windows, but only as far as their managed assemblies, with no app bundle; that
  needs Xcode, so a runnable iOS build would take a macOS job of its own.
* The MAUI tester shows the sheet with its content, so that hosting content in the border is exercised too, but it
  has no `ComparisonView` and no `--render` mode yet, so MAUI is not part of the pixel comparison below.
* MAUI has no layout transform, and `Scale` is no substitute. A border draws into a canvas sized to its layout, so
  scaling only magnifies that bitmap: straight edges stay hard, because the magnification does not interpolate,
  but the corner arcs come out in visible blocks while the text beside them, which Android rasterises again under
  the transform, stays smooth. The zoom therefore scales the sheet itself, in the code behind, multiplying every
  size, thickness, radius and font it was designed with, so that each border is laid out larger and draws itself
  again at that size. Wrapping the sheet in a layout transform of our own does not help, because the drawing
  would still be a `Scale`; Avalonia gets away with exactly that only because it composites vectors rather than a
  bitmap per border.
* Measuring this needs a curved edge. A straight one looks the same either way, since the magnification does not
  interpolate, and an edge sharpness figure is if anything higher for the magnified one, blocks being abrupt. The
  corner arc is where the difference shows.
* The sheet grows to the width of the viewport instead of staying at the 900 the other testers fix it at, which on
  a tablet would leave a third of the screen empty. 900 is kept as a floor, so a narrower screen scrolls instead of
  squeezing. This is the one place the MAUI sheet deliberately does not lay out like the others.
* The slider is wired in the code behind rather than bound in XAML. An `x:Reference` binding to it compiles, and
  throws nothing at runtime, but leaves the whole page blank under the source generated XAML inflator the project
  uses, which is worth knowing before reaching for one elsewhere.
* On Android the slider is padded clear of the top of the screen. MAUI draws edge to edge there, so it would
  otherwise sit under the status bar, and the top of the screen is reserved for the system gestures, which take
  the touches before the app sees them.
* Each tester shows the same sheet of examples. `ComparisonView` is that sheet with the content left out, which is
  what the platforms are compared on: text rasterises differently everywhere and would drown out the border.
* The WinUI and Uno testers take `--render <file>` and write that sheet to a PNG. They have to, because a border
  draws onto a canvas the compositor owns and neither a screen grab nor `RenderTargetBitmap` can read it back; the
  render mode asks each border to draw itself onto a surface of its own, through the very same code. It pins the
  device pixel scale rather than taking it from the display, so a captured sheet is the same wherever it was taken,
  and it records where each border was found, because two sheets are only comparable if the borders line up.

### How closely the platforms agree:
Rendering that sheet with geometry rounded at scale 1, out of 675,000 pixels:

| | differing at all | by more than 32 of 255 |
| --- | --- | --- |
| WPF against WinUI | 0.69% | **none at all** |
| WinUI against Uno | 1.61% | 0.38% |

The WinUI and Uno residual is not rendering. Twelve of the seventeen borders land at identical positions on both,
and every one of those twelve agrees to within 0.007% of pixels above 32. The other five are placed a little under
half a pixel apart, because Uno rounds some layout to the pixel grid of the display even where
`UseLayoutRounding` is off, and every differing pixel is along the edges of those five.

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
* The zoom is bound in XAML, straight onto the `ScaleTransform` of the `LayoutTransformControl`, with
  `{Binding #ZoomSlider.Value}`. An `ElementName` binding cannot reach a transform, which is not in the name scope
  it resolves through, but the `#name` form of a compiled binding is resolved against the XAML itself and can.
  The MAUI tester cannot do the same: its equivalent, `x:Reference`, compiles and throws nothing but leaves the
  page blank, so its slider is wired in the code behind.
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
