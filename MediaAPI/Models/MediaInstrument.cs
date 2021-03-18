using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class MediaInstrument
    {
        public int MediaInstrumentId { get; set; }
        public int MediaId { get; set; }
        public Instrument Instrument { get; set; }

        public virtual MediaFile Media { get; set; }
    }
}
