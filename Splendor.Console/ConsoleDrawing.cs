using System;
using System.Collections.Generic;
using System.Text;

namespace Splendor.ConsoleGame
{
    public static class ConsoleDrawing
    {
        public static void DrawBox(int x, int y, int width, int height)
        {
            WriteAt('#', x, y);
            WriteAt('#', x + width - 1, y);
            WriteAt('#', x, y + height - 1);
            WriteAt('#', x + width - 1, y + height - 1);

            DrawLineX('-', x + 1, y, width - 2);
            DrawLineX('-', x + 1, y + height - 1, width - 2);

            DrawLineY('|', x, y + 1, height - 2);
            DrawLineY('|', x + width - 1, y + 1, height - 2);
        }

        public static void DrawBoxDouble(int x, int y, int width, int height)
        {
            WriteAt('╔', x, y);
            WriteAt('╗', x + width - 1, y);
            WriteAt('╚', x, y + height - 1);
            WriteAt('╝', x + width - 1, y + height - 1);

            DrawLineX('═', x + 1, y, width - 2);
            DrawLineX('═', x + 1, y + height - 1, width - 2);

            DrawLineY('║', x, y + 1, height - 2);
            DrawLineY('║', x + width - 1, y + 1, height - 2);
        }

        public static void DrawBoxSingle(int x, int y, int width, int height)
        {
            WriteAt('┌', x, y);
            WriteAt('┐', x + width - 1, y);
            WriteAt('└', x, y + height - 1);
            WriteAt('┘', x + width - 1, y + height - 1);

            DrawLineX('─', x + 1, y, width - 2);
            DrawLineX('─', x + 1, y + height - 1, width - 2);

            DrawLineY('│', x, y + 1, height - 2);
            DrawLineY('│', x + width - 1, y + 1, height - 2);
        }

        public static void BlankRegion(int x, int y, int width, int height)
        {
            var blank = new string(' ', width);
            for(var i = 0; i < height; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(blank);
            }
        }

        public static void WriteAt(char c, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(c);
        }

        public static void WriteAt(string str, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(str);
        }

        public static void DrawLineX(char c, int x, int y, int length)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(new string(c, length));
        }

        public static void DrawLineY(char c, int x, int y, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(c);
            }
        }
    }

    internal class ConsoleColour : IDisposable
    {
        private readonly ConsoleColor m_PrevousColour;

        public ConsoleColour(ConsoleColor colour)
        {
            m_PrevousColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
        }

        public void Dispose()
        {
            Console.ForegroundColor = m_PrevousColour;
        }
    }
}
