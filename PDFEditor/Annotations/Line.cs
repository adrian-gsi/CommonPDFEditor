using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kahua.kdk.property;

namespace PDFEditorNS
{
    public class Line : BaseAnnotation
    {
        internal static class Names
        {
            public const string Line = "LineAnnotation";
            public const string XStart = "XStart";
            public const string YStart = "YStart";
            public const string XEnd = "XEnd";
            public const string YEnd = "YEnd";
        }

        #region Constructors
        public Line()
            : base(Names.Line)
        {

        }

        public Line(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors

        public double XStart() { return Properties.Double(Names.XStart); }
        public Line XStart(double xStart) { Properties.Double(Names.XStart, xStart); return this; }

        public double YStart() { return Properties.Double(Names.YStart); }
        public Line YStart(double yStart) { Properties.Double(Names.YStart, yStart); return this; }

        public double XEnd() { return Properties.Double(Names.XEnd); }
        public Line XEnd(double xEnd) { Properties.Double(Names.XEnd, xEnd); return this; }

        public double YEnd() { return Properties.Double(Names.YEnd); }
        public Line YEnd(double yEnd) { Properties.Double(Names.YEnd, yEnd); return this; }
    }
}
