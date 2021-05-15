using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CircuitPro.CircuitModel
{
    public enum TipComponent
    {
        GENERATOR,
        REZISTENTA,
        BOBINA,
        CONDENSATOR,
        SERIE,
        PARALEL
    }

    public class ComponentTreeItem
    {
        public ComponentTreeItem(string title, Component c)
        {
            Title = title;
            Items = new ObservableCollection<ComponentTreeItem>();
            component = c;
        }

        public string Title { get; set; }

        public Component component { get; protected set; }

        public ObservableCollection<ComponentTreeItem> Items { get; set; }
    }

    public abstract class Component
    {
        public double Reactanta { get; protected set; }

        public double Defazaj { get; protected set; }

        public double Intensitate { get; protected set; }

        public double Tensiune { get; protected set; }

        public string Nume { get; protected set; }

        public TipComponent Tip { get; protected set; }

        public int Height { get; protected set; }
        public int Width { get; protected set; }
        public int Row { get; protected set; }
        public int Column { get; protected set; }

        /// <summary>
        /// Component circuit
        /// </summary>
        /// <param name="tip">tipul componentului</param>
        /// <param name="defazaj">defazajul dintre tensiune si intensitate</param>
        /// <param name="reactanta">reactanta circuitului</param>
        protected Component(TipComponent tip, string nume = "", double defazaj = 0, double reactanta = 0)
        {
            Tip = tip;
            Nume = nume;
            Reactanta = reactanta;
            Defazaj = defazaj;
        }

        public abstract (int w, int h) Desenare(int row, int column, Canvas canvas);

        public virtual ComponentTreeItem GetTree()
        {
            return new ComponentTreeItem(Nume, this);
        }

        public virtual Component LegareSerie(Component b)
        {
            return new CircuitSerie(new List<Component> { this, b });
        }

        public virtual Component LegareParalel(Component b)
        {
            return new CircuitParalel(new List<Component> { this, b });
        }

        public static Component operator +(Component a, Component b)
            => a.LegareSerie(b);

        public static Component operator *(Component a, Component b)
            => a.LegareParalel(b);
    }

    class Rezistenta : Component
    {
        public Rezistenta(string nume, double reactanta) : base(TipComponent.REZISTENTA, nume, 0, reactanta) { }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas)
        {
            canvas.Children.Add(new CircuitShapes.RezistentaDraw(this, row, column));
            return (1, 1);
        }
    }

    class Bobina : Component
    {
        public Bobina(string nume, double inductanta, double frecventa) : base(TipComponent.BOBINA, nume, 90)
        {
            Reactanta = 2 * Math.PI * frecventa * inductanta;
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas)
        {
            canvas.Children.Add(new CircuitShapes.BobinaDraw(this, row, column));
            return (1, 1);
        }
    }

    class Condensator : Component
    {
        public Condensator(string nume, double capacitate, double frecventa) : base(TipComponent.CONDENSATOR, nume, -90)
        {
            Reactanta = 1 / (2 * Math.PI * frecventa * capacitate);
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas)
        {
            canvas.Children.Add(new CircuitShapes.CondensatorDraw(this, row, column));
            return (1, 1);
        }
    }

    class Generator : Component
    {
        public double Frecventa { get; set; }

        public Generator(double frecventa, double tensiune) : base(TipComponent.GENERATOR, "Generator")
        {
            Frecventa = frecventa;
            Tensiune = tensiune;
        }

        public void SetTensiune(double t)
        {
            Tensiune = t;
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas)
        {
            canvas.Children.Add(new CircuitShapes.GeneratorDraw(row, column));
            return (1, 1);
        }
    }
}
