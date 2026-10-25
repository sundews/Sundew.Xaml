namespace Sundew.Xaml.Maui.Development.Tester;

using SundewBorder = Sundew.Xaml.Controls.Border;

public partial class MainPage : ContentPage
{
	/// <summary>
	/// The width the sheet is designed at, which every tester shares so that the sheets lay out alike. It is a
	/// floor here rather than a fixed size, because leaving the rest of a wider screen empty helps nobody.
	/// </summary>
	private const double SheetWidth = 900;

	/// <summary>
	/// What each element of the sheet does when the zoom changes, closed over the size it was designed at.
	/// </summary>
	private readonly List<Action<double>> zoomAppliers = new();

	private readonly List<double> rowHeights = new();
	private double zoom = 1;

	public MainPage()
	{
		InitializeComponent();

		// MAUI has no layout transform, and its Scale is no substitute here: a border draws into a canvas sized to
		// its layout, so scaling only magnifies that bitmap and the corners come out blocky. The zoom therefore
		// scales the sheet itself, multiplying every size, thickness, radius and font it was designed with, which
		// lays each border out larger and has it draw itself again at that size. Avalonia gets this from
		// LayoutTransformControl because it composites vectors rather than a bitmap per border.
		this.Capture(this.Examples);
		foreach (var rowDefinition in this.Examples.RowDefinitions)
		{
			this.rowHeights.Add(rowDefinition.Height.Value);
		}

		// The slider is wired here rather than bound in XAML, because an x:Reference binding leaves the page
		// blank under the source generated XAML inflator this project uses.
		this.ZoomSlider.ValueChanged += this.OnZoomChanged;
		this.SheetScroll.SizeChanged += this.OnViewportChanged;
		this.ApplyZoom(this.ZoomSlider.Value);
	}

	private static IEnumerable<Element> ChildrenOf(Element element)
	{
		switch (element)
		{
			case Layout layout:
				return layout.Children.OfType<Element>();
			case ContentView contentView when contentView.Content != null:
				return new Element[] { contentView.Content };
			default:
				return Enumerable.Empty<Element>();
		}
	}

	private static Thickness Scaled(Thickness thickness, double zoom)
	{
		return new Thickness(thickness.Left * zoom, thickness.Top * zoom, thickness.Right * zoom, thickness.Bottom * zoom);
	}

	private static CornerRadius Scaled(CornerRadius cornerRadius, double zoom)
	{
		return new CornerRadius(
			cornerRadius.TopLeft * zoom,
			cornerRadius.TopRight * zoom,
			cornerRadius.BottomLeft * zoom,
			cornerRadius.BottomRight * zoom);
	}

	private void Capture(Element element)
	{
		switch (element)
		{
			case SundewBorder border:
				var borderThickness = border.BorderThickness;
				var cornerRadius = border.CornerRadius;
				var contentPadding = border.ContentPadding;
				var margin = border.Margin;
				var width = border.WidthRequest;
				var height = border.HeightRequest;
				this.zoomAppliers.Add(zoom =>
				{
					border.BorderThickness = Scaled(borderThickness, zoom);
					border.CornerRadius = Scaled(cornerRadius, zoom);
					border.ContentPadding = Scaled(contentPadding, zoom);
					border.Margin = Scaled(margin, zoom);
					if (width > 0)
					{
						border.WidthRequest = width * zoom;
					}

					if (height > 0)
					{
						border.HeightRequest = height * zoom;
					}
				});

				break;

			case Label label:
				var fontSize = label.FontSize;
				this.zoomAppliers.Add(zoom => label.FontSize = fontSize * zoom);
				break;
		}

		foreach (var child in ChildrenOf(element))
		{
			this.Capture(child);
		}
	}

	private void OnZoomChanged(object? sender, ValueChangedEventArgs e)
	{
		this.ApplyZoom(e.NewValue);
	}

	private void ApplyZoom(double zoom)
	{
		this.zoom = zoom;
		foreach (var applier in this.zoomAppliers)
		{
			applier(zoom);
		}

		for (var index = 0; index < this.rowHeights.Count; index++)
		{
			this.Examples.RowDefinitions[index].Height = new GridLength(this.rowHeights[index] * zoom);
		}

		this.ZoomValue.Text = zoom.ToString("0.00");
		this.UpdateSheetWidth();
	}

	private void OnViewportChanged(object? sender, EventArgs e)
	{
		this.UpdateSheetWidth();
	}

	private void UpdateSheetWidth()
	{
		// Grow the sheet to the viewport where there is room, and keep it scrollable where there is not. Zooming
		// widens it in step, so the scroll area grows with the sheet rather than the sheet spilling out of it.
		var width = Math.Max(SheetWidth, this.SheetScroll.Width) * this.zoom;
		if (Math.Abs(this.Examples.WidthRequest - width) > 0.5)
		{
			this.Examples.WidthRequest = width;
		}
	}
}
