using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class GroupMember
    {
        public int GroupMemberId { get; set; }
        public int? GroupId { get; set; }
        public int? MemberId { get; set; }
        public Role Role { get; set; }

        [JsonIgnore]
        public virtual Group Group { get; set; }
        public virtual User Member { get; set; }

        #region validation

        public static bool ValidateModel(GroupMember _data)
        {
            try
            {
                if (_data == null ||
                    _data.MemberId == null)
                {
                    return false;
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine($"Ошибка при валидации GroupMember {_ex}");
                return false;
            }
            return true;
        }

        #endregion
    }
}
