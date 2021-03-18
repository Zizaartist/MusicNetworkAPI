using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable

namespace MediaAPI.Models
{
    public partial class GroupGenre
    {
        public int GroupGenreId { get; set; }
        public int GroupId { get; set; }
        public Genre Genre { get; set; }

        public virtual Group Group { get; set; }

        #region validation

        public static bool ValidateModel(GroupGenre _data)
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
                Debug.WriteLine($"Ошибка при валидации GroupGenre {_ex}");
                return false;
            }
            return true;
        }

        #endregion
    }
}
