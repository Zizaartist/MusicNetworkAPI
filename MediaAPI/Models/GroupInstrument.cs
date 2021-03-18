using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class GroupInstrument
    {
        public int GroupInstrumentId { get; set; }
        public int GroupId { get; set; }
        public Instrument Instrument { get; set; }

        public virtual Group Group { get; set; }
    }
}
