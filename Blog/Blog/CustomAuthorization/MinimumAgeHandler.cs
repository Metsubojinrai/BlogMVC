using Blog.Data;
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
    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        // UserManager được Inject qua khởi tạo
        private readonly UserManager<User> _userManager;
        // Có Inject Logger để ghi log
        private readonly ILogger<MinimumAgeHandler> _logger;
        public MinimumAgeHandler(
        ILogger<MinimumAgeHandler> logger,
        BlogDbContext blogDbContext,
        UserManager<User> userManager)
        {
            _userManager = userManager;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                    MinimumAgeRequirement requirement)
        {
            var user = _userManager.GetUserAsync(context.User).Result;
            if (user == null)
                return Task.CompletedTask;
            var dateOfBirth = user.Birthday;
            if (dateOfBirth == null)
            {
                _logger.LogInformation("Không có ngày sinh");
                //Trả về mà chưa chứng thực thành công
                return Task.CompletedTask;
            }
            int calculateAge = DateTime.Today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth > DateTime.Today.AddYears (-calculateAge))
                calculateAge--;

            if (calculateAge < requirement.MinimumAge)
            {
                _logger.LogInformation("Không đủ tuổi truy cập");
                return Task.CompletedTask;
            }

            TimeSpan start = new TimeSpan(requirement.OpenTime, 0, 0);
            TimeSpan end = new TimeSpan(requirement.CloseTime, 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;

            if(start<end)
                if(!(start<=now && now<=end))
                {
                    _logger.LogInformation(now.ToString() + "không trong khung giờ truy cập");
                    return Task.CompletedTask;
                }

            if (end < now && now < start)
            {
                _logger.LogInformation(now.ToString() + " Không trong khung giờ được truy cập");
                return Task.CompletedTask;
            }

            // Thiết lập chứng thực thành công
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

    }
}