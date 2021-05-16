using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CircuitPro.CircuitModel
{
    /// <summary>
    /// Element arbore componente circuit
    /// </summary>
    public class ComponentTreeItem : TreeViewItem 
    {
        public string Id { get; protected set; }

        public Component component { get; protected set; }


        /// <summary>
        /// Element arbore componente circuit
        /// </summary>
        /// <param name="nume">Numele afisat</param>
        /// <param name="c">Referinta catre component</param>
        /// <param name="id">Id-ul componentului</param>
        public ComponentTreeItem(string nume, Component c, string id)
        {
            Header = nume;
            Id = id;
            component = c;
        }

        /// <summary>
        /// Actualizare selectie arbore elemente
        /// </summary>
        /// <param name="id">Id-ul elementului selectat</param>
        /// <returns>true daca unul din descendentii elementului este selectat, false altfel</returns>
        public bool SetSelected(string id)
        {
            // selectare daca element selectat
            IsSelected = id == Id;

            // evaluare descendenti
            bool sel = false;
            foreach(ComponentTreeItem item in Items)
            {
                bool res = item.SetSelected(id);
                sel = sel || res;
            }

            // expandare daca unul din descendenti este selectat
            if (sel || IsSelected)
            {
                IsExpanded = true;
            }
            else
            {
                IsExpanded = false;
            }

            return sel || IsSelected;
        }
    }


    /// <summary>
    /// Tipuri component
    /// </summary>
    public enum TipComponent
    {
        GENERATOR,
        REZISTENTA,
        BOBINA,
        CONDENSATOR,
        SERIE,
        PARALEL
    }


    /// <summary>
    /// Reprezentarea unui component al circuitului
    /// </summary>
    public abstract class Component
    {
        /// <summary>
        /// Date component
        /// </summary>
        public string Nume { get; protected set; }

        public TipComponent Tip { get; protected set; }

        public double Reactanta { get; protected set; }

        public double Defazaj { get; protected set; }

        public double Tensiune { get; protected set; }

        public double Intensitate { get; protected set; }


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

        /// <summary>
        /// Creaza o clona a componentului
        /// </summary>
        /// <returns>Noua instanta a clasei cu aceleasi date</returns>
        public virtual Component Clone()
        {
            return null;
        }
        /// <summary>
        /// Desenare component
        /// </summary>
        /// <param name="row">Pozitie rand</param>
        /// <param name="column">Pozitie coloana</param>
        /// <param name="canvas">Canvas-ul pe care este desenat</param>
        /// <param name="id">Id-ul elementului desenat</param>
        /// <returns>Dimensiunile (cate coloane si cate randuri) ocupa elementul desenat</returns>
        public abstract (int w, int h) Desenare(int row, int column, Canvas canvas, string id = "");

        /// <summary>
        /// Construire arbore elemente pentru tree view
        /// </summary>
        /// <param name="id">Id-ul elementului curent</param>
        /// <returns></returns>
        public virtual ComponentTreeItem GetTree(string id = "")
        {
            return new ComponentTreeItem(Nume, this, id);
        }

        // operatii
        /// <summary>
        /// Actualizeaza frecventa elementului (si defazajele exprimate in functie de aceasta)
        /// </summary>
        /// <param name="frecventa">noua frecventa a circuitului</param>
        public virtual void SetFrecventa(double frecventa) { }

        /// <summary>
        /// Actualizeaza tensiunea si intensitatea componentului
        /// </summary>
        /// <param name="tensiune">Tensiunea la bornele componentului</param>
        public virtual void SetTensiune(double tensiune)
        {
            Tensiune = tensiune;
            Intensitate = tensiune / Reactanta;
        }

        /// <summary>
        /// Leaga in serie componentul de un alt component
        /// </summary>
        /// <param name="b">Alt component</param>
        /// <returns>Gruparea in serie a celor doua componente</returns>
        public virtual Component LegareSerie(Component b)
        {
            return new CircuitSerie(new List<Component> { this, b });
        }

        /// <summary>
        /// Leaga in paralel componentul de un alt component
        /// </summary>
        /// <param name="b">Alt component</param>
        /// <returns>Gruparea in paralel a celor doua componente</returns>
        public virtual Component LegareParalel(Component b)
        {
            return new CircuitParalel(new List<Component> { this, b });
        }

        public static Component operator +(Component a, Component b)
            => a.LegareSerie(b);

        public static Component operator *(Component a, Component b)
            => a.LegareParalel(b);
    }


    /// <summary>
    /// Reprezentarea unei rezistente
    /// </summary>
    class Rezistenta : Component
    {
        public Rezistenta(string nume, double reactanta) : base(TipComponent.REZISTENTA, nume, 0, reactanta) { }

        public override Component Clone()
        {
            return new Rezistenta(Nume, Reactanta);
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas, string id)
        {
            canvas.Children.Add(new CircuitShapes.RezistentaDraw(this, id, row, column));
            return (1, 1);
        }
    }


    /// <summary>
    /// Reprezentarea unei bobine
    /// </summary>
    class Bobina : Component
    {
        public double Inductanta { get; protected set; }

        public Bobina(string nume, double inductanta) : base(TipComponent.BOBINA, nume, Math.PI / 2)
        {
            Inductanta = inductanta;
        }

        public override Component Clone()
        {
            return new Bobina(Nume, Inductanta);
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas, string id)
        {
            canvas.Children.Add(new CircuitShapes.BobinaDraw(this, id, row, column));
            return (1, 1);
        }

        public override void SetFrecventa(double frecventa)
        {
            Reactanta = 2 * frecventa * Inductanta;
        }
    }


    /// <summary>
    /// Reprezentarea unui condensator
    /// </summary>
    class Condensator : Component
    {
        public double Capacitate { get; protected set; }

        public Condensator(string nume, double capacitate) : base(TipComponent.CONDENSATOR, nume, -Math.PI / 2)
        {
            Capacitate = capacitate;
        }

        public override Component Clone()
        {
            return new Condensator(Nume, Capacitate);
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas, string id)
        {
            canvas.Children.Add(new CircuitShapes.CondensatorDraw(this, id, row, column));
            return (1, 1);
        }

        public override void SetFrecventa(double frecventa)
        {
            Reactanta = 1 / (2 * frecventa * Capacitate);
        }
    }


    /// <summary>
    /// Reprezentarea generatorului
    /// </summary>
    class Generator : Component
    {
        public double Frecventa { get; set; }

        public Generator(double frecventa, double tensiune) : base(TipComponent.GENERATOR, "Generator")
        {
            Frecventa = frecventa;
            Tensiune = tensiune;
        }

        public override Component Clone()
        {
            return new Generator(Frecventa, Tensiune);
        }

        public override (int w, int h) Desenare(int row, int column, Canvas canvas, string id)
        {
            canvas.Children.Add(new CircuitShapes.GeneratorDraw(this, id, row, column));
            return (1, 1);
        }

        public override void SetFrecventa(double frecventa)
        {
            Frecventa = frecventa;
        }
    }
}
