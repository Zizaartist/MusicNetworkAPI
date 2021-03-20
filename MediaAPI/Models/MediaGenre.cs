using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class MediaGenre
    {
        public int MediaGenreId { get; set; }
        public int MediaId { get; set; }
        public Genre Genre { get; set; }

        [JsonIgnore]
        public virtual MediaFile Media { get; set; }

        #region validation

        public static bool ValidateModel(MediaGenre _data)
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
                Debug.WriteLine($"Ошибка при валидации MediaGenre {_ex}");
                return false;
            }
            return true;
        }

        #endregion
    }
}
