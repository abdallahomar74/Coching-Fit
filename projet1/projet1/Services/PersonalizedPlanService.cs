using projet1.Data;
using projet1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace projet1.Services
{
    public class PersonalizedPlanService : IPersonalizedPlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoogleDriveService _googleDriveService;

        public PersonalizedPlanService(ApplicationDbContext context, IGoogleDriveService googleDriveService)
        {
            _context = context;
            _googleDriveService = googleDriveService;
        }

        public async Task<object> AddPersonalizedPlanAsync(ClaimsPrincipal user, [FromQuery] string subscriberUserName, [FromForm] IFormFile file)
        {
            try
            {
                var coachId = user.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(coachId))
                    throw new UnauthorizedAccessException("This Coach is Unauthorized");

                var subscriber = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == subscriberUserName);
                if (subscriber == null)
                    throw new Exception("Subscriber not found.");

                var sub = await _context.coachsubscriptions
                    .Include(cs => cs.SubscriptionPlan)
                    .FirstOrDefaultAsync(cs => cs.SubscriberId == subscriber.Id
                        && cs.SubscriptionPlan.CoachId == coachId
                        && cs.ExpirationDate > DateTime.UtcNow);
                if (sub == null)
                    throw new Exception("Invalid or expired subscription.");

                // التحقق من نوع الملف
                var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedTypes.Contains(fileExtension))
                    throw new Exception("File type not allowed.");

                // التحقق من حجم الملف (5MB maximum)
                if (file.Length > 5 * 1024 * 1024)
                    throw new Exception("File size too large. Maximum 5MB allowed.");

                // رفع الملف لـ Google Drive
                var (fileId, viewUrl, downloadUrl) = await _googleDriveService.UploadFileAsync(file);

                var plan = new PersonalizedPlan
                {
                    CoachSubscriptionId = sub.Id,
                    FileName = file.FileName,
                    GoogleDriveFileId = fileId,
                    GoogleDriveUrl = viewUrl,
                    FileType = fileExtension.TrimStart('.'),
                    FileSize = file.Length,
                    PublishedDate = DateTime.UtcNow
                };

                _context.PersonalizedPlans.Add(plan);
                await _context.SaveChangesAsync();

                return new
                {
                    success = true,
                    message = "File uploaded successfully",
                    data = new
                    {
                        planId = plan.Id,
                        fileName = plan.FileName,
                        fileUrl = viewUrl,
                        downloadUrl = downloadUrl,
                        fileType = plan.FileType,
                        publishedDate = plan.PublishedDate,
                        fileSize = plan.FileSize
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message,
                    error = "UPLOAD_FAILED"
                };
            }
        }

        public async Task<object> GetPlansForSubscriberAsync(ClaimsPrincipal user)
        {
            try
            {
                var subscriberId = user.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(subscriberId))
                    throw new UnauthorizedAccessException("This User is Unauthorized");

                var subs = await _context.coachsubscriptions
                    .Include(cs => cs.SubscriptionPlan)
                    .ThenInclude(sp => sp.Coach)
                    .Where(cs => cs.SubscriberId == subscriberId && cs.ExpirationDate > DateTime.UtcNow)
                    .ToListAsync();

                if (!subs.Any())
                {
                    return new
                    {
                        success = true,
                        data = new List<object>()
                    };
                }

                var subIds = subs.Select(s => s.Id).ToList();

                var plans = await _context.PersonalizedPlans
                    .Include(pp => pp.CoachSubscription)
                    .ThenInclude(cs => cs.SubscriptionPlan)
                    .ThenInclude(sp => sp.Coach)
                    .Where(pp => subIds.Contains(pp.CoachSubscriptionId))
                    .OrderByDescending(pp => pp.PublishedDate)
                    .ToListAsync();

                var result = plans.Select(plan => new
                {
                    planId = plan.Id,
                    fileName = plan.FileName,
                    fileUrl = plan.GoogleDriveUrl,
                    downloadUrl = _googleDriveService.GetFileDownloadUrlAsync(plan.GoogleDriveFileId).Result,
                    fileType = plan.FileType,
                    publishedDate = plan.PublishedDate,
                    fileSize = plan.FileSize,
                    coachName = plan.CoachSubscription.SubscriptionPlan.Coach.FullName
                }).ToList();

                return new
                {
                    success = true,
                    data = result
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message,
                    error = "FETCH_FAILED"
                };
            }
        }

        public async Task<object> GetPlanDownloadUrlAsync(ClaimsPrincipal user,[FromQuery] int planId)
        {
            try
            {
                var userId = user.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is Unauthorized");

                var plan = await _context.PersonalizedPlans
                    .Include(pp => pp.CoachSubscription)
                    .FirstOrDefaultAsync(pp => pp.Id == planId);

                if (plan == null)
                    throw new Exception("Plan not found.");

                // التحقق من أن المستخدم مشترك في هذا المدرب
                var hasAccess = await _context.coachsubscriptions
                    .AnyAsync(cs => cs.Id == plan.CoachSubscriptionId
                                && cs.SubscriberId == userId
                                && cs.ExpirationDate > DateTime.UtcNow);

                if (!hasAccess)
                    throw new UnauthorizedAccessException("Access denied.");

                var downloadUrl = await _googleDriveService.GetFileDownloadUrlAsync(plan.GoogleDriveFileId);

                return new
                {
                    success = true,
                    data = new
                    {
                        downloadUrl = downloadUrl,
                        fileName = plan.FileName,
                        fileType = plan.FileType,
                        fileSize = plan.FileSize,
                        expiresAt = DateTime.UtcNow.AddHours(1) // الرابط يعمل لمدة ساعة
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message,
                    error = "DOWNLOAD_FAILED"
                };
            }
        }
    }
}