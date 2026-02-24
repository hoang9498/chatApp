using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace chatApp.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public required string EmailAddress { get; set; }

        [Required]  
        public required string Password { get; set; } 
    }
}