﻿using Microsoft.AspNetCore.Identity;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Convience.Entity.Data
{
    public class SystemUser : IdentityUser<int>
    {
        public string Avatar { get; set; }

        public Sex Sex { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedTime { get; set; }

        [NotMapped]
        public string RoleIds { get; set; }

        [NotMapped]
        public string PositionIds { get; set; }

        [NotMapped]
        public string DepartmentId { get; set; }
    }

    public enum Sex
    {
        Unknown,
        Male,
        Famale
    }
}
