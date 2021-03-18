using System;
using System.Collections.Generic;

#nullable disable

namespace MediaAPI.Models
{
    public partial class Subscription
    {
        public int SubscriptionId { get; set; }
        public int? SubscriberId { get; set; }
        public int ProviderId { get; set; }

        public virtual User Provider { get; set; }
        public virtual User Subscriber { get; set; }
    }
}
