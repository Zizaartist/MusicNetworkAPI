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
        alternative,
        hiphop,
        pop,
        rap,
        instrumental,
        punk
    }

    public class GenreDictionaries
    {
        public static Dictionary<Genre, string> GenreToString = new Dictionary<Genre, string>()
        {
            { Genre.rock, "Рок" },
            { Genre.indie, "Инди" },
            { Genre.alternative, "Альтернатива" },
            { Genre.hiphop, "Хип-хоп" },
            { Genre.pop, "Поп-музыка" },
            { Genre.rap, "Рэп" },
            { Genre.instrumental, "Инструментальная музыка" },
            { Genre.punk, "Панк" },
        };

        public static Dictionary<string, Genre> StringToGenre = new Dictionary<string, Genre>()
        {
            { "Рок", Genre.rock },
            { "Инди", Genre.indie },
            { "Альтернатива", Genre.alternative },
            { "Хип-хоп", Genre.hiphop },
            { "Поп-музыка", Genre.pop },
            { "Рэп", Genre.rap },
            { "Инструментальная музыка", Genre.instrumental },
            { "Панк", Genre.punk },
        };
    }
}
