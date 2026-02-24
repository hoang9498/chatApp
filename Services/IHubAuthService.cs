using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatApp.Services
{
    public interface IHubAuthService
    {
         Task<bool> UserCanAccessRoom(
            string userId,
            string chatRoomId);
    }
}