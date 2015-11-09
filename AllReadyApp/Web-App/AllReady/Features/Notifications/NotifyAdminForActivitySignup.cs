﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllReady.Models;
using MediatR;
using Microsoft.Data.Entity;
using Microsoft.Framework.OptionsModel;

namespace AllReady.Features.Notifications
{
    public class NotifyAdminForActivitySignup : INotificationHandler<VolunteerInformationAdded>
    {
        private readonly AllReadyContext _context;
        private readonly IMediator _bus;
        private readonly IOptions<GeneralSettings> _options;

        public NotifyAdminForActivitySignup(AllReadyContext context, IMediator bus, IOptions<GeneralSettings> options)
        {
            _context = context;
            _bus = bus;
            _options = options;
        }

        public void Handle(VolunteerInformationAdded notification)
        {
            var voluteer = _context.Users.Single(u => u.Id == notification.UserId);
            var activity = _context.Activities.Single(a => a.Id == notification.ActivityId);
            var campaign = _context.Campaigns
                .Include(c => c.Organizer)
                .Single(c => c.Id == activity.CampaignId);
            var link = $"View activity: http://{_options.Value.SiteBaseUrl}/Admin/Activity/Details/{activity.Id}";

            var subject = $"A volunteer has signed up for {activity.Name}";

            if (campaign.Organizer != null)
            {
                var command = new NotifyVolunteersCommand
                {
                    ViewModel = new NotifyVolunteersViewModel
                    {
                        EmailMessage = $"Your {campaign.Name} campaign activity '{activity.Name}' has a new volunteer. {voluteer.UserName} can be reached at {voluteer.Email}. {link}",
                        EmailRecipients = new List<string> { campaign.Organizer.Email },
                        Subject = subject
                    }
                };

                _bus.Send(command);
            }
        }
    }
}
