using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Models.EnumModels
{
    public enum Genre
    {
        rock,
        indie,
        alternative
    }

    public class GenreDictionaries
    {
        public static Dictionary<Genre, string> GenreToString = new Dictionary<Genre, string>()
        {
            { Genre.rock, "Рок" },
            { Genre.indie, "Инди" },
            { Genre.alternative, "Альтернатива" }
        };

        public static Dictionary<string, Genre> StringToGenre = new Dictionary<string, Genre>()
        {
            { "Рок", Genre.rock },
            { "Инди", Genre.indie },
            { "Альтернатива", Genre.alternative }
        };
    }
}
