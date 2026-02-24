using Google.Cloud.Firestore;
using System.Globalization;


//this class for testing only
namespace chatApp.Controllers.Utils
{
    public class FirebaseUtil
    {
    
        public static bool IsLoggedIn(HttpContext httpContext)
        {
            string? userId = httpContext.Session.GetString("UserId");
            return !string.IsNullOrEmpty(userId);
        }

        public static DocumentReference CurrentUserDetails(FirestoreDb db,string userId)
        {
            return db.Collection("users").Document(userId);
        }

        public static CollectionReference AllUserCollectionReference(FirestoreDb db)
        {
            return db.Collection("users");
        }

        public static DocumentReference GetChatroomReference(FirestoreDb db, string chatroomId)
        {
            return db.Collection("chatrooms").Document(chatroomId);
        }

        public static CollectionReference GetChatroomMessageReference(FirestoreDb db, string chatroomId)
        {
            return GetChatroomReference(db, chatroomId).Collection("chats");
        }


        public static CollectionReference AllChatroomCollectionReference(FirestoreDb db)
        {
            return db.Collection("chatrooms");
        }
        

        public static DocumentReference GetOtherUserFromChatroom(FirestoreDb db, List<string> userIds,string currentUserId)
        {
            string otherUserId = userIds[0] == currentUserId ? userIds[1] : userIds[0];
            return AllUserCollectionReference(db).Document(otherUserId);
        }

        public static string TimestampToString(Timestamp timestamp)
        {
            return timestamp.ToDateTime().ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        
        // Firebase Storage access in .NET is typically done via REST or Admin SDK
        public static string GetCurrentProfilePicStoragePath(string userId)
        {
            return $"profile_pic/{userId}";
        }

        public static string GetOtherProfilePicStoragePath(string otherUserId)
        {
            return $"profile_pic/{otherUserId}";
        }
    }
}