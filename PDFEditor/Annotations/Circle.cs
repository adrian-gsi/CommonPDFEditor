using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kahua.kdk.property;

namespace PDFEditorNS
{
    public class Circle : BaseAnnotation
    {
        internal static class Names
        {
            public const string Circle = "CircleAnnotation";
        }

        #region Constructors
        public Circle()
            : base(Names.Circle)
        {

        }

        public Circle(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}
