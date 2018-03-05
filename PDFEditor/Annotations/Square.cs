using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kahua.kdk.property;

namespace PDFEditorNS
{
    public class Square : BaseAnnotation
    {
        internal static class Names
        {
            public const string Square = "SquareAnnotation";
        }

        #region Constructors
        public Square()
            : base(Names.Square)
        {

        }

        public Square(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}
