using DonationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Entities
{
    public class ApplicationUser : Entity
    {

        public string Email { get; private set; } = null!;

        public string UserName { get; private set; } = null!;

        public string FullName { get; private set; } = null!;

        public string PreferredLanguage { get; private set; } = "en";

        public bool IsActive { get; private set; } = true;


        //navigation properties
        public ICollection<Campaign> Campaigns { get; private set; } = new List<Campaign>();
        public ICollection<Donation> Donations { get; private set; } = new List<Donation>();
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        
        private ApplicationUser() { }

        private ApplicationUser(Guid id, string email,string userName,string fullName,string preferredLanguage,DateTime createdAtUtc ) : base(id, createdAtUtc)
        {
            Email = email;
            UserName = userName;
            FullName = fullName;
            PreferredLanguage = preferredLanguage;
        }

        public static ApplicationUser Create(Guid id, string email,string userName,string fullName,string preferredLanguage,DateTime createdAtUtc) 
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username is required.");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.");

            if (preferredLanguage != "en" && preferredLanguage != "ar")
                throw new ArgumentException("Language must be en or ar.");

            return new ApplicationUser(id,email,userName,fullName,preferredLanguage,createdAtUtc);
        }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void ChangePreferredLanguage(string language)
    {
        if (language != "en" && language != "ar")
            throw new ArgumentException("Language must be en or ar.");

        PreferredLanguage = language;
    }
    }
}
