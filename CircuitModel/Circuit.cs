using CircuitShapes;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

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
                GetApp().SetModificat();
            }
        }

        public double Frecventa
        {
            get => g.Frecventa;
            set
            {
                g.SetFrecventa(value);
                if (circ != null)
                {
                    circ.SetFrecventa(value);
                }
                GetApp().SetModificat();
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
                GetApp().SetModificat();
            }
        }

        public Circuit(double tensiune = 120, double frecventa = 50, List<Component> comp = null, string desc = "")
        {
            // initializare generator
            g = new Generator(frecventa, tensiune);
            
            // initializare componente
            componente = new Dictionary<string, Component>();
            if(comp != null)
            {
                comp.ForEach(c => componente.Add(c.Nume, c));
            }
            
            // initializare descriere
            Descriere = desc;
        }

        public string Serialize()
        {
            // initializare obiect
            using MemoryStream stream = new MemoryStream();
            using Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();

            // scriere informatii circuit
            writer.WriteNumber("Frecventa", Frecventa);
            writer.WriteNumber("Tensiune", Tensiune);
            writer.WriteString("Descriere", Descriere);

            // adaugare componente
            writer.WriteStartArray("Componente");
            foreach (string key in componente.Keys)
            {
                // preluare date component
                Component elem = componente[key];
                double valoare = elem.Tip switch
                {
                    TipComponent.BOBINA => ((Bobina)elem).Inductanta,
                    TipComponent.CONDENSATOR => ((Condensator)elem).Capacitate,
                    _ => elem.Reactanta
                };

                // scriere obiect component
                writer.WriteStartObject();
                writer.WriteNumber("Tip", (int)elem.Tip);
                writer.WriteString("Nume", elem.Nume);
                writer.WriteNumber("Valoare", valoare);
                writer.WriteEndObject();
            }

            // inchieiere obiect
            writer.WriteEndArray();
            writer.WriteEndObject();

            // transformare in string
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static Circuit Deserialize(string json)
        {
            // pregatire document
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            JsonElement componentsElement = root.GetProperty("Componente");
            int count = componentsElement.GetArrayLength();

            // citire informatii circuit
            double frecventa = root.GetProperty("Frecventa").GetDouble();
            double tensiune = root.GetProperty("Tensiune").GetDouble();
            string descriere = root.GetProperty("Descriere").GetString();

            // adaugare componente
            List<Component> c = new List<Component>();
            foreach (JsonElement comp in componentsElement.EnumerateArray())
            {
                int tip = comp.GetProperty("Tip").GetInt32();
                string nume = comp.GetProperty("Nume").GetString();
                double valoare = comp.GetProperty("Valoare").GetDouble();
                c.Add((TipComponent)tip switch
                {
                    TipComponent.REZISTENTA => new Rezistenta(nume, valoare),
                    TipComponent.BOBINA => new Bobina(nume, valoare),
                    TipComponent.CONDENSATOR => new Condensator(nume, valoare),
                    _ => null
                });
            }

            // creare circuit nou
            return new Circuit(tensiune, frecventa, c, descriere);
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
            GetApp().SetModificat();
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

        private App GetApp()
        {
            return (App)Application.Current;
        }
    }
}
