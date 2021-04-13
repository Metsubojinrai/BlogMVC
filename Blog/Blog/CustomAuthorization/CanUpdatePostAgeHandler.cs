using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.CustomAuthorization
{
    public class CanUpdatePostAgeHandler : AuthorizationHandler<CanUpdatePostRequirement, Post>
    {
        private readonly ILogger<MinimumAgeHandler> _logger;
        private readonly UserManager<User> _userManager;
        public CanUpdatePostAgeHandler(ILogger<MinimumAgeHandler> logger,
        UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        protected Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                           MinimumAgeRequirement requirement)
        {
            var user = _userManager.GetUserAsync(context.User).Result;
            if (user == null)
                return Task.CompletedTask;

            var dateOfBirth = user.Birthday;
            if (dateOfBirth == null)
            {
                _logger.LogInformation("Không có ngày sinh");
                // Trả về mà chưa chứng thực thành công
                return Task.CompletedTask;
            }

            int calculatedAge = DateTime.Today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-calculatedAge))
            {
                calculatedAge--;
            }

            if (calculatedAge < requirement.MinimumAge)
            {
                _logger.LogInformation(calculatedAge + ": Không đủ tuổi truy cập");
                return Task.CompletedTask;
            }

            TimeSpan start = new(requirement.OpenTime, 0, 0);
            TimeSpan end = new(requirement.CloseTime, 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;
            // see if start comes before end
            if (start < end)
                if (!(start <= now && now <= end))
                {
                    _logger.LogInformation(now.ToString() + " Không trong khung giờ được truy cập");
                    return Task.CompletedTask;
                }
            // start is after end, so do the inverse comparison
            if (end < now && now < start)
            {
                _logger.LogInformation(now.ToString() + " Không trong khung giờ được truy cập");
                return Task.CompletedTask;
            }

            // Thiết lập chứng thực thành công
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanUpdatePostRequirement requirement, Post resource)
        {
            if (requirement.AdminCanUpdate)
            {
                if(context.User.IsInRole("Admin"))
                {
                    //Cho phép
                    _logger.LogInformation("Admin được cập nhật");
                    return Task.CompletedTask;
                }
            }

            if(context.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogInformation("User không đăng nhập");
                return Task.CompletedTask;
            }

            if (requirement.OwnerCanUpdate)
            {
                var user = _userManager.GetUserAsync(context.User).Result;
                if(user.Id == resource.AuthorId)
                {
                    _logger.LogInformation("Được phép cập nhật");
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
            _logger.LogInformation("Không được phép cập nhật");
            return Task.CompletedTask;
        }
        
    }
}