using Microsoft.Maui.Graphics;

namespace WordsTrainer.Mobile.Resources.Drawables;

public sealed class GlobeDrawable : IDrawable
{
    public Color StrokeColor { get; set; } = Color.FromArgb("#2965F1");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = StrokeColor;
        canvas.StrokeSize = 1.4f;
        canvas.StrokeLineCap = LineCap.Round;

        var padding = 1.5f;
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - padding * 2;
        var left = (dirtyRect.Width - size) / 2;
        var top = (dirtyRect.Height - size) / 2;

        canvas.DrawEllipse(left, top, size, size);
        canvas.DrawLine(left, top + size / 2, left + size, top + size / 2);

        var meridianWidth = size * 0.42f;
        var meridianLeft = left + (size - meridianWidth) / 2;
        canvas.DrawEllipse(meridianLeft, top, meridianWidth, size);
    }
}