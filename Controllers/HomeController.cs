using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using chatApp.Models;
using System.Text;
using Firebase.Storage;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using chatApp.Services;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
namespace chatApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ICacheService _cacheService;
        private readonly FirestoreService _firestoreService;
        private readonly IImageChecking _imageChecking;
        public HomeController(ICacheService cacheService,
        FirestoreService firestoreService,
        IImageChecking imageChecking)
        {
            _cacheService= cacheService;
            _firestoreService = firestoreService;
            _imageChecking = imageChecking;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sessionCookie = Request.Cookies["session"];
            if (string.IsNullOrEmpty(sessionCookie))
            {
                // Session cookie is not available. Force user to login.
                return Redirect("/Login/UserLogin");
            }

            try
            {
                // Verify the session cookie. In this case an additional check is added to detect
                // if the user's Firebase session was revoked, user deleted/disabled, etc.
                var checkRevoked = true;
                // 1. Verify Firebase ID token
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifySessionCookieAsync(
                    sessionCookie, checkRevoked);
                ViewBag.CurrentUserId = decodedToken.Uid;
                Console.WriteLine($"Current user Id: {decodedToken.Uid}");
                HttpContext.Session.SetString("UserId", decodedToken.Uid);
                return View("Index");
            }
            catch (FirebaseAuthException)
            {
                // Session cookie is invalid or revoked. Force user to login.
                return Redirect("/Login/UserLogin");
            }
        }
        // find room when know userId
        private async Task<bool> UserCanAccessRoom(
            string userId,
            string chatRoomId)
        {   // if userId is yourshelf, do test 1
            // else do test 2
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
        // find roomID
        private async Task<(bool Accessible, string? RoomId)> CheckMember(
        string OtherId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var collectionRef = _firestoreService._db.Collection("users").Document(userId)
                            .Collection("joinedchatrooms");
            Query query = collectionRef.WhereEqualTo("otherUserId", OtherId);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count > 0)
            {
                string? rId = snapshot.Documents.FirstOrDefault()?.Id;
                return (true, rId);
            }
            else
            {
                return (false, null);
            }
        }
        [HttpGet]
        public async Task<IActionResult> JoinedChatrooms()
        {
            //string? userId = HttpContext.Session.GetString("UserId");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var userRef = _firestoreService._db.Collection("users").Document(userId)
                            .Collection("joinedchatrooms");
                // discard limit for correct logic of searchUser method
                Query query = userRef.Limit(10);
                QuerySnapshot chatroomSnapshot = await query.GetSnapshotAsync();  
                if (chatroomSnapshot.Documents.Count > 0)
                {
                    var chatroomIds = chatroomSnapshot.Documents.Select(
                    doc => new JoinedRoomModel
                    {
                        roomId = doc.Id,
                        otherUserId = doc.GetValue<string>("otherUserId")
                    }).ToList();
                    return Json(chatroomIds);
                
                }
                // If no chatrooms found or an exception occurred,
                //  return an empty result or appropriate response
                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new List<object>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetInfor(string userId)
        {
            try
            {
                var userRef = _firestoreService._db.Collection("users")
                            .Document(userId);
                var snapshot = await userRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var user =  new UserModel
                    {
                        userId = snapshot.Id,
                        avatar = snapshot.GetValue<string>("avatar"),
                        userName = snapshot.GetValue<string>("userName")
                    };
                    return Json(user);
                
                }
                // If no user found or an exception occurred,
                //  return an empty result or appropriate response
                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Json(new List<object>());
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> SearchUser(string userName)
        {
            try
            {
                // Reference to the subcollection
                CollectionReference usersRef = _firestoreService._db.Collection("users");
                Query userQuery = usersRef.WhereEqualTo("userName", userName);                                        
                QuerySnapshot userQuerySnapshot = await userQuery.GetSnapshotAsync();
                if (userQuerySnapshot.Documents.Count > 0)
                {
                    var user = userQuerySnapshot.Documents.Select(doc => new UserModel
                    {
                        userId = doc.Id,
                        avatar = doc.GetValue<string>("avatar"),
                        userName =doc.GetValue<string>("userName")
                    }).ToList();
                    return Json(user);
                
                }
                // If no user found or an exception occurred
                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new List<object>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetMessages(string chatroomId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            if (!await UserCanAccessRoom(userId, chatroomId))
                return StatusCode(StatusCodes.Status403Forbidden);
            try
            {
                var chatroomRef = _firestoreService._db.Collection("chatrooms")
                            .Document(chatroomId)
                            .Collection("chats");
                var query = chatroomRef.OrderByDescending("Timestamp").Limit(10);
                var snapshot = await query.GetSnapshotAsync();
                var messages = snapshot.Documents.Select(doc => new MessageModel
                {
                    Message = doc.GetValue<string>("Message"),
                    SenderId = doc.GetValue<string>("SenderId"),
                    Timestamp = doc.GetValue<Timestamp>("Timestamp").ToDateTime(),
                    type = doc.GetValue<string>("type")
                }).Reverse().ToList();
                return Json(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Json(new List<object>());
            }
            
        }
        /*
            should check data type, never trust clients
        */
        [HttpGet]
        public async Task<IActionResult> GetMessagesPage(string chatroomId, int displayedMessage)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            if (!await UserCanAccessRoom(userId, chatroomId))
                return StatusCode(StatusCodes.Status403Forbidden);
            try
            {
                var chatroomRef = _firestoreService._db.Collection("chatrooms")
                            .Document(chatroomId)
                            .Collection("chats");
                var firstQuery = chatroomRef.OrderByDescending("Timestamp").Limit(displayedMessage);
                var firstSnapshot = await firstQuery.GetSnapshotAsync();
                // Get last DocumentSnapshot
                DocumentSnapshot lastDoc = firstSnapshot.Documents.Last();
                // Create second page
                Query secondQuery = chatroomRef
                .OrderByDescending("Timestamp")
                .StartAfter(lastDoc);
                QuerySnapshot secondSnapshot = await secondQuery.GetSnapshotAsync();
                var messages = secondSnapshot.Documents.Select(doc => new MessageModel
                {
                    Message = doc.GetValue<string>("Message"),
                    SenderId = doc.GetValue<string>("SenderId"),
                    // 
                    Timestamp = doc.GetValue<Timestamp>("Timestamp").ToDateTime(),
                    type = doc.GetValue<string>("type")
                }).ToList();
                return Json(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred when get older msgs: {ex.Message}");
                return Json(new List<object>());
            }
            
        }

        public string GetRandomChatroomId()
        {
            var chatroomRef = _firestoreService._db.Collection("chatrooms").Document();
            return chatroomRef.Id;
        }
        
        [HttpPost]
        public async Task CreateChatroom(string userId,string otherId, string randomRoomId)
        {   
            var userIds = new List<string> { userId, otherId };
            var chatroomRef = _firestoreService._db.Collection("chatrooms").Document(randomRoomId);

            // Check if the chatroom already exists
            var snapshot = await chatroomRef.GetSnapshotAsync();

            Dictionary<string, object> chatroom = new Dictionary<string, object>
            {
                { "UserIds", userIds }
            };

            await chatroomRef.SetAsync(chatroom);
        }
        [HttpPost]
        public async Task AddToJoinedChatroom(string userId, string chatroomId, string otherUserId)
        {

            var chatroomRef = _firestoreService._db.Collection("users").Document(userId)
                .Collection("joinedchatrooms").Document(chatroomId);

            // Check if the chatroom already exists
            var snapshot = await chatroomRef.GetSnapshotAsync();
            Dictionary<string, object> joinedRoomInfor = new Dictionary<string, object>
            {
                { "LastMessageTime", Timestamp.GetCurrentTimestamp()}, 
                { "otherUserId", otherUserId}
            };
            await chatroomRef.SetAsync(joinedRoomInfor);
        }

        [HttpPost]
        public async Task<IActionResult> EstablishChat(string otherId)
        {
            // new Task = CreateRoom +2 AddToJoinedChatroom
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            // replace with CanUser...
            var t1 = CheckMember(otherId);
            var t2 = CheckMember(userId);
            await Task.WhenAll(t1,t2);
            
            bool check1 =t1.Result.Accessible; // B in A chat
            bool check2 =t1.Result.Accessible; // A in B chat
            string roomId = GetRandomChatroomId();
            try{
                if (!check1&&!check2)
                {
                    var t4 = CreateChatroom(userId,otherId,roomId);
                    var t5 = AddToJoinedChatroom(userId,roomId,otherId);
                    var t6 = AddToJoinedChatroom(otherId,roomId,userId);
                    await Task.WhenAll(t4,t5,t6);
                    _cacheService.Add(userId,roomId,otherId);
                    return Ok(new{roomId});
                }
                else if(!check1&&check2)
                {
                    string? rId1 =t2.Result.RoomId;
                    if (rId1 != null)
                    {
                        await AddToJoinedChatroom(userId,rId1,otherId);
                        _cacheService.Add(userId,rId1,otherId);
                    }
                    return Ok();
                }
                else if(check1&&!check2)
                {
                    string? rId2 =t1.Result.RoomId;
                    if (rId2 != null)
                    {
                        await AddToJoinedChatroom(otherId,rId2,userId);
                    }
                    return Ok();
                }
                else
                {
                    return Ok("Room Exist");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error Establish Room: {ex.Message}");
                return Ok("Can't establish room"); 
            }

        }

        [HttpPost]
        public async Task AddMessage(string chatroomId, string senderId, string message, string type)
        {
            try
            {
                var chatRef = _firestoreService._db.Collection("chatrooms")
                            .Document(chatroomId)
                            .Collection("chats");

                var timestamp = Timestamp.GetCurrentTimestamp();

                var newMessage = new MessageModel
                {
                    Message = message,
                    SenderId = senderId,
                    Timestamp = timestamp.ToDateTime(),
                    type = type

                };

                await chatRef.AddAsync(newMessage);

                // Optionally update last message in the parent document
                // using batch update:users/{chatroomId}/joinedchatrooms
                //  collection and chats collection
                
                /*
                var secondChatRef = _firestoreService._db.Collection("chatrooms")
                        .Document(chatroomId);
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "lastMessage", message },
                    { "lastMessageTime", timestamp },
                    { "lastMessageSenderId", senderId },

                };
                await secondChatRef.UpdateAsync(updates);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage2Db([FromBody] MessageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.chatroomId) ||
                string.IsNullOrWhiteSpace(dto.message))
            {
                return BadRequest("Invalid data.");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            if (!await UserCanAccessRoom(userId, dto.chatroomId))
                return StatusCode(StatusCodes.Status403Forbidden);
            try
            {
                await AddMessage(dto.chatroomId, userId, dto.message, dto.type);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        
        (bool isValid, Stream? imgStream) ValidateAndSaveImage(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false,null);

            if (!_imageChecking.HasImageSignature(file))
                return (false,null);

            if (!_imageChecking.IsValidImage(file, out var img))
                return (false,null);

            using (img)
            {
                Stream imgStream = _imageChecking.SaveSanitizedImage(img);
                return (true,imgStream);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(
         IFormFile image,
         string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return BadRequest("Group ID is not provided.");
            }
            var firebaseToken = HttpContext.Session.GetString("FirebaseToken");

            if (string.IsNullOrEmpty(firebaseToken))
            {
                // Handle case where token is not found (e.g., user not logged in)
                throw new InvalidOperationException("Firebase token is not available in session.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            if (!await UserCanAccessRoom(userId, groupId))
                return StatusCode(StatusCodes.Status403Forbidden);
            // Construct FirebaseStorage
            var extension = Path.GetExtension(image.FileName);
            var uniqueFileName = Guid.NewGuid()+ extension;

            var (isValid, imgStream) = ValidateAndSaveImage(image);
            if (!isValid)
            {
                return BadRequest("image is not valid.");
            }
            using var ms = imgStream;
            ms.Position = 0; // move pointer to the begin of Stream instead of EOF
            try
            {
                var task = new FirebaseStorage(
                "your-firebase-storage-bucket.appspot.com",
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(firebaseToken),
                    ThrowOnCancel = true,
                })
                .Child("images")
                .Child(uniqueFileName)
                .PutAsync(ms);

                // await the task to wait until upload completes and get the download url
                var downloadUrl = await task;
                Console.WriteLine(downloadUrl);
                /*
                await _hubContext.Clients.Group(groupId)
                .SendAsync("ReceiveImage", senderId, downloadUrl);
                */
                await AddMessage(groupId, userId, downloadUrl, "image");
                return Ok(new { url = downloadUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while uploading the image: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // For testing: send an image from client to server
        // It's not used at present.
        [HttpPost]
        public async Task<IActionResult> ReceiveImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No file uploaded.");
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploads);
            var extension = Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploads, Guid.NewGuid() + extension);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return Ok(new { filePath });
        }

        [HttpPost]
        public static async Task DeleteCollection(CollectionReference collectionReference, int batchSize)
        {
            QuerySnapshot snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
            IReadOnlyList<DocumentSnapshot> documents = snapshot.Documents;
            while (documents.Count > 0)
            {
                foreach (DocumentSnapshot document in documents)
                {
                    Console.WriteLine("Deleting document {0}", document.Id);
                    await document.Reference.DeleteAsync();
                }
                snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
                documents = snapshot.Documents;
            }
            Console.WriteLine("Finished deleting all documents from the collection.");
        }

        [HttpPost]
        // new Task = 2 LeaveChatroom + DeleteChatroom (consider this)
        public async Task<IActionResult> LeaveChatroom(string chatroomId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            if (!await UserCanAccessRoom(userId, chatroomId))
                return StatusCode(StatusCodes.Status403Forbidden);
            try
            {
                var chatroomRef = _firestoreService._db.Collection("users").Document(userId)
                .Collection("joinedchatrooms").Document(chatroomId);
                await chatroomRef.DeleteAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while removing joined room: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChatroom(string chatroomId)
        {
            try
            {
                var chatroomRef = _firestoreService._db.Collection("chatrooms").Document(chatroomId);
                var chatsRef = chatroomRef.Collection("chats");
                await DeleteCollection(chatsRef, 10);
                await chatroomRef.DeleteAsync();
                return Ok("Chatroom deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the chatroom: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}