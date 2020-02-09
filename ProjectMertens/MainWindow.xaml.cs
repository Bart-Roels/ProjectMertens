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
using CsvHelper;

namespace ProjectMertens
{
    public class sleutel
    {
        //deze waarden bevatten de klasse sleutel
        public string badge { get; set; }
        public string datum { get; set; }
        public string naam { get; set; }


        public sleutel(string _badge, string _datum, string _naam)
        {
            badge = _badge;
            datum = _datum;
            naam = _naam;
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
        public string start { get; set; }
        public string einde { get; set; }

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

                    var gegevens = new Dictionary<sleutel, List<waarde>>();
                    var overuren = new Dictionary<sleutel, TimeSpan>();
                    TimeSpan overurenTotaal = new TimeSpan(0,0,0);

                    //values[0] //nr's based op de csv zn kolomnrs (badgenr)
                    //values[2] //datum
                    //values[3] //beginuur
                    //values[4] //einduur
                    //values[8] //naam

                    string headerLine = reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');

                        sleutel sl = new sleutel(values[0], values[2], values[8]);
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
                            Console.WriteLine("1 Maal gebatcht die dag.");
                            waarde datum = pair.Value.First();
                            Console.WriteLine(datum.start + " " + datum.einde);
                            overuren[pair.Key] = Overuurberekening(TimeSpan.Parse(datum.start), TimeSpan.Parse(datum.einde), true, DateTime.Parse(pair.Key.datum));
                        }
                        else
                        {
                            waarde datum = pair.Value[0];
                            Console.WriteLine("Voormiddag");
                            overuren[pair.Key] = Overuurberekening(TimeSpan.Parse(datum.start), TimeSpan.Parse(datum.einde), true, DateTime.Parse(pair.Key.datum));
                            datum = pair.Value[1];
                            Console.WriteLine("Namiddag");
                            overuren[pair.Key] += Overuurberekening(TimeSpan.Parse(datum.start), TimeSpan.Parse(datum.einde), false, DateTime.Parse(pair.Key.datum));
                            Console.WriteLine("Totaal overuren: " + overuren[pair.Key]);
                        }
                    }
                    Console.WriteLine("output:");
                    //gegevens.Select(i => $"{i.Key.badge + " " + i.Key.datum}: {i.Value.ToString()}").ToList().ForEach(Console.WriteLine);

                    //var csv = new StringBuilder();
                    //var newLine = string.Format("Badgenr;Naam;Datum;Overuren");
                    //csv.AppendLine(newLine);

                    foreach (var pair in gegevens)
                    {
                        Console.WriteLine(pair.Key.badge + " " + pair.Key.datum);       
                        Console.WriteLine(overuren[pair.Key]);
                        overurenTotaal += overuren[pair.Key];
                        lblOutput.Text = ("In de maand " + DateTime.Parse(pair.Key.datum).ToString("MMMM") + " heeft " + pair.Key.naam + " (" + pair.Key.badge + ") " + overurenTotaal + " overuren gemaakt");
                    }
                    
                    ////newLine = string.Format(pair.Key.badge + ";" + pair.Key.naam + ";" + pair.Key.datum + ";" + overuren[pair.Key]);
                    //csv.AppendLine(newLine);

                    ////after your loop
                    //string pad = "D:\\Desktop\\output.csv";
                    //File.WriteAllText(@pad, csv.ToString());
                    //MessageBox.Show("De CSV met output is weggeschreven naar: " + pad);


                }
            }
            }
            public static TimeSpan Overuurberekening(TimeSpan beginUurWerknemer, TimeSpan eindUurWerknemer, bool voormiddag,  DateTime datum)
            {
                // Constanten
                TimeSpan beginUurConstant = new TimeSpan(8, 0, 0);
                TimeSpan StartUurmiddagPauze = new TimeSpan(12, 0, 0);
                TimeSpan EindUurMiddagPauze = new TimeSpan(13, 0, 0);
                TimeSpan EindUur = new TimeSpan(17, 0, 0);
                TimeSpan EindUurVrijdag = new TimeSpan(16, 0, 0);


                // Aantal overUren en aantal Uren te laat gestart 
                TimeSpan overUren = new TimeSpan(0, 0, 0);
                TimeSpan telaat = new TimeSpan(0, 0, 0);
                TimeSpan TotaaloverUren = new TimeSpan(0, 0, 0);
                TimeSpan teVroegVertrokken = new TimeSpan(0, 0, 0);
                TimeSpan kwartier = new TimeSpan(0, 15, 0);



                if (voormiddag)
                {
                    // Als begin uur van werknemer uur grooter dan standaard begin uur is 
                    if (beginUurWerknemer > beginUurConstant) //te laat gestart (smorgens)
                    {
                        // Beginuur van werknemer - beginuur = aantal Uren te laat begonnen
                        overUren = overUren.Subtract(beginUurWerknemer - beginUurConstant);
                        Console.WriteLine(datum.ToString() + " Overuren aftrekken, te laat toegekomen (smorgens): " + overUren);
                    }
                    // Als eind uur van werknemer uur grooter dan standaard eind uur
                    if (eindUurWerknemer < StartUurmiddagPauze) //te vroeg gestopt (smorgens)
                    {
                        // Beginuur van werknemer - beginuur = overuren
                        overUren = overUren.Subtract(StartUurmiddagPauze - eindUurWerknemer);
                        Console.WriteLine(datum.ToString() + " Overuren aftrekken, te vroeg gestopt (smiddag): " + overUren);
                    }
                    if (eindUurWerknemer > StartUurmiddagPauze) // overgewerkt in voormiddag
                    {
                        Console.WriteLine("Overgewerkt");
                        if (eindUurWerknemer - StartUurmiddagPauze >= kwartier)
                        {
                        overUren = overUren.Add(eindUurWerknemer - StartUurmiddagPauze);
                        Console.WriteLine(datum.ToString() + " Overuren optellen, te lang gewerkt (voormiddag) : " + overUren);
                        }
                    }
                } 
                else
                {
                        // Als begin uur van werknemer uur grooter dan standaard begin uur is 
                    if (beginUurWerknemer > EindUurMiddagPauze) // te laat begonnen (smiddags)
                    {
                       // Beginuur van werknemer - beginuur = aantal Uren te laat begonnen
                         overUren = overUren.Subtract(beginUurWerknemer - EindUurMiddagPauze);
                        Console.WriteLine(datum.ToString() + " Overuren aftrekken, te laat gestart (smiddags) : " + overUren);
                    }
                       // Te vroeg gestopt op vrijdag
                    if (eindUurWerknemer < EindUur && IsVrijdag(datum))
                    {
                        // Beginuur van werknemer - beginuur = overuren
                        overUren = overUren.Subtract(EindUurVrijdag - eindUurWerknemer);
                        Console.WriteLine(datum.ToString() + " Overuren aftrekken, te vroeg vertokken op vrijdag: " + overUren);
                    }
                    if (eindUurWerknemer < EindUur && !IsVrijdag(datum))
                    {
                        overUren = overUren.Subtract(EindUur - eindUurWerknemer);
                        Console.WriteLine(datum.ToString() + " Overuren aftrekken, te vroeg vertokken: " + overUren);
                    }
                    if (eindUurWerknemer > EindUur && !IsVrijdag(datum)) // overgewerkt in namiddag
                    {
                        Console.WriteLine("Overgewerkt");
                        if (eindUurWerknemer - EindUur >= kwartier)
                        {
                        overUren = overUren.Add(eindUurWerknemer - EindUur);
                        Console.WriteLine(datum.ToString() + " Overuren optellen, te lang gewerkt (namiddag) : " + overUren);
                        }
                    }
                    if (eindUurWerknemer > EindUur && IsVrijdag(datum)) // overgewerkt in namiddag
                    {
                        Console.WriteLine("Overgewerkt");
                        if (eindUurWerknemer - EindUur <= kwartier)
                        {
                        overUren = overUren.Add(eindUurWerknemer - EindUur);
                        Console.WriteLine(datum.ToString() + " Overuren optellen, te lang gewerkt (vrijdag namiddag) : " + overUren);
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




