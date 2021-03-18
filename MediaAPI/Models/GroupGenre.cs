using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class GroupGenre
    {
        public int GroupGenreId { get; set; }
        public int GroupId { get; set; }
        public int Genre { get; set; }

        public virtual Group Group { get; set; }
    }
}
