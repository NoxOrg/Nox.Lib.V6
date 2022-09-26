using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class Property
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsAutoNumber { get; set; } = false;
        public bool IsForeignKey { get; set; } = false;
        public bool IsRequired { get; set; } = false;
        public bool IsUnicode { get; set; } = true;
        public bool CanFilter { get; set; } = false;
        public bool CanSort { get; set; } = false;
        public int MinWidth { get; set; } = 0;
        public int MaxWidth { get; set; } = 512;
        public int MinValue { get; set; } = int.MinValue;
        public int MaxValue { get; set; } = int.MaxValue;
        public object Default { get; set; } = string.Empty;


    }
}
