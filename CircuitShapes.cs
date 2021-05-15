using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Linq;

using CircuitPro;

namespace CircuitShapes
{
    abstract class ComponentDraw : Shape
    {
        protected static readonly int unit = 20;
        protected static readonly int offset = unit;
        protected static readonly int width = 5 * unit;
        protected static readonly int height = 2 * unit;

        private readonly bool fillShape;

        public CircuitPro.CircuitModel.Component Component { get; protected set; }

        /// <summary>
        /// Deseneaza un component de circuit
        /// </summary>
        /// <param name="component">Referinta catre componentul care este desenat</param>
        /// <param name="x">Randul din tablou</param>
        /// <param name="y">Coloana din tablou</param>
        /// <param name="fillShape"">Daca figura sa fie umpluta cu transparenta (primeste evenimentul de click)</param>
        protected ComponentDraw(CircuitPro.CircuitModel.Component component, int x = 0, int y = 0, bool fillShape = true)
        {
            // setari desenare
            Stroke = Brushes.Black;
            StrokeThickness = 1;
            Fill = Brushes.Transparent;

            // setare informatii
            this.fillShape = fillShape;
            Component = component;

            // setare pozitie in canvas
            Canvas.SetTop(this, offset + height * x);
            Canvas.SetLeft(this, offset + width * y);
        }

        /// <summary>
        /// Generare geometrie figura (override din Shape)
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                StreamGeometry geometry = new StreamGeometry();

                using (StreamGeometryContext context = geometry.Open())
                {
                    // desenare forma
                    DrawGeometry(context);

                    // dreptunghi zona (capteaza click-ul mouse-ului pe element)
                    if (fillShape)
                    {
                        DrawRectangle(context, new Point(0, 0), 5 * unit, 2 * unit, false);
                    }
                }

