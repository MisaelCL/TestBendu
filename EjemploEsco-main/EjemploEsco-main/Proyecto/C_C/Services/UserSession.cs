using System;

namespace C_C.Services
{
    public class UserSession
    {
        private static readonly Lazy<UserSession> LazyInstance = new Lazy<UserSession>(() => new UserSession());

        private UserSession()
        {
        }

        public static UserSession Instance => LazyInstance.Value;

        public Guid CurrentUserId { get; set; }
    }
}
