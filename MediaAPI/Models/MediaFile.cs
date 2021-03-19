using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;

#nullable disable

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
        public int? MusicDataId { get; set; }
        public int? VideoDataId { get; set; }
        public int? AuthorId { get; set; }

        public virtual User Author { get; set; }
        public virtual MusicData MusicData { get; set; }
        public virtual VideoData VideoData { get; set; }
        public virtual ICollection<Favourite> Favourites { get; set; }
        public virtual ICollection<MediaGenre> MediaGenres { get; set; }
        public virtual ICollection<MediaInstrument> MediaInstruments { get; set; }
    }
}
