using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using projet1.Data;
using projet1.Data.Models;

namespace projet1.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        public ChatHub(ApplicationDbContext ctx) => _context = ctx;

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("uid")?.Value
                         ?? throw new HubException("Unauthenticated");

            // If coach, join all subscription groups where he is the coach
            var coachSubs = await _context.coachsubscriptions
                .Where(cs => cs.SubscriptionPlan.CoachId == userId
                          && cs.ExpirationDate > DateTime.UtcNow)
                .Select(cs => cs.Id)
                .ToListAsync();

            if (coachSubs.Any())
            {
                foreach (var subId in coachSubs)
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"sub_{subId}");
            }
            else
            {
                // Otherwise treat as subscriber, join his single active subscription
                var subId = await _context.coachsubscriptions
                    .Where(cs => cs.SubscriberId == userId && cs.ExpirationDate > DateTime.UtcNow)
                    .Select(cs => cs.Id)
                    .FirstOrDefaultAsync();

                if (subId == 0)
                    throw new HubException("No active subscription");

                await Groups.AddToGroupAsync(Context.ConnectionId, $"sub_{subId}");
            }

            await base.OnConnectedAsync();
        }

        public async Task SendText(int subscriptionId, string text)
        {
            var userId = Context.User.FindFirst("uid")!.Value;

            // authorize user for this subscription
            var isMember = await _context.coachsubscriptions.AnyAsync(cs =>
                cs.Id == subscriptionId
                && (cs.SubscriberId == userId || cs.SubscriptionPlan.CoachId == userId)
                && cs.ExpirationDate > DateTime.UtcNow);

            if (!isMember)
                throw new HubException("Not authorized for this subscription");

            // fetch sender name
            var sender = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.UserName })
                .FirstOrDefaultAsync();
            var senderName = sender?.UserName ?? "";

            var msg = new ChatMessage
            {
                CoachSubscriptionId = subscriptionId,
                SenderId = userId,
                Type = MessageType.Text,
                Text = text
            };
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            await Clients.Group($"sub_{subscriptionId}")
                         .SendAsync("ReceiveMessage", new
                         {
                             id = msg.Id,
                             senderId = userId,
                             senderName = senderName,
                             type = msg.Type,
                             text = msg.Text,
                             sentAt = msg.SentAt
                         });
        }

        public async Task SendAudio(int subscriptionId, string fileRelativePath)
        {
            var userId = Context.User.FindFirst("uid")!.Value;

            var isMember = await _context.coachsubscriptions.AnyAsync(cs =>
                cs.Id == subscriptionId
                && (cs.SubscriberId == userId || cs.SubscriptionPlan.CoachId == userId)
                && cs.ExpirationDate > DateTime.UtcNow);
            if (!isMember)
                throw new HubException("Not authorized for this subscription");

            var sender = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.UserName })
                .FirstOrDefaultAsync();
            var senderName = sender?.UserName ?? "";

            var msg = new ChatMessage
            {
                CoachSubscriptionId = subscriptionId,
                SenderId = userId,
                Type = MessageType.Audio,
                AudioPath = fileRelativePath
            };
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            var url = $"{Context.GetHttpContext()!.Request.Scheme}://{Context.GetHttpContext()!.Request.Host}/{msg.AudioPath}";
            await Clients.Group($"sub_{subscriptionId}")
                         .SendAsync("ReceiveMessage", new
                         {
                             id = msg.Id,
                             senderId = userId,
                             senderName = senderName,
                             type = msg.Type,
                             audioUrl = url,
                             sentAt = msg.SentAt
                         });
        }

        public async Task LoadHistory(int subscriptionId)
        {
            var userId = Context.User.FindFirst("uid")!.Value;
            var isMember = await _context.coachsubscriptions.AnyAsync(cs =>
                cs.Id == subscriptionId
                && (cs.SubscriberId == userId || cs.SubscriptionPlan.CoachId == userId)
                && cs.ExpirationDate > DateTime.UtcNow);
            if (!isMember)
                throw new HubException("Not authorized for this subscription");

            var history = await _context.ChatMessages
                .Where(m => m.CoachSubscriptionId == subscriptionId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            await Clients.Caller.SendAsync("History", history.Select(m => new
            {
                id = m.Id,
                senderId = m.SenderId,
                senderName = _context.Users.Where(u => u.Id == m.SenderId).Select(u => u.UserName).FirstOrDefault(),
                type = m.Type,
                text = m.Text,
                audioUrl = m.Type == MessageType.Audio
                           ? $"{Context.GetHttpContext()!.Request.Scheme}://{Context.GetHttpContext()!.Request.Host}/{m.AudioPath}"
                           : null,
                sentAt = m.SentAt
            }));
        }
    }
}
