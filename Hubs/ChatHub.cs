using System.Security.Claims;
using chatApp.Models;
using chatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IHubAuthService _hubAuthService;

    public ChatHub(ILogger<ChatHub> logger, IHubAuthService hubAuthService)
    {
        _logger = logger;
        _hubAuthService = hubAuthService;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "SignalR connected. UserIdentifier = {UserIdentifier}, IsAuthenticated = {IsAuth}",
            Context.UserIdentifier,
            Context.User?.Identity?.IsAuthenticated
        );

        await base.OnConnectedAsync();
    }
    public async Task SendMessage(string chatroomId, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");
        if(!await _hubAuthService.UserCanAccessRoom(userId,chatroomId))
            throw new HubException("Forbidden");
        await Clients.Group(chatroomId).SendAsync("ReceiveMessage", chatroomId,userId, message);
    }

    public async Task SendImage(string chatroomId, string imageUrl)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");
        if(!await _hubAuthService.UserCanAccessRoom(userId,chatroomId))
            throw new HubException("Forbidden");
        await Clients.Group(chatroomId).SendAsync("ReceiveImage", chatroomId,userId, imageUrl);
    }

    public async Task JoinChatroom(string chatroomId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");
        if(!await _hubAuthService.UserCanAccessRoom(userId,chatroomId))
            throw new HubException("Forbidden");
        await Groups.AddToGroupAsync(Context.ConnectionId, chatroomId);
    }
    //must be run after active chatroom
    public async Task Invite2Chat(string targetUserId, string chatroomId,UserModel other)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");
        if(!await _hubAuthService.UserCanAccessRoom(userId,chatroomId))
            throw new HubException("Forbidden");
        // Tell ALL of Client B's devices to join the group
        // This sends a command to Client B's frontend
        await Clients.User(targetUserId).SendAsync("JoinRoomCommand", chatroomId,other);
    }
    
}