using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace chatApp.Models
{
    public class JoinedRoomModel
    {
        [FirestoreProperty]
        public string? roomId { get; set; }

        [FirestoreProperty]
        public string? otherUserId { get; set; }
    }
}