using MediaAPI.Controllers.FrequentlyUsed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MediaAPI.Models
{
    public partial class Group
    {
        public Group()
        {
            GroupGenres = new HashSet<GroupGenre>();
            GroupInstruments = new HashSet<GroupInstrument>();
            GroupMembers = new HashSet<GroupMember>();
        }

        //Required
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int OwnerId { get; set; }

        //Nullable
        public string Description { get; set; }
        public string ImagePath { get; set; }

        public virtual User Owner { get; set; }
        public virtual ICollection<GroupGenre> GroupGenres { get; set; }
        public virtual ICollection<GroupInstrument> GroupInstruments { get; set; }
        public virtual ICollection<GroupMember> GroupMembers { get; set; }

        #region validation

        public static bool ValidateModel(Group _data)
        {
            try
            {
                if (_data == null ||
                    string.IsNullOrEmpty(_data.GroupName) ||
                    !ValidateGroupGenres(_data.GroupGenres) ||
                    !ValidateGroupInstruments(_data.GroupInstruments))
                {
                    return false;
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine($"Ошибка при валидации Group {_ex}");
                return false;
            }
            return true;
        }

        private static bool ValidateGroupInstruments(ICollection<GroupInstrument> _instruments) 
        {
            if (_instruments == null ||
                !_instruments.Any()) 
            {
                return false;
            }
            foreach (var instrument in _instruments) 
            {
                if (!GroupInstrument.ValidateModel(instrument)) 
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateGroupGenres(ICollection<GroupGenre> _genres)
        {
            if (_genres == null ||
                !_genres.Any())
            {
                return false;
            }
            foreach (var genre in _genres)
            {
                if (!GroupGenre.ValidateModel(genre))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

    }
}
