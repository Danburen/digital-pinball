using Godot;
using System;
using System.Linq;

public partial class LevelGenerator : Node
{
    [Export] private float NoiseScale = 0.3f;
    [Export] private float Threshold = 0.4f;
    private FastNoiseLite _noise;
    private int _Columns;
    public LevelGenerator()
    {
        _Columns = 6;
    }
    public LevelGenerator(int columns)
    {
        _Columns = columns;
        _noise = new FastNoiseLite();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        _noise.Seed = (int)GD.Randf() * 10000;
    }
    public int[] GenerageRow(int rowIndex)
    {
        bool[] hasBrick = new bool[_Columns];
        int[] values = new int[_Columns];
        for(int col = 0; col < _Columns; col++)
        {
            float n = _noise.GetNoise2D(col * NoiseScale, rowIndex * NoiseScale);
            float normalize = (n + 1f) / 2f;
            hasBrick[col] = normalize > Threshold;
        }
        EnsurePlayability(hasBrick);
        for(int i = 0; i < _Columns; i++)
        {
            if(hasBrick[i])
            {
                values[i] = GetInitialBrickHitpoints(rowIndex);
            }
        }

        int spikeCount = GD.RandRange(1, 2);
	    for (int i = 0; i < spikeCount; i++)
		{
            if(! hasBrick[i]) continue;
			int pos = GD.RandRange(1, _Columns - 2);
			values[pos] = values[pos] * (int)Mathf.Pow(2, GD.RandRange(2, 3));
		}
        return values;
    }

    private void EnsurePlayability(bool[] hasBrick)
    {
        if (hasBrick.All(x => x)) // at least one space
            hasBrick[GD.RandRange(0, _Columns-1)] = false;
        for (int i = 0; i < _Columns - 3; i++) // ensure four space.
        {
            if (hasBrick[i] && hasBrick[i+1] && hasBrick[i+2] && hasBrick[i+3])
                hasBrick[i+2] = false;
        }
    }

    private  int GetInitialBrickHitpoints(int rowIndex)
	{
		if (rowIndex >= 4) return 2;
        if (rowIndex >= 2) return GD.RandRange(0, 3) == 0 ? 4 : 2;
        return GD.RandRange(0, 2) switch
        {
            0 => 2,
            1 => 4,
            _ => 8
        };
	}
}
