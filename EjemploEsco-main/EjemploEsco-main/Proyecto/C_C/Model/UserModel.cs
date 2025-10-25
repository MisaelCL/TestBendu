using System;

namespace C_C.Model
{
    public class UserModel
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Salt { get; set; }

        public DateTime RegisteredAt { get; set; }

        public bool IsBlocked { get; set; }
    }
}
