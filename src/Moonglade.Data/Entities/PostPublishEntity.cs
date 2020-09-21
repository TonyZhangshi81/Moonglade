﻿using System;

namespace Moonglade.Data.Entities
{
    public class PostPublishEntity
    {
        public Guid PostId { get; set; }
        public bool IsPublished { get; set; }
        public bool ExposedToSiteMap { get; set; }
        public bool IsFeedIncluded { get; set; }
        public DateTime? LastModifiedUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? PubDateUtc { get; set; }

        public virtual PostEntity Post { get; set; }
    }
}
