using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CircuitPro.CircuitModel
{
    abstract class PortiuneCircuit : Component
    {
        protected List<Component> components;

        public PortiuneCircuit(TipComponent tip, List<Component> c) : base(tip)
        {
            components = new List<Component>();
            c.ForEach(component => components.Add(component));
            CalculareDefazaj();
        }

        protected abstract void CalculareDefazaj();

        /// <summary>
        /// Calcularea proiectiilor componentelor pe axe
        /// </summary>
        /// <returns>Pereche de numere reprezentand proiectiile pe axele Ox si Oy</returns>
        protected Tuple<double, double> ProiectiiAxe()
        {
            double x = 0;
            double y = 0;
            components.ForEach(component =>
            {
                x += component.Reactanta * Math.Cos(component.Defazaj);
                y += component.Reactanta * Math.Sin(component.Defazaj);
            });
            return new Tuple<double, double>(x, y);
        }
    }

    class CircuitSerie : PortiuneCircuit
    {
        public CircuitSerie(List<Component> c) : base(TipComponent.SERIE, c)
        {

        }

        protected override void CalculareDefazaj()
        {

        }

        public override Component LegareSerie(Component b)
        {
            components.Add(b);
            return this;
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas)
        {
            // dimensiuni grupare
            int width = 0;
            int height = 0;
            List<int> heights = new List<int>();

            // desenare componente
            components.ForEach(component =>
            {
                (int w, int h) = component.Desenare(row, column + width, canvas);
                width += w;
                height = Math.Max(height, h);

                heights.Add(h);
            });           

            return (width, height);
        }

        public override ComponentTreeItem GetTree()
        {
            ComponentTreeItem trItem = new ComponentTreeItem("Circuit serie", this);
            components.ForEach(component => trItem.Items.Add(component.GetTree()));
            return trItem;
        }
    }

    class CircuitParalel : PortiuneCircuit
    {
        public CircuitParalel(List<Component> c) : base(TipComponent.PARALEL, c)
        {

        }

        protected override void CalculareDefazaj()
        {

        }

        public override Component LegareParalel(Component b)
        {
            components.Add(b);
            return this;
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas)
        {
            // dimensiuni grupare
            int width = 0;
            int height = 0;
            List<(int w, int h)> widths = new List<(int w, int h)>();

            // desenare componente
            components.ForEach(component =>
            {
                (int w, int h) = component.Desenare(row + height, column, canvas);
                widths.Add((w, height));
                width = Math.Max(width, w);
                height += h;
            });

            // desenare linii conectare
            for(int i = 0; i < widths.Count; i++)
            {
                canvas.Children.Add(new CircuitShapes.FillLine(row + widths[i].h, column + widths[i].w, width - widths[i].w));
            }
            canvas.Children.Add(new CircuitShapes.ParalelLine(row, column, widths[^1].h + 1, width));

            return (width, height);
        }

        public override ComponentTreeItem GetTree()
        {
            ComponentTreeItem trItem = new ComponentTreeItem("Circuit paralel", this);
            components.ForEach(component => trItem.Items.Add(component.GetTree()));
            return trItem;
        }
    }
}
