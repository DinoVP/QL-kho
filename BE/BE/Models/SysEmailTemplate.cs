using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SysEmailTemplate
{
    public int TemplateId { get; set; }

    public string? Code { get; set; }

    public string? Body { get; set; }

    public int? CreatedBy { get; set; }

    public virtual SysUser? CreatedByNavigation { get; set; }
}
