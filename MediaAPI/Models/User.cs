using MediaAPI.Controllers.FrequentlyUsed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class User
    {
        public User()
        {
            Favourites = new HashSet<Favourite>();
            GroupMembers = new HashSet<GroupMember>();
            Groups = new HashSet<Group>();
            MediaFiles = new HashSet<MediaFile>();
            SubscriptionProviders = new HashSet<Subscription>();
            SubscriptionSubscribers = new HashSet<Subscription>();
        }

        //Required
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }

        //Nullable
        public string AvatarPath { get; set; }
        public string Status { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [JsonIgnore]
        public virtual ICollection<Favourite> Favourites { get; set; }
        [JsonIgnore]
        public virtual ICollection<GroupMember> GroupMembers { get; set; }
        [JsonIgnore]
        public virtual ICollection<Group> Groups { get; set; }
        [JsonIgnore]
        public virtual ICollection<MediaFile> MediaFiles { get; set; }
        [JsonIgnore]
        public virtual ICollection<Subscription> SubscriptionProviders { get; set; }
        [JsonIgnore]
        public virtual ICollection<Subscription> SubscriptionSubscribers { get; set; }

        #region validation

        public static bool ValidateModel(User _data)
        {
            try
            {
                if (_data == null ||
                    string.IsNullOrEmpty(_data.UserName) ||
                    string.IsNullOrEmpty(_data.Password) ||
                    !ValidatePhone(_data.Phone) /*||
                    !ValidateDateOfBirth(_data.DateOfBirth)*/
                    )
                {
                    return false;
                }
            }
            catch (Exception _ex) 
            {
                Debug.WriteLine($"Ошибка при валидации User {_ex}");
                return false;
            }
            return true;
        }

        private static bool ValidateDateOfBirth(DateTime _date) => _date > new DateTime(1900, 1, 1);
        private static bool ValidatePhone(string _phone) => Functions.IsPhoneNumber(_phone);

        #endregion

    }
}
