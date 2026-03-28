using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SalDeliveryLine
{
    public int DolineId { get; set; }

    public int? Doid { get; set; }

    public int? SolineId { get; set; }

    public int? DeliveredQty { get; set; }

    public virtual SalDelivery? Do { get; set; }

    public virtual SalOrderLine? Soline { get; set; }
}
