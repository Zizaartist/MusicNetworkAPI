using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class VideoData
    {
        public VideoData()
        {
            MediaFiles = new HashSet<MediaFile>();
        }

        public int VideoId { get; set; }
        public string Preview { get; set; }

        public virtual ICollection<MediaFile> MediaFiles { get; set; }
    }
}
