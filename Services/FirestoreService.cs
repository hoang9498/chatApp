using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Grpc.Auth; // Required for UseCredential
public class FirestoreService
{
    public FirestoreDb _db;

    public FirestoreService(IConfiguration config)
    {
        var path = config["Firebase:ServiceAccountPath"];
        var projectId = config["Firebase:ProjectId"];

        var credential = GoogleCredential.FromFile(path);

        // Initialize FirebaseApp (required by Admin SDK)
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
                ProjectId = projectId
            });
        }
        // Initialize Firestore explicitly — this is CRUCIAL!
        var builder = new Google.Cloud.Firestore.V1.FirestoreClientBuilder
        {
            Credential = credential
        };

        var client = builder.Build();
        _db = FirestoreDb.Create(projectId, client);
    }
    //check in database
    // find (userId, chatroomId) is exist or not 
    public async Task<(bool Accessible, string OtherId)> FindRoomId(string userId, string chatRoomId)
    {
        try
        {
            var userRef = _db.Collection("users").Document(userId)
                            .Collection("joinedchatrooms").Document(chatRoomId);
            var snapshot = await userRef.GetSnapshotAsync();  
            if (snapshot.Exists)
            {
                bool acc =true;
                string OId = snapshot.GetValue<string>("otherUserId");
                return (acc,OId);
            
            }
            // If no chatrooms found
            return (false,"none");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return (false,"none");;
        }
    }

}
