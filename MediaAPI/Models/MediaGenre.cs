using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class MediaGenre
    {
        public int MediaGenreId { get; set; }
        public int MediaId { get; set; }
        public Genre Genre { get; set; }

        public virtual MediaFile Media { get; set; }
    }
}
