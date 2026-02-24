using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace chatApp.Models
{
    [FirestoreData]
    public class MessageModel
    {
        public MessageModel() { }
        [FirestoreProperty]
        public string? Message { get; set; }
        [FirestoreProperty]
        public string? SenderId { get; set; }
        [FirestoreProperty]
        public DateTime Timestamp { get; set; }
        [FirestoreProperty]
        public string? type { get; set; }
    }
}