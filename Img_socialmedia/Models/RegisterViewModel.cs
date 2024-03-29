﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Img_socialmedia.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string first_name { get; set; }
        [Required]
        public string last_name { get; set; }
        [Required]
        public string username { get; set; }

        [Required]  
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,15}$")]
        public string password { get; set; }

        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage ="Password confirm does not match.")]
        public string confirm_password { get; set; }

        [Required(ErrorMessage = "The email field is required.")]
        [EmailAddress(ErrorMessage = "The email field is not valid email address")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}")]
        public string email { get; set; }
        public bool RememberMe { get; set; }

    }
}