                return geometry;
            }
        }

        /// <summary>
        /// Metoda desenare, specifica fiecarui component
        /// </summary>
        /// <param name="context">Contextul de desenare</param>
        protected abstract void DrawGeometry(StreamGeometryContext context);

        /// <summary>
        /// Deseneaza o linie dreapta de la un punct dat si avand o lungime
        /// </summary>
        /// <param name="context">Contextul de desenare</param>
        /// <param name="start">Punctul de start al liniei</param>
        /// <param name="len">Lungimea liniei</param>
        /// <param name="horizontal">Linie orizontala sau verticala</param>
        protected void DrawStraightLine(StreamGeometryContext context, Point start, double len, bool horizontal = true)
        {
            Point end = horizontal ? new Point(start.X + len, start.Y) : new Point(start.X, start.Y + len);
            context.BeginFigure(start, false, false);
            context.LineTo(end, true, true);
        }

        /// <summary>
        /// Deseneaza un dreptunghi
        /// </summary>
        /// <param name="context">Contextul de desenare</param>
        /// <param name="start">Punctul coltului stanga sus al dreptunghiului</param>
        /// <param name="width">Latimea dreptunghiului</param>
        /// <param name="height">Inaltimea dreptunghiului</param>
        /// <param name="isStroked">Daca are contur</param>
        /// <param name="fill">Daca este umplut (cu transparenta)</param>
        protected void DrawRectangle(StreamGeometryContext context, Point start, double width, double height, bool isStroked, bool fill = true)
        {
            context.BeginFigure(start, fill, true);
            List<Point> pointList = new List<Point>
            {
                new Point(start.X + width, start.Y),
                new Point(start.X + width, start.Y + height),
                new Point(start.X, start.Y + height),
                new Point(start.X, start.Y)
            };
            context.PolyLineTo(pointList, isStroked, false);
        }

        /// <summary>
        /// Eveniment selectare element
        /// </summary>
        /// <param name="e">Eveniment mouse</param>
        override protected void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            //TODO: set selected
            Console.WriteLine("Clicked");
            Stroke = Brushes.Blue;
        }

        public static void SetCanvasSize(Canvas c, int w, int h)
        {
            c.MinWidth = w * width + 2 * unit;
            c.MinHeight = (h + 1) * height + 2 * unit;
        }
    }

    class RezistentaDraw : ComponentDraw
    {
        /// <summary>
        /// Deseneaza o rezistenta
        /// </summary>
        /// <param name="x">Randul din tablou</param>
        /// <param name="y">Coloana din tablou</param>
        public RezistentaDraw(CircuitPro.CircuitModel.Component component, int x = 0, int y = 0) : base(component, x, y) { }

        protected override void DrawGeometry(StreamGeometryContext context)
        {
            DrawStraightLine(context, new Point(0, unit), unit);
            DrawRectangle(context, new Point(unit, unit / 2), 3 * unit, unit, true, false);
            DrawStraightLine(context, new Point(4 * unit, unit), unit);
        }
    }

    class CondensatorDraw : ComponentDraw
    {
        /// <summary>
        /// Deseneaza un condensator
        /// </summary>
        /// <param name="x">Randul din tablou</param>
        /// <param name="y">Coloana din tablou</param>
        public CondensatorDraw(CircuitPro.CircuitModel.Component component, int x = 0, int y = 0) : base(component, x, y) { }

        protected override void DrawGeometry(StreamGeometryContext context)
        {
            DrawStraightLine(context, new Point(0, unit), 2.25 * unit);
            DrawStraightLine(context, new Point(2.25 * unit, unit / 4), 1.5 * unit, false);
            DrawStraightLine(context, new Point(2.75 * unit, unit / 4), 1.5 * unit, false);
            DrawStraightLine(context, new Point(2.75 * unit, unit), 2.25 * unit);
        }
    }

    class BobinaDraw : ComponentDraw
    {
        /// <summary>
        /// Deseneaza o bobina
        /// </summary>
        /// <param name="x">Randul din tablou</param>
        /// <param name="y">Coloana din tablou</param>
        public BobinaDraw(CircuitPro.CircuitModel.Component component, int x = 0, int y = 0) : base(component, x, y) { }

        protected override void DrawGeometry(StreamGeometryContext context)
        {
            // legatura stanga
            DrawStraightLine(context, new Point(0, unit), unit);

            // bucle bobina
            int len = 5;
            for (int i = 0; i < len; i++)
            {
                int delta = i * unit / 2;
                context.ArcTo(new Point(2 * unit + delta - 4, unit), new Size(1, 1.3), 0, false, SweepDirection.Clockwise, true, true);
                if (i != len - 1)
                {
                    context.ArcTo(new Point(1.5 * unit + delta, unit), new Size(1, 3), 0, false, SweepDirection.Clockwise, true, true);
                }
            }

            // conexiune dreapta
            context.LineTo(new Point(5 * unit, unit), true, true);
        }
    }

    class GeneratorDraw : ComponentDraw
    {
        /// <summary>
        /// Deseneaza o bobina
        /// </summary>
        /// <param name="x">Randul din tablou</param>
        /// <param name="y">Coloana din tablou</param>
        public GeneratorDraw(int x = 0, int y = 0) : base(null, x, y) { }

        protected override void DrawGeometry(StreamGeometryContext context)
        {
            // legatura stanga
            DrawStraightLine(context, new Point(0, unit), 1.75 * unit);

            // cerc
            double diameter = 0.75 * unit;
            context.ArcTo(new Point(3.25 * unit, unit), new Size(diameter, diameter), 0, false, SweepDirection.Clockwise, true, true);
            context.ArcTo(new Point(1.75 * unit, unit), new Size(diameter, diameter), 0, true, SweepDirection.Clockwise, true, true);

            // semn alternativ
            context.BeginFigure(new Point(2 * unit, unit), false, false);
            context.BezierTo(new Point(2.5 * unit, 0), new Point(2.5 * unit, 2 * unit), new Point(3 * unit, unit), true, true);

            // conexiune dreapta
            DrawStraightLine(context, new Point(3.25 * unit, unit), 1.75 * unit);
        }
    }

    class FillLine : ComponentDraw
    {
        protected int len = 0;
        public FillLine(int row, int column, int len) : base(null, row, column, false)
        {
            this.len = len;
        }

        protected override void DrawGeometry(StreamGeometryContext context)
        {
            DrawStraightLine(context, new Point(0, unit), len * width);
        }
    }

    class ParalelLine : ComponentDraw
    {
        protected int h = 0;
        protected int w = 0;
        public ParalelLine(int row, int column, int h, int w) : base(null, row, column, false)
        {
            this.h = h;
            this.w = w;
        }

        protected override void DrawGeometry(StreamGeometryContext context)
        {
            DrawStraightLine(context, new Point(0, unit), (h - 1) * height, false);
            DrawStraightLine(context, new Point(w * width, unit), (h - 1) * height, false);
        }
    }
}
