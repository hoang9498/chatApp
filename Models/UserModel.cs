using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace chatApp.Models
{
    [FirestoreData]
    public class UserModel
    {
        public UserModel() { }
        [FirestoreProperty]
        public String? userId { set; get; }
        [FirestoreProperty]
        public String? avatar { set; get; }
        [FirestoreProperty]
        public String? userName { set; get; }
    }
}