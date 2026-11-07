using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Identity
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string CampaignManager = "CampaignManager";

        public const string Donor = "Donor";
        
        public static readonly string[] AllRoles = 
        {
            Admin,
            CampaignManager,
            Donor
        };
    }
}
