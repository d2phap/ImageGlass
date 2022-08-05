using DirectN;

namespace ImageGlass.Views;


public static class IVHelpers
{

    /// <summary>
    /// Creates checker box tile for drawing checkerboard (GDI+)
    /// </summary>
    public static Bitmap CreateCheckerBoxTile(float cellSize, Color cellColor1, Color cellColor2)
    {
        // draw the tile
        var width = cellSize * 2;
        var height = cellSize * 2;
        var result = new Bitmap((int)width, (int)height);

        using var g = Graphics.FromImage(result);
        using (Brush brush = new SolidBrush(cellColor2))
        {
            g.FillRectangle(brush, new RectangleF(cellSize, 0, cellSize, cellSize));
            g.FillRectangle(brush, new RectangleF(0, cellSize, cellSize, cellSize));
        }

        using (Brush brush = new SolidBrush(cellColor1))
        {
            g.FillRectangle(brush, new RectangleF(0, 0, cellSize, cellSize));
            g.FillRectangle(brush, new RectangleF(cellSize, cellSize, cellSize, cellSize));
        }

        return result;
    }


    
}

