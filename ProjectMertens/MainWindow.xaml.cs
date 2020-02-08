using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace ProjectMertens
{
    public class sleutel
    {
        //deze waarden bevatten de klasse sleutel
        public string badge
        {
            get;
            set;
        }
        public string datum
        {
            get;
            set;
        }

        public sleutel(string _badge, string _datum)
        {
            badge = _badge;
            datum = _datum;
        }
        public override int GetHashCode()
        {
            return Tuple.Create(badge, datum).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is sleutel && this == (sleutel)obj;
        }
        public static bool operator ==(sleutel a, sleutel b)
        {
            return a.badge == b.badge && a.datum == b.datum;
        }
        public static bool operator !=(sleutel a, sleutel b)
        {
            return !(a == b);
        }
    }

    public class waarde
    {
        //klasse waarde bevat volgende waarden
        public string start
        {
            get;
            set;
        }
        public string einde
        {
            get;
            set;
        }

        public waarde(string _start, string _einde)
        {
            start = _start;
            einde = _einde;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV files (*.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Copy path
                string path = dlg.InitialDirectory + dlg.FileName;
                //Get csv to list
                using (var reader = new StreamReader(path))
                {

                    var gegevens = new Dictionary<sleutel,
                        List<waarde>>();
                    var overuren = new Dictionary<sleutel,
                        TimeSpan>();

                    //values[0] //nr's based op de csv zn kolomnrs (badgenr)
                    //values[2] //datum
                    //values[3] //beginuur
                    //values[4] //einduur

                    string headerLine = reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');

                        sleutel sl = new sleutel(values[0], values[2]);
                        List<waarde> temp;
                        waarde w = new waarde(values[3], values[4]);
                        if (gegevens.TryGetValue(sl, out temp))
                        {
                            temp.Add(w);
                        }
                        else
                        {
                            gegevens[sl] = new List<waarde>();
                            gegevens[sl].Add(w);
                        }

                    }

                    foreach (var pair in gegevens)
                    {
                        if (pair.Value.Count == 1)
                        {
                            waarde datum = pair.Value.First();
                            Console.WriteLine(datum.start + " " + datum.einde);
                            overuren[pair.Key] = Overuurberekening(TimeSpan.Parse(datum.start), TimeSpan.Parse(datum.einde), true, DateTime.Parse(pair.Key.datum));
                        }
                        else
                        {
                            foreach (waarde datum in pair.Value) // Loop through List with foreach
                            {
                                overuren[pair.Key] = Overuurberekening(TimeSpan.Parse(datum.start), TimeSpan.Parse(datum.einde), true, DateTime.Parse(pair.Key.datum));
                                overuren[pair.Key] = Overuurberekening(TimeSpan.Parse(datum.start), TimeSpan.Parse(datum.einde), false, DateTime.Parse(pair.Key.datum));
                            }
                        }
                    }
                    Console.WriteLine("output:");
                    //gegevens.Select(i => $"{i.Key.badge + " " + i.Key.datum}: {i.Value.ToString()}").ToList().ForEach(Console.WriteLine);
                    foreach (var pair in gegevens)
                    {
                        Console.WriteLine(pair.Key.badge + " " + pair.Key.datum);
                        foreach (var uren in overuren)
                        {
                            Console.WriteLine(uren.Value);
                        }
                    }
                }
            }
        }
        public static TimeSpan Overuurberekening(TimeSpan begin, TimeSpan einde, bool voormiddag, DateTime datum)
        {
            // Constanten
            TimeSpan BeginUur = new TimeSpan(8, 0, 0);
            TimeSpan StartUurmiddagPauze = new TimeSpan(12, 0, 0);
            TimeSpan EindUurMiddagPauze = new TimeSpan(13, 0, 0);
            TimeSpan EindUur = new TimeSpan(17, 0, 0);
            TimeSpan EindUurVrijdag = new TimeSpan(16, 0, 0);


            // Aantal overUren en aantal Uren te laat gestart 
            TimeSpan overUren = new TimeSpan(0, 0, 0);
            TimeSpan TotaaloverUren = new TimeSpan(0, 0, 0);
            // Vraag eerste uur op 

            if (voormiddag)
            {
                // Als begin uur van werknemer uur grooter dan standaard begin uur is 
                if (begin > BeginUur) //te laat
                {
                    // Beginuur van werknemer - beginuur = aantal Uren te laat begonnen
                    overUren.Subtract(BeginUur - begin);
                    Console.WriteLine(datum.ToString() + "Overuren aftrekken, te laat toegekomen: " + overUren.ToString());
                }

                // Als eind uur van werknemer uur grooter dan standaard eind uur
                if (einde < StartUurmiddagPauze) //te vroeg vertokken
                {
                    // Beginuur van werknemer - beginuur = overuren
                    overUren = overUren.Subtract(EindUur - einde);
                    Console.WriteLine(datum.ToString() + "Overuren aftrekken, te vroeg vertokken: " + overUren.ToString());
                }
            }
            else
            {
                // Als begin uur van werknemer uur grooter dan standaard begin uur is 
                if (begin > EindUurMiddagPauze)
                {
                    // Beginuur van werknemer - beginuur = aantal Uren te laat begonnen
                    overUren.Subtract(begin - BeginUur);
                    Console.WriteLine(datum.ToString() + "Overuren aftrekken, te laat toegekomen: " + overUren.ToString());
                }


                // Als eind uur van werknemer uur grooter dan standaard eind uur
                if (einde < EindUur && IsVrijdag(datum))
                {
                    // Beginuur van werknemer - beginuur = overuren
                    overUren = overUren.Subtract(einde - EindUurVrijdag);
                    Console.WriteLine(datum.ToString() + "Overuren aftrekken, te vroeg vertokken: " + overUren.ToString());
                }
                else
                {
                    if (einde < EindUur && !IsVrijdag(datum))
                    {
                        overUren = overUren.Subtract(einde - EindUur);
                        Console.WriteLine(datum.ToString() + "Overuren aftrekken, te vroeg vertokken: " + overUren.ToString());
                    }
                }
            }
            return overUren;
        }

        public static bool IsVrijdag(DateTime datum)
        {
            if (datum.DayOfWeek == DayOfWeek.Friday)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}