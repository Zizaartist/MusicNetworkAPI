using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class MusicData
    {
        public MusicData()
        {
            MediaFiles = new HashSet<MediaFile>();
        }

        public int MusicId { get; set; }
        public string Cover { get; set; }
        public string Artist { get; set; }

        public virtual ICollection<MediaFile> MediaFiles { get; set; }
    }
}
