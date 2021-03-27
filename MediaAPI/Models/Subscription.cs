using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class Subscription
    {
        public int SubscriptionId { get; set; }
        public int? SubscriberId { get; set; }
        public int ProviderId { get; set; }

        [JsonIgnore]
        public virtual User Provider { get; set; }
        [JsonIgnore]
        public virtual User Subscriber { get; set; }
    }
}
