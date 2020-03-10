
using static Splendor.TokenColour;

namespace Splendor.Blazor
{
    public static class ExtensionMethods
    {
        public static string CardColour(this TokenColour c) => c switch
        {
            White => "background-color--grey--3",
            Red   => "background-color--red--6",
            Blue  => "background-color--blue--5",
            Green => "background-color--green--5",
            _     => "background-color--black"
        };

        public static string TokenColour(this TokenColour c) => c switch
        {
            Gold  => "background-color--orange--5",
            White => "background-color--white",
            Red   => "background-color--red--5",
            Blue  => "background-color--blue--4",
            Green => "background-color--green--4",
            _     => "background-color--grey--9"
        };
    }
}