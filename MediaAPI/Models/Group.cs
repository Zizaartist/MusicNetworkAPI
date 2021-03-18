using System;
using System.Collections.Generic;

#nullable disable

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

        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public int OwnerId { get; set; }

        public virtual User Owner { get; set; }
        public virtual ICollection<GroupGenre> GroupGenres { get; set; }
        public virtual ICollection<GroupInstrument> GroupInstruments { get; set; }
        public virtual ICollection<GroupMember> GroupMembers { get; set; }
    }
}
