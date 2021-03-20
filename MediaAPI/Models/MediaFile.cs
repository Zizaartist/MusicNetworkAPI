using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class MediaFile
    {
        public MediaFile()
        {
            Favourites = new HashSet<Favourite>();
            MediaGenres = new HashSet<MediaGenre>();
            MediaInstruments = new HashSet<MediaInstrument>();
        }

        //Required
        public int MediaId { get; set; }
        public MediaType MediaType { get; set; }
        public string MediaPath { get; set; }
        public string MediaName { get; set; }
        public long Views { get; set; }
        public MediaExtension Extension { get; set; }

        //Nullable
        public int? AuthorId { get; set; }

        public User Author { get; set; } //Без virtual чтобы случайно не срабатывал lazy loading
        [JsonIgnore]
        public virtual VideoFile VideoFile { get; set; }
        [JsonIgnore]
        public virtual MusicFile MusicFile { get; set; }
        [JsonIgnore]
        public virtual ICollection<Favourite> Favourites { get; set; }
        public ICollection<MediaGenre> MediaGenres { get; set; }
        public ICollection<MediaInstrument> MediaInstruments { get; set; }

        #region validation

        public static bool ValidateModel(MediaFile _data)
        {
            try
            {
                if (_data == null ||
                    string.IsNullOrEmpty(_data.MediaName) ||
                    string.IsNullOrEmpty(_data.MediaPath) ||
                    !ValidateMediaInstruments(_data.MediaInstruments) ||
                    !ValidateMediaGenres(_data.MediaGenres) 
                    )
                {
                    return false;
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine($"Ошибка при валидации MediaFile {_ex}");
                return false;
            }
            return true;
        }

        private static bool ValidateMediaInstruments(ICollection<MediaInstrument> _instruments)
        {
            if (_instruments == null ||
                !_instruments.Any())
            {
                return false;
            }
            foreach (var instrument in _instruments)
            {
                if (!MediaInstrument.ValidateModel(instrument))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateMediaGenres(ICollection<MediaGenre> _genres)
        {
            if (_genres == null ||
                !_genres.Any())
            {
                return false;
            }
            foreach (var genre in _genres)
            {
                if (!MediaGenre.ValidateModel(genre))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
