﻿using EmployeeManagement.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Remote(action:"IsEmailInUse",controller:"Account")]
        //[ValidEmailDomain(allowedDomain:"sauravk.com",ErrorMessage = "Email domain must be sauravk.com")]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name ="Confirm Passoword")]
        [Compare("Password",ErrorMessage ="Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; }
        public string City { get; set; }
    }
}
