using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Server.Models {
    public enum MpUserState {
        None = 0,
        PrePending,
        Pending,
        Active,
        Inactive,
        Reset,
        Deactivated
    }

    public class MpUser {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]        
        [StringLength(maximumLength: 18, MinimumLength = 6)]
        public string Password { get; set; }

        public int UserStateTypeId { get; set; } = 0;
    }
}
