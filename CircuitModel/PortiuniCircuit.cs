using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CircuitPro.CircuitModel
{
    abstract class PortiuneCircuit : Component
    {
        protected List<Component> components;

        public PortiuneCircuit(TipComponent tip, List<Component> c, string nume) : base(tip, nume)
        {
            components = new List<Component>();
            c.ForEach(component => components.Add(component));
        }

        /// <summary>
        /// Calcularea proiectiilor componentelor pe axe
        /// </summary>
        /// <returns>Pereche de numere reprezentand proiectiile pe axele Ox si Oy</returns>
        protected (double x, double y) ProiectiiAxe()
        {
            double x = 0;
            double y = 0;
            components.ForEach(component =>
            {
                // unghi in radiani
                double angle = component.Defazaj;

                // calcul element proiectat pe axe
                x += GetChildReactanta(component) * Math.Cos(angle);
                y += GetChildReactanta(component) * Math.Sin(angle);
            });
            return (x, y);
        }

        /// <summary>
        /// Returneaza reactanta elementului pentru a fi adunata la axa
        /// </summary>
        /// <param name="component">Elementul adunat</param>
        /// <returns>Reactanta elementului pentru a fi adunata la axa</returns>
        protected abstract double GetChildReactanta(Component component);

        /// <summary>
        /// Calculeaza reactanta gruparii
        /// </summary>
        /// <param name="x">Suma proiectiilor pe axa Ox</param>
        /// <param name="y">Suma proiectiilor pe axa Oy</param>
        /// <returns>Reactanta</returns>
        protected abstract double ComputeReactanta(double x, double y);

        public override ComponentTreeItem GetTree(string id)
        {
            ComponentTreeItem trItem = new ComponentTreeItem(Nume, this, id);
            for (int i = 0; i < components.Count; i++)
            {
                trItem.Items.Add(components[i].GetTree(id + i));
            }
            return trItem;
        }

        public override void SetFrecventa(double frecventa)
        {
            // setare frecventa descendenti
            components.ForEach(component => component.SetFrecventa(frecventa));

            // calculare reactanta si defazaj (in radiani)
            (double x, double y) = ProiectiiAxe();
            Reactanta = ComputeReactanta(x, y);
            Defazaj = Math.Atan2(y, x);
        }
    }

    class CircuitSerie : PortiuneCircuit
    {
        public CircuitSerie(List<Component> c) : base(TipComponent.SERIE, c, "Grupare Serie") {}

        public override void SetTensiune(double tensiune)
        {
            base.SetTensiune(tensiune);
            components.ForEach(component => component.SetTensiune(component.Reactanta * Intensitate));
        }

        public override Component LegareSerie(Component b)
        {
            components.Add(b);
            return this;
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas, string id)
        {
            // dimensiuni grupare
            int width = 0;
            int height = 0;
            List<int> heights = new List<int>();

            // desenare componente
            for (int i = 0; i < components.Count; i++)
            {
                Component component = components[i];

                (int w, int h) = component.Desenare(row, column + width, canvas, id + i);
                width += w;
                height = Math.Max(height, h);

                heights.Add(h);
            }        

            return (width, height);
        }

        protected override double GetChildReactanta(Component component)
        {
            return component.Reactanta;
        }

        protected override double ComputeReactanta(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }
    }

    class CircuitParalel : PortiuneCircuit
    {
        public CircuitParalel(List<Component> c) : base(TipComponent.PARALEL, c, "Grupare paralel") {}

        public override void SetTensiune(double tensiune)
        {
            base.SetTensiune(tensiune);
            components.ForEach(component => component.SetTensiune(Tensiune));
        }

        public override Component LegareParalel(Component b)
        {
            components.Add(b);
            return this;
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas, string id)
        {
            // dimensiuni grupare
            int width = 0;
            int height = 0;
            List<(int w, int h)> widths = new List<(int w, int h)>();

            // desenare componente
            for (int i = 0; i < components.Count; i++)
            {
                Component component = components[i];

                (int w, int h) = component.Desenare(row + height, column, canvas, id + i);
                widths.Add((w, height));
                width = Math.Max(width, w);
                height += h;
            };

            // desenare linii conectare
            for(int i = 0; i < widths.Count; i++)
            {
                canvas.Children.Add(new CircuitShapes.FillLine(id, row + widths[i].h, column + widths[i].w, width - widths[i].w));
            }
            canvas.Children.Add(new CircuitShapes.ParalelLine(id, row, column, widths[^1].h + 1, width));

            return (width, height);
        }

        protected override double GetChildReactanta(Component component)
        {
            return 1 / component.Reactanta;
        }

        protected override double ComputeReactanta(double x, double y)
        {
            return 1 / Math.Sqrt(x * x + y * y);
        }
    }
}
