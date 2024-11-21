using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace MemoryGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int score = 0;
        private List<string> cardImages;
        private Button firstCard, secondCard;
        private DispatcherTimer gameTimer;
        private DateTime startTime;
        private bool isChecking;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            cardImages = new List<string>
            {
                "🐶", "🐶", "🐱", "🐱", "🐭", "🐭", "🐹", "🐹",
                "🐰", "🐰", "🦊", "🦊", "🐻", "🐻", "🐼", "🐼"
            };
            cardImages = cardImages.OrderBy(x => Guid.NewGuid()).ToList();

            cardGrid.Children.Clear();
            foreach (var image in cardImages)
            {
                Button cardButton = new Button
                {
                    Content = "❓",
                    FontSize = 28
                };
                cardButton.Click += CardButton_Click;
                cardButton.Tag = image;
                cardGrid.Children.Add(cardButton);
            }

            startTime = DateTime.Now;
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            ShowNewGameMessage();
        }
        private void UpdateScore(int points)
        {
            score += points;
            lbScore.Content = score;
        }

        private void ShowNewGameMessage()
        {
            newGamePopup.IsOpen = true;
            Task.Delay(2000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => newGamePopup.IsOpen = false);
            });
        }
        private void ShowNewGamePopup()
        {
            newGamePopup.IsOpen = true;
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
            timer.Tick += (s, args) =>
            {
                newGamePopup.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        private TextBlock FindTextBlock(DependencyObject parent)
        {
            if (parent is TextBlock textBlock && textBlock.Name == "congratulationsTextBlock")
            {
                return textBlock;
            }

            return null;
        }
        private void ShowCongratulations()
        {
            congratulationsPopup.IsOpen = true;
            Storyboard showAnimation = (Storyboard)FindResource("ShowCongratulations");

            var textBlock = FindTextBlock(congratulationsPopup.Child);

            if (textBlock != null)
            {
                showAnimation.Completed += (s, e) => congratulationsPopup.IsOpen = false;
                showAnimation.Begin(textBlock);
            }
        }


        private void GameTimer_Tick(object sender, EventArgs e)
        {
            var elapsedTime = DateTime.Now - startTime;
            timerTextBlock.Text = $"Idő: {elapsedTime:mm\\:ss}";
        }

        private async void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (isChecking) return;

            var clickedButton = sender as Button;
            if (clickedButton.Content.ToString() != "❓") return;

            clickedButton.Content = clickedButton.Tag.ToString();

            if (firstCard == null)
            {
                firstCard = clickedButton;
            }
            else
            {
                secondCard = clickedButton;
                isChecking = true;

                if (firstCard.Tag.ToString() == secondCard.Tag.ToString())
                {
                    firstCard.IsEnabled = false;
                    secondCard.IsEnabled = false;
                    firstCard = null;
                    secondCard = null;
                    isChecking = false;
                    UpdateScore(20);

                    if (cardGrid.Children.OfType<Button>().All(b => !b.IsEnabled))
                    {
                        gameTimer.Stop();
                        ShowCongratulations();
                    }
                }
                else
                {
                    await Task.Delay(1000);
                    firstCard.Content = "❓";
                    secondCard.Content = "❓";
                    firstCard = null;
                    secondCard = null;
                    isChecking = false;
                }
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }
    }
}