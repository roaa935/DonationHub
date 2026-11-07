using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; private set; }
        //should br nullable and updated when the entity is updated
        public DateTime ?UpdatedAt { get; private set; } 
        protected Entity() { }
        
        protected Entity (Guid id,DateTime createdAt)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty.");

            Id = id;
            CreatedAt = createdAt;
        }
        protected void UpdateTimestamp(DateTime nowUtc)
        {
            UpdatedAt = nowUtc;
        }
    }
}
