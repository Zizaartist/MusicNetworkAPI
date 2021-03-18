using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class Favourite
    {
        public int FavouriteId { get; set; }
        public int? MediaId { get; set; }
        public int? UserId { get; set; }

        public virtual MediaFile Media { get; set; }
        public virtual User User { get; set; }
    }
}
