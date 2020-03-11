
using Splendor.Core;

using static Splendor.Core.TokenColour;

namespace Splendor.Blazor
{
    public static class ExtensionMethods
    {
        public static string CardColour(this TokenColour c) => c switch
        {
            White => "#dcdcdc",
            Red   => "#a30000",
            Blue  => "#3c85df",
            Green => "#1aac1e",
            _     => "black"
        };

        public static string TokenColour(this TokenColour c) => c switch
        {
            Gold  => "#fc9003",
            White => "white",
            Red   => "#c00",
            Blue  => "#67a9f1",
            Green => "#4abc4b",
            _     => "#222"
        };

        public static string TextColour(this TokenColour c) => c switch
        {
            Black => "white",
            _     => "black"
        };
    }
}