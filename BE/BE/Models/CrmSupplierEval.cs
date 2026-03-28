using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CrmSupplierEval
{
    public int EvalId { get; set; }

    public int? SupplierId { get; set; }

    public int? EvaluatorId { get; set; }

    public int? Score { get; set; }

    public virtual HrmEmployee? Evaluator { get; set; }

    public virtual CrmPartner? Supplier { get; set; }
}
