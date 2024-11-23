using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MuggedShop.Models
{
    public partial class AdminUser
    {
        [Key]
        [Required]
        [Display(Name = "Admin Email")]
        public string Email { get; set; }

        [Key]
        [Required]
        [Display(Name = "Admin Email")]
        public string PasswordHash { get; set; }
    }
}
