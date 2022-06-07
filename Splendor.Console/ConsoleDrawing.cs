using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Splendor.ConsoleGame
{
    public static class ConsoleDrawing
    {
        static int _OriginX = 0;
        static int _OriginY = 0;

        public static void InitFullScreenApp()
        {
            Console.Clear();
            _OriginX = 0;
            _OriginY = Console.CursorTop;
        }

        public static void Clear()
        {
            BlankRegion(_OriginX, _OriginY, Console.WindowWidth, Console.WindowHeight);
            SetPos(0,0);
        }

        public static void MessageBox(string message)
        {
            var width = message.Length + 4;
            var height = 5;
            var x = (Console.WindowWidth - width) / 2;
            var y = (Console.WindowHeight - height) / 2;
            BlankRegion(x, y, width, height);
            DrawBoxDouble(x, y, width, height);
            WriteAt(message, x + 2, y + 2);
        }

        public static void MessageBox(params string[] messages)
        {
            var width = messages.Max(l => l.Length) + 4;
            var height = 4 + messages.Length;
            var x = (Console.WindowWidth - width) / 2;
            var y = (Console.WindowHeight - height) / 2;
            BlankRegion(x, y, width, height);
            DrawBoxDouble(x, y, width, height);
            for(int i = 0; i < messages.Length; i++)
                WriteAt(messages[i], x + 2, y + 2 + i);
        }

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
            for (var i = 0; i < height; i++)
            {
                SetPos(x, y + i);
                Console.Write(blank);
            }
        }

        public static void WriteAt(char c, int x, int y)
        {
            SetPos(x, y);
            Console.Write(c);
        }

        public static void WriteAt(string str, int x, int y)
        {
            SetPos(x, y);
            Console.Write(str);
        }

        public static void DrawLineX(char c, int x, int y, int length)
        {
            SetPos(x, y);
            Console.Write(new string(c, length));
        }

        public static void DrawLineY(char c, int x, int y, int length)
        {
            for (int i = 0; i < length; i++)
            {
                SetPos(x, y + i);
                Console.Write(c);
            }
        }

        private static void SetPos(int x, int y)
        {
            Console.SetCursorPosition(_OriginX + x, _OriginY + y);
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
