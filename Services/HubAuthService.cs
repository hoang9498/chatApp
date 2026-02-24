using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatApp.Services
{
    public class HubAuthService: IHubAuthService
    {
        private readonly ICacheService _cacheService;
        private readonly FirestoreService _firestoreService;
        public HubAuthService(ICacheService cacheService,
        FirestoreService firestoreService)
        {
            _cacheService= cacheService;
            _firestoreService = firestoreService;
        }
        public async Task<bool> UserCanAccessRoom(
            string userId,
            string chatRoomId)
        {
            if (_cacheService.ContainsKey(userId, chatRoomId))
                return true;

            var existsInDb =
                await _firestoreService.FindRoomId(userId, chatRoomId);

            if (existsInDb.Accessible)
            {
                _cacheService.Add(userId, chatRoomId,existsInDb.OtherId);
                return true;
            }

            return false;
        }
    }
}