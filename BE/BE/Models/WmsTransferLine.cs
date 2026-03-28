using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class WmsTransferLine
{
    public int TransLineId { get; set; }

    public int? TransferId { get; set; }

    public int? VariantId { get; set; }

    public int? Qty { get; set; }

    public virtual WmsTransfer? Transfer { get; set; }

    public virtual ItmVariant? Variant { get; set; }
}
