using CircuitShapes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CircuitPro.CircuitModel
{
    public class Circuit
    {
        public double Tensiune { get; set; }
        public double Frecventa { get; set; }

        public Dictionary<string, Component> componente;
        private Component circ;
        private Generator g;

        public Circuit(double tensiune = 0, double frecventa = 0)
        {
            Tensiune = tensiune;
            Frecventa = frecventa;
            componente = new Dictionary<string, Component>();

            componente.Add("R1", new Rezistenta("R1", 20));
            componente.Add("R2", new Rezistenta("R2", 30));
            componente.Add("R3", new Rezistenta("R3", 40));

            componente.Add("C1", new Condensator("C1", 10, 50));
            componente.Add("C2", new Condensator("C2", 20, 50));
            componente.Add("C3", new Condensator("C3", 30, 50));

            componente.Add("B1", new Bobina("B1", 10, 50));
            componente.Add("B2", new Bobina("B2", 20, 50));
            componente.Add("B3", new Bobina("B3", 40, 50));

            g = new Generator(Frecventa, Tensiune);

            /*circ = ((componente["R1"] + componente["C2"]) * componente["B2"]) + componente["R1"];*/
            SetCircuit("R1+((R1+R2)*R3*(C3*(R1+R2)*C2+B1))+C1");
        }

        public void AddComponent(Component c)
        {
            componente.Add(c.Nume, c);
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
                        // throw nume incorect
                        System.Console.WriteLine("nume incorect");
                    }

                    c = componente[key];
                }

                // - intrare in paranteza
                else if(text[i] == '(')
                {
                    stComponente.Add(null);
                }

                // - iesire din paranteza
                else if (text[i] == ')')
                {
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
                    // throw caracter incorect
                    System.Console.WriteLine("caracter incorect");
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
                // throw malformed expression
                System.Console.WriteLine("malformed expression");
            }

            // setare nou circuit
            circ = stComponente[0];
        }

        public void Desenare(Canvas canvas)
        {
            if (circ == null)
                return;

            // desenare circuit
            (int w, int h) = circ.Desenare(0, 0, canvas);

            // adaugare generator
            canvas.Children.Add(new ParalelLine(0, 0, h + 1, w));
            canvas.Children.Add(new GeneratorDraw(h, w / 2));
            canvas.Children.Add(new FillLine(h, 0, w / 2));
            canvas.Children.Add(new FillLine(h, w / 2 + 1, w / 2 - 1 + w % 2));

            // setare dimensiune interna
            ComponentDraw.SetCanvasSize(canvas, w, h);
        }

        public ComponentTreeItem GetComponentTree()
        {
            if (circ == null)
                return null;

            return circ.GetTree();
        }
    }
}
