using System;
using System.Collections.Generic;
using System.Text;

namespace UnivesalProjectGenerator.Models
{
    public class TableDetails
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
    }
}
