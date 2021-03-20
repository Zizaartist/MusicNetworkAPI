using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class MediaInstrument
    {
        public int MediaInstrumentId { get; set; }
        public int MediaId { get; set; }
        public Instrument Instrument { get; set; }

        [JsonIgnore]
        public virtual MediaFile Media { get; set; }

        #region validation

        public static bool ValidateModel(MediaInstrument _data)
        {
            try
            {
                if (_data == null)
                {
                    return false;
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine($"Ошибка при валидации MediaInstrument {_ex}");
                return false;
            }
            return true;
        }

        #endregion
    }
}
