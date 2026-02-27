using Godot;
using System;

public partial class Background : Sprite2D
{
	[Export] public int GridSize = 50;
    [Export] public Color GridColor = new(0.2f, 0.2f, 0.2f, 0.15f);
    [Export] public Color BackgroundColor = new(0.05f, 0.05f, 0.05f, 1f);

     [ExportGroup("Overflow")]
    [Export] public float OverflowPercent = 0.2f; // 20% 溢出

    public override void _Ready()
    {
        if (GridSize <= 0) GridSize = 50;
        Texture = GenerateGridTexture();
        RegionEnabled = true;
        TextureRepeat = TextureRepeatEnum.Mirror;
        
        CallDeferred(nameof(UpdateRegion));
        GetTree().Root.SizeChanged += UpdateRegion;
    }

    private void UpdateRegion()
    {
        Vector2 viewportSize = GetViewportRect().Size;
        Vector2 targetSize = viewportSize * (1f + OverflowPercent);
        
        RegionRect = new Rect2(Vector2.Zero, targetSize);
        Position = viewportSize / 2;
    }

    private ImageTexture GenerateGridTexture()
    {
        var image = Image.CreateEmpty(GridSize, GridSize, false, Image.Format.Rgba8);
        image.Fill(BackgroundColor);
        
        for (int i = 0; i < GridSize; i++)
        {
            image.SetPixel(i, GridSize - 1, GridColor);
            image.SetPixel(GridSize - 1, i, GridColor);
        }

        var texture = new ImageTexture();
        texture.SetImage(image);
        return texture;
    }
}
