using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Models
{
    public class GroupFavourite
    {
        public int GroupFavouriteId { get; set; }
        [ForeignKey("MediaFile")]
        public int? MediaFileId { get; set; }
        [ForeignKey("Group")]
        [Required]
        public int GroupId { get; set; }

        public virtual MediaFile MediaFile { get; set; }
        public virtual Group Group { get; set; }
    }
}
