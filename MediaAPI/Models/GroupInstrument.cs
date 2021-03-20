using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class GroupInstrument
    {
        public int GroupInstrumentId { get; set; }
        public int GroupId { get; set; }
        public Instrument Instrument { get; set; }

        [JsonIgnore]
        public virtual Group Group { get; set; }

        #region validation

        public static bool ValidateModel(GroupInstrument _data)
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
                Debug.WriteLine($"Ошибка при валидации GroupInstrument {_ex}");
                return false;
            }
            return true;
        }

        #endregion
    }
}
