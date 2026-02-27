using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IPlayeable
{
    public void GameOver(Globals.GameOverType type);
    public bool IsAutoPlay();
}