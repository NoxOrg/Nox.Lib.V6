using ETLBox.DataFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData
{
    public class MetaBase : IModelBase
    {
        public int Id { get; set; }

        [NotMapped]
        public string DefinitionFileName { get; set; } = String.Empty;
    }
}
