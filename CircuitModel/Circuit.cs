using CircuitShapes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CircuitPro.CircuitModel
{
    public class Circuit
    {
        public Dictionary<string, Component> componente;
        private Component circ;
        private readonly Generator g;
        private string descriere;

        public string Descriere
        {
            get => descriere;
            set
            {
                descriere = value;
                SetCircuit(value);
            }
        }

        public double Frecventa
        {
            get
            {
                return g.Frecventa;
            }
            set
            {
                g.SetFrecventa(value);
                if (circ != null)
                {
                    circ.SetFrecventa(value);
                }
            }
        }

        public double Tensiune
        {
            get => g.Tensiune;
            set
            {
                g.SetTensiune(value);
                if(circ != null)
                {
                    circ.SetTensiune(value);
                }
            }
        }

        public Circuit(double tensiune = 120, double frecventa = 50, List<Component> comp = null, string desc = "")
        {
            // initializare date
            g = new Generator(frecventa, tensiune);
            
            componente = new Dictionary<string, Component>();
            if(comp != null)
            {
                comp.ForEach(c => AddComponent(c));
            }
            
            Descriere = desc;

/*            componente.Add("R1", new Rezistenta("R1", 20));
            componente.Add("R2", new Rezistenta("R2", 30));
            componente.Add("R3", new Rezistenta("R3", 40));

            componente.Add("C1", new Condensator("C1", 10));
            componente.Add("C2", new Condensator("C2", 20));
            componente.Add("C3", new Condensator("C3", 30));

            componente.Add("B1", new Bobina("B1", 10));
            componente.Add("B2", new Bobina("B2", 20));
            componente.Add("B3", new Bobina("B3", 40));*/

            /*circ = ((componente["R1"] + componente["C2"]) * componente["B2"]) + componente["R1"];*/
            //SetCircuit("R1+((R1+R2)*R3*(C3*(R1+R2)*C2+B1))+C1");
            /*Descriere = "R1+((R1+R2)*R3*(C3*(R1+R2)*C2+B1))+C1";*/
        }

        public Circuit(CircuitSerialize s) : this(s.Tensiune, s.Frecventa, s.Componente, s.Descriere) { }

        public CircuitSerialize Serialize()
        {
            return CircuitSerialize{ Descriere = descriere, Frecventa = g.Frecventa, Tensiune = g.Tensiune, Componente = componente.Keys.ToList()}
        }

        public void AddComponent(Component c)
        {
            if(c.Nume == "")
            {
                throw new Exception("Nume necompletat");
            }

            else if(componente.ContainsKey(c.Nume))
            {
                throw new Exception("Numele a fost deja folosit");
            }

            componente.Add(c.Nume, c);
        }

        public ComponentTreeItem GetComponentTree()
        {
            if (circ == null)
                return null;

            return circ.GetTree();
        }

        /// <summary>
        /// Actualizare structura circuit pe baza ecuatiei acestuia
        /// </summary>
        /// <param name="text">ecuatia circuitului</param>
        public void SetCircuit(string text)
        {
            // stiva componente si operatori
            List<Component> stComponente = new List<Component> { null };
            Stack<char> stOperatori = new Stack<char>();

            // parcurgere text
            int len = text.Length;
            for (int i = 0; i < len; i++)
            {
                // component evaluat
                Component c = null;

                // evaluare caracter
                // - alfanumeric
                if (Char.IsLetterOrDigit(text[i]))
                {
                    string key = new Regex(@"^\w+").Match(text[i..]).Value;
                    i += key.Length - 1;

                    if (!componente.ContainsKey(key))
                    {
                        throw new Exception($"Nume component incorect:'{key}'");
                    }

                    c = componente[key].Clone();
                }

                // - intrare in paranteza
                else if(text[i] == '(')
                {
                    stComponente.Add(null);
                }

                // - iesire din paranteza
                else if (text[i] == ')')
                {
                    // prea multe paranteze de inchidere
                    if(stComponente.Count == 1)
                    {
                        throw new Exception("Ecuație incompletă");
                    }

                    c = stComponente[^1];
                    stComponente.RemoveAt(stComponente.Count - 1);
                }

                // - operator
                else if(text[i] == '+' || text[i] == '*')
                {
                    stOperatori.Push(text[i]);
                }

                // - ignorare spatii
                else if(text[i] == ' ')
                {
                    continue;
                }

                // - caracter ilegal
                else
                {
                    throw new Exception($"Caracter nerecunoscut: '{text[i]}'");
                }

                // nu s-a evaluat niciun component
                if(c == null)
                {
                    continue;
                }

                // adaugare component la rezultat
                if (stComponente[^1] == null)
                {
                    stComponente[^1] = c;
                }
                else
                {
                    switch(stOperatori.Peek())
                    {
                        case '+':
                            stComponente[^1] = stComponente[^1] + c;
                            break;

                        case '*':
                            stComponente[^1] = stComponente[^1] * c;
                            break;
                    }

                    stOperatori.Pop(); 
                }
            }

            // verificare incheiere completa
            if(stComponente.Count != 1 || stOperatori.Count != 0)
            {
                throw new Exception("Ecuație incompletă");
            }

            // setare nou circuit
            circ = stComponente[0];
            if (circ != null)
            {
                circ.SetFrecventa(Frecventa);
                circ.SetTensiune(Tensiune);
            }
        }

        public void Desenare(Canvas canvas)
        {
            if (circ == null)
                return;

            // desenare circuit
            (int w, int h) = circ.Desenare(0, 0, canvas);

            // adaugare generator si legare de circuit
            g.Desenare(h, w / 2, canvas, "-1");
            canvas.Children.Add(new ParalelLine("-2", 0, 0, h + 1, w));
            canvas.Children.Add(new FillLine("-1", h, 0, w / 2));
            canvas.Children.Add(new FillLine("-1", h, w / 2 + 1, w / 2 - 1 + w % 2));

            // setare dimensiune interna
            ComponentDraw.SetCanvasSize(canvas, w, h);
        }
    }

    public class CircuitSerialize
    {
        public List<Component> Componente { get; set; }
        public double Frecventa { get; set; }
        public double Tensiune { get; set; }
        public string Descriere { get; set; }
    }
}
