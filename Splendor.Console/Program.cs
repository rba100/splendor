using System;
using System.Threading.Tasks;

namespace Splendor.ConsoleGame
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleDrawing.InitFullScreenApp();
            InteractiveRunner runner = new(); runner.Run();
            ConsoleDrawing.Clear();
        }

        static int colour = 0;

        static ConsoleColor Next()
        {
            colour++;
            if (colour == 16) colour = 1;
            return (ConsoleColor)colour;
        }

        static void DrawBox(int x, int y, int width, int height)
        {
            using (new ConsoleColour(Next()))
            {
                ConsoleDrawing.DrawBoxSingle(x, y, width, height);
            }
        }

        static void DrawRecurse(int x, int y, int width, int height)
        {
            DrawBox(x, y, width, height);
            if (width < 5 || height < 5) return;

            var right = (width - 2) / 2;
            var left = (width - 2) - right;
            var bottom = (height - 2) / 2;
            var top = (height - 2) - bottom;

            DrawRecurse(x + 1, y + 1, left, top);
            DrawRecurse(x + left + 1, y + 1, right, top);
            DrawRecurse(x + 1, y + top + 1, left, bottom);
            DrawRecurse(x + left + 1, y + top + 1, right, bottom);
        }
    }
}

