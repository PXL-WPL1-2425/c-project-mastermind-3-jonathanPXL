using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
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
using System.Windows.Threading;

namespace Mastermind_PE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] generatedCode; // De willekeurig gegenereerde code
        private int attempts = 0;
        private DispatcherTimer timer;
        DateTime clicked;
        TimeSpan elapsedTime;
        string feedback = "";
        string[,] Historiek = new string[10, 5];
        private int score = 100;
        string naam = "";
        int maxAttemps = 0;
        private string[] highscores = new string[15];
        private int highscoreCount = 0;
        private List<string> playerNames = new List<string>();
        private int currentPlayerIndex = 0;


        public MainWindow()
        {
            InitializeComponent();
            GenerateRandomCode();
            OpvullenComboBoxes();
            

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += startcountdown;
        }
        private void startgame(object sender, RoutedEventArgs e)
        {
            bool morePlayers = true;
            while (morePlayers)
            {
                naam = Interaction.InputBox("Wat is jouw naam?", "Naam gebruiker");
                if (!string.IsNullOrWhiteSpace(naam))
                {
                    playerNames.Add(naam); // Voeg de naam toe aan de lijst
                }
                else
                {
                    MessageBox.Show("Naam mag niet leeg zijn. Probeer het opnieuw.", "Foutmelding");
                    continue; // Vraag opnieuw om een naam
                }
                
                var result = MessageBox.Show("Wil je nog een speler toevoegen?","Nog een speler?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    morePlayers = false;
                }
            }
            maxAttemps = int.Parse(Interaction.InputBox("Hoeveel pogingen wil je hebben", "Aantal pogingen"));

            while (maxAttemps < 3 || maxAttemps > 20)
            {
                maxAttemps = int.Parse(Interaction.InputBox("Hoeveel pogingen wil je hebben", "Aantal pogingen"));
            }
        }
        private void startgame2()
        {
            naam = Interaction.InputBox("Wat is jouw naam?", "Naam gebruiker");

            while (string.IsNullOrEmpty(naam))
            {

                naam = Interaction.InputBox("Wat is jouw naam?", "Naam gebruiker");
            }

            maxAttemps = int.Parse(Interaction.InputBox("Hoeveel pogingen wil je hebben", "Aantal pogingen"));

            while (maxAttemps < 3 || maxAttemps > 20)
            {
                maxAttemps = int.Parse(Interaction.InputBox("Hoeveel pogingen wil je hebben", "Aantal pogingen"));
            }
        }
        private void startcountdown(object sender, EventArgs e)
        {
            elapsedTime = DateTime.Now - clicked;
            timerTextBox.Text = $"{elapsedTime.Seconds.ToString()} ";
            if (elapsedTime.Seconds >= 10)
            {
                timer.Stop();

                attempts++;
            }
        }

        private void Afsluiten_Click(object sender, RoutedEventArgs e)
        {

            Close();

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Voorkom dat het spel zomaar wordt gesloten
            var result = MessageBox.Show("Weet je zeker dat je de applicatie wilt sluiten?",
                                          "Bevestig afsluiten",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                // Annuleer het afsluiten
                e.Cancel = true;
            }
        }






        private void GenerateRandomCode()
        {

            Random random = new Random();
            string[] Colors = { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
            generatedCode = Enumerable.Range(0, 4).Select(_ => Colors[random.Next(Colors.Length)]).ToArray();
            this.Title = $"MasterMind ({string.Join(",", generatedCode)}), Poging: ";
        }

        private void OpvullenComboBoxes()
        {
            string[] Colors = { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };

            ComboBox1.ItemsSource = Colors;
            ComboBox2.ItemsSource = Colors;
            ComboBox3.ItemsSource = Colors;
            ComboBox4.ItemsSource = Colors;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Label1.Background = GetBrushFromColorName(ComboBox1.SelectedItem as string);
            Label2.Background = GetBrushFromColorName(ComboBox2.SelectedItem as string);
            Label3.Background = GetBrushFromColorName(ComboBox3.SelectedItem as string);
            Label4.Background = GetBrushFromColorName(ComboBox4.SelectedItem as string);
        }


        private SolidColorBrush GetBrushFromColorName(string colorName)
        {
            switch (colorName)
            {
                case "Rood": return Brushes.Red;
                case "Geel": return Brushes.Yellow;
                case "Oranje": return Brushes.Orange;
                case "Wit": return Brushes.White;
                case "Groen": return Brushes.Green;
                case "Blauw": return Brushes.Blue;
                default: return Brushes.Transparent;
            }
        }

        private void CheckCode_Click(object sender, RoutedEventArgs e)
        {

            attempts++;
            this.Title = $"MasterMind ({string.Join(",", generatedCode)}), Poging: " + attempts;

            timer.Start();
            clicked = DateTime.Now;

            string Kleur1 = ComboBox1.SelectedItem?.ToString();
            string Kleur2 = ComboBox2.SelectedItem?.ToString();
            string Kleur3 = ComboBox3.SelectedItem?.ToString();
            string Kleur4 = ComboBox4.SelectedItem?.ToString();

            // Reset feedback en score reductie
            feedback = "";
            int penalty = 0;

            // Haal de geselecteerde kleuren op
            string[] userCode = {
        ComboBox1.SelectedItem as string,
        ComboBox2.SelectedItem as string,
        ComboBox3.SelectedItem as string,
        ComboBox4.SelectedItem as string
    };

            // Controleer elke invoer en stel de randkleur en score reductie in
            penalty += CheckColorAndCalculatePenalty(Label1, userCode[0], 0);
            penalty += CheckColorAndCalculatePenalty(Label2, userCode[1], 1);
            penalty += CheckColorAndCalculatePenalty(Label3, userCode[2], 2);
            penalty += CheckColorAndCalculatePenalty(Label4, userCode[3], 3);

            // Update score
            score -= penalty;

            // Voeg poging toe aan de historiek
            if (attempts <= maxAttemps)
            {
                Historiek[attempts - 1, 0] = Kleur1;
                Historiek[attempts - 1, 1] = Kleur2;
                Historiek[attempts - 1, 2] = Kleur3;
                Historiek[attempts - 1, 3] = Kleur4;
                Historiek[attempts - 1, 4] = feedback;
            }

            // Update de ListBox
            ListBoxHistoriek.Items.Clear();
            for (int i = 0; i < attempts; i++)
            {
                string feedbackString = $"{Historiek[i, 0]}, {Historiek[i, 1]}, {Historiek[i, 2]}, {Historiek[i, 3]} -> {Historiek[i, 4]}";
                ListBoxHistoriek.Items.Add(feedbackString);
            }

            // Toon de score in een Label
            ScoreLabel.Content = $"Score: {score}";

            // Controleer of het spel moet eindigen
            if (feedback.Contains("J J J J")) // Code gekraakt
            {
                timer.Stop();
                string volgendeSpeler = GetNextPlayerName();
                MessageBox.Show($"Gefeliciteerd {playerNames[currentPlayerIndex]}! Je hebt de code gekraakt!\nNu is {volgendeSpeler} aan de beurt.",
                                $"Spel gewonnen - {playerNames[currentPlayerIndex]}");
                MoveToNextPlayer();
                AddHighscore(naam, attempts, score);
            }
            else if (attempts >= maxAttemps) // Maximaal aantal pogingen bereikt
            {
                timer.Stop();
                string volgendeSpeler = GetNextPlayerName();
                MessageBox.Show($"Helaas {playerNames[currentPlayerIndex]}! Je hebt de code niet gekraakt.\nDe juiste code was: {string.Join(", ", generatedCode)}\nNu is {volgendeSpeler} aan de beurt.",
                                $"Spel verloren - {playerNames[currentPlayerIndex]}");
                MoveToNextPlayer();
                AddHighscore(naam, attempts, score);
            }

        }
        private string GetNextPlayerName()
        {
            int nextPlayerIndex = (currentPlayerIndex + 1) % playerNames.Count;
            return playerNames[nextPlayerIndex];
        }

        private void MoveToNextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % playerNames.Count;
            this.Title = $"MasterMind - {playerNames[currentPlayerIndex]}";
            ResetGame();
        }

        private void AddHighscore(string playerName, int attempts, int score)
        {
            if (highscoreCount < highscores.Length)
            {
                highscores[highscoreCount] = $"{playerName} - {attempts} pogingen - {score}/100";
                highscoreCount++;
            }
            else
            {
                MessageBox.Show("De highscorelijst is vol. Oude scores moeten worden overschreven.", "Highscores vol");
            }
        }

        private void ShowHighscores()
        {
            if (highscoreCount == 0)
            {
                MessageBox.Show("Er zijn nog geen highscores.", "Mastermind highscores");
            }
            else
            {
                string highscoreList = string.Join("\n", highscores.Where(h => !string.IsNullOrEmpty(h)));
                MessageBox.Show(highscoreList, "Mastermind highscores");
            }
        }



        private int CheckColorAndCalculatePenalty(System.Windows.Controls.Label label, string selectedColor, int position)
        {
            if (selectedColor == generatedCode[position])
            {
                label.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                feedback += "J "; // Correct kleur en positie
                label.BorderThickness = new Thickness(3);
                return 0; // 0 strafpunten
            }
            else if (generatedCode.Contains(selectedColor))
            {
                label.BorderBrush = new SolidColorBrush(Colors.Wheat);
                feedback += "FP "; // Correct kleur, verkeerde positie
                label.BorderThickness = new Thickness(3);
                return 1; // 1 strafpunt
            }
            else
            {
                label.BorderBrush = Brushes.Transparent;
                feedback += "F "; // Onjuiste kleur
                label.BorderThickness = new Thickness(0);
                return 2; // 2 strafpunten
            }
        }
        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            ShowHighscores();
        }

        private void ResetGame()
        {
            // Reset score en pogingen
            score = 100;
            attempts = 0;
            feedback = "";



            // Genereer een nieuwe code
            GenerateRandomCode();

            // Wis de historiek
            ListBoxHistoriek.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Historiek[i, j] = null;
                }
            }

            // Reset labels en comboboxes
            Label1.BorderBrush = Brushes.Transparent;
            Label2.BorderBrush = Brushes.Transparent;
            Label3.BorderBrush = Brushes.Transparent;
            Label4.BorderBrush = Brushes.Transparent;

            ComboBox1.SelectedItem = null;
            ComboBox2.SelectedItem = null;
            ComboBox3.SelectedItem = null;
            ComboBox4.SelectedItem = null;

            // Reset score label
            ScoreLabel.Content = "Score: 100";

            // Herstart timer
            timerTextBox.Text = "";
            timer.Stop();
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            startgame2();
        }

        private void pogingen_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Aantal pogingen: {attempts}");
        }


    }
}
