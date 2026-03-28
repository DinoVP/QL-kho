using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmContact
{
    public int ContactId { get; set; }

    public int? PartnerId { get; set; }

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public virtual CrmPartner? Partner { get; set; }
}
