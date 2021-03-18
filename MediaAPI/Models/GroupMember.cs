using MediaAPI.Models.EnumModels;
using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class GroupMember
    {
        public int GroupMemberId { get; set; }
        public int? GroupId { get; set; }
        public int? MemberId { get; set; }
        public Role Role { get; set; }

        public virtual Group Group { get; set; }
        public virtual User Member { get; set; }
    }
}
