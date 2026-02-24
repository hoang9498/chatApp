using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace chatApp.Models
{
    [FirestoreData]
    public class ChatroomModel
    {
        public ChatroomModel() { }

        [FirestoreProperty]
        public List<String>? UserIds { set; get; }
        [FirestoreProperty]
        public string? LastMessage { get; set; }
        [FirestoreProperty]
        public Timestamp LastMessageTime { get; set; }
        [FirestoreProperty]
        public string? LastMessageSenderId { get; set; }

    }
}