using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    [Table("Post")]
    public class Post : PostBase
    {
        [Required]
        [Display(Name = "ID Tác giả")]
        public Guid AuthorId { set; get; }
        [ForeignKey("AuthorId")]

        [Display(Name = "Tác giả")]
        public User Author { set; get; }

        [Display(Name = "Ngày tạo")]
        public DateTime DateCreated { set; get; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime DateUpdated { set; get; }
    }
}
