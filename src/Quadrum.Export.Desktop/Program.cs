using System;
using System.Threading;
using revghost;

namespace Quadrum.Export.Desktop
{
    public class Program
    {
        public static void Main()
        {
            using var ghost = GhostInit.Launch(
                scope => {},
                scope => new Quadrum.Game.Module(scope)
            );

            while (ghost.Loop())
            {
                Thread.Sleep(10);
            }
        }
    }
}