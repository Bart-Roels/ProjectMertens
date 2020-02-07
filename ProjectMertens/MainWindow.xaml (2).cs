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

namespace ProjectMertens
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Standaard begin uuren
            TimeSpan BeginUur = new TimeSpan(8, 0, 0);
            TimeSpan StartUurmiddagPauze = new TimeSpan(12, 0, 0);
            TimeSpan BeginUurMiddagPauze = new TimeSpan(1, 0, 0);
            TimeSpan EindUur = new TimeSpan(5, 0, 0);



            // Uuren gewerkt door werknemer --> input van jens
            TimeSpan beginUurInput = new TimeSpan(8, 40, 0);
            TimeSpan StartUurmiddagPauzeInput = new TimeSpan(12, 15, 0);
            TimeSpan BeginUurMiddagPauzeInput = new TimeSpan(1, 0, 0);
            TimeSpan indUurInput = new TimeSpan(5, 15, 0);


            // Aantal overuuren en aantal uuren te laat gestart 
            TimeSpan telaat = new TimeSpan(0, 0, 0);
            TimeSpan overuuren = new TimeSpan(0, 0, 0);
            TimeSpan Totaaloveruuren = new TimeSpan(0, 0, 0);
            TimeSpan test = new TimeSpan(0, 15, 0);
            // Vraag eerste uur op 

            // Als begin uur van werknemer uur grooter dan standaard begin uur is 
            if (beginUurInput > BeginUur)
            {
                // Beginuur van werknemer - beginuur = aantal uuren te laat begonnen
                telaat = telaat.Subtract(BeginUur - beginUurInput);
            }

            // Als begin uur van werknemer uur grooter dan standaard begin uur is (middag)
            if (StartUurmiddagPauzeInput > StartUurmiddagPauze)
            {
                // Beginuur van werknemer - beginuur = overuuren
                overuuren = overuuren.Subtract(StartUurmiddagPauze - StartUurmiddagPauzeInput);
            }

            // Als eind uur van werknemer uur grooter dan standaard eind uur
            if (indUurInput > EindUur)
            {
                // Beginuur van werknemer - beginuur = overuren
                overuuren = overuuren.Subtract(EindUur - indUurInput);
            }

            // aantal overuren - aantal uren te laat
            Totaaloveruuren = Totaaloveruuren.Subtract(telaat - overuuren);

            Console.Write(overuuren);
            Console.WriteLine();
            Console.WriteLine(telaat);

            // ALS overuuren
            if (Totaaloveruuren < test)
            {
                // Kwou gwn iets testen en wa spelen me c#
                Console.ForegroundColor = ConsoleColor.Red;
                // Kwou gwn iets testen en wa spelen me c#
                Console.WriteLine($"Totaal van {Console.ForegroundColor}{Totaaloveruuren} niet goe ge werkt he maneke!");
            }
            else
            {
                Console.WriteLine($"Totaal van {Totaaloveruuren} overuuren ");

            }

            Console.ReadKey();







        }
    }
}
