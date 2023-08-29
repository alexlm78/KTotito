using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace KTotito;

public partial class MainWindow : Window {

    private readonly Dictionary<Player, ImageSource> imageSources = new(){
        { Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png"))},
        { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png"))}
    };

    private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animations = new() {
        { Player.X, new ObjectAnimationUsingKeyFrames() },
        { Player.O, new ObjectAnimationUsingKeyFrames() }
    };

    private readonly DoubleAnimation fadeOutAnimation = new DoubleAnimation {
        Duration = TimeSpan.FromSeconds(0.5),
        From = 1,
        To = 0
    };

    private readonly DoubleAnimation fadeInAnimation = new DoubleAnimation {
        Duration = TimeSpan.FromSeconds(0.5),
        From = 0,
        To = 1
    };

    private readonly Image[,] imageControls = new Image[3, 3];
    private readonly GameState gameState = new GameState();

    public MainWindow() {
        InitializeComponent();
        SetupGameGrid();
        SetupAnimations();

        gameState.MoveMade += OnMoveMade;
        gameState.GameFinished += OnGameFinished;
        gameState.GameRestarted += OnGameRestarted;
    }

    private void SetupGameGrid() {
        for (int r=0; r<3; r++) {
            for (int c=0; c<3; c++) {
                Image imageControl = new Image();
                GameGrid.Children.Add(imageControl);
                imageControls[r, c] = imageControl;
            }
        }
    }

    private void SetupAnimations() {
        animations[Player.X].Duration = TimeSpan.FromSeconds(0.25);
        animations[Player.O].Duration = TimeSpan.FromSeconds(0.25);

        for (int i=0; i<16; i++) {
            Uri xUri = new Uri($"pack://application:,,,/Assets/X{i}.png");
            BitmapImage xImg = new BitmapImage(xUri);
            DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
            animations[Player.X].KeyFrames.Add(xKeyFrame);

            Uri oUri = new Uri($"pack://application:,,,/Assets/O{i}.png");
            BitmapImage oImg = new BitmapImage(oUri);
            DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
            animations[Player.O].KeyFrames.Add(oKeyFrame);
        }
    }

    private async Task FadeOut(UIElement element) {
        element.BeginAnimation(OpacityProperty, fadeOutAnimation);
        await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
        element.Visibility = Visibility.Hidden;
    }

    private async Task FadeIn(UIElement element) {
        element.Visibility = Visibility.Visible;
        element.BeginAnimation(OpacityProperty, fadeInAnimation);
        await Task.Delay(fadeInAnimation.Duration.TimeSpan);
    }

    private async Task TransitionToEndScreen(string text, ImageSource winnerImage) {
        await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
        await FadeIn(EndScreen);
    }

    private async Task TransitionToGameScreen() {
        await FadeOut(EndScreen);
        Line.Visibility = Visibility.Hidden;
        await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
    }

    private (Point, Point) FindLinePoints(WinInfo winInfo) {
        double squareSize = GameGrid.Width / 3;
        double margin = squareSize / 2;

        if ( winInfo.Type == WinType.Row) {
            double y = winInfo.Number * squareSize + margin;
            return (new Point(0, y), new Point(GameGrid.Width, y));
        }
        if ( winInfo.Type == WinType.Column) {
            double x = winInfo.Number * squareSize + margin;
            return (new Point(x, 0), new Point(x, GameGrid.Height));
        }
        if( winInfo.Type == WinType.MainDiagonal) {
            return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));
        }

        return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));
    }

    private async Task ShowLine(WinInfo winInfo) {
        (Point start, Point end) = FindLinePoints(winInfo);

        Line.X1 = start.X;
        Line.Y1 = start.Y;

        DoubleAnimation x2Animation = new DoubleAnimation {
            Duration = TimeSpan.FromSeconds(0.25),
            From = start.X,
            To = end.X
        };

        DoubleAnimation y2Animation = new DoubleAnimation {
            Duration = TimeSpan.FromSeconds(0.25),
            From = start.Y,
            To = end.Y
        }; 

        Line.Visibility = Visibility.Visible;
        Line.BeginAnimation(Line.X2Property, x2Animation);
        Line.BeginAnimation(Line.Y2Property, y2Animation);
        await Task.Delay(x2Animation.Duration.TimeSpan);
    }

    private void OnMoveMade(int r, int c) {
        Player player = gameState.GameGrid[r, c];
        imageControls[r, c].BeginAnimation(Image.SourceProperty, animations[player]);
        PlayerImage.Source = imageSources[gameState.CurrentPlayer]; 
    }

    private async void OnGameFinished(GameResult result) {
        await Task.Delay(1000);

        if (result.Winner == Player.None)
            await TransitionToEndScreen("It's a tie!", null);
        else {
            await ShowLine(result.WinInfo); 
            await Task.Delay(1000);
            await TransitionToEndScreen("Winner:", imageSources[result.Winner]);
        }
    }

    private async void OnGameRestarted() {
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++) {
                imageControls[r, c].BeginAnimation(Image.SourceProperty, null);
                imageControls[r, c].Source = null;
            }

        PlayerImage.Source = imageSources[gameState.CurrentPlayer];
        await TransitionToGameScreen();
    }

    private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e) {
        double squeareSize = GameGrid.Width / 3;
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squeareSize);
        int col = (int)(clickPosition.X / squeareSize);

        gameState.MakeMove(row, col);
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        if (gameState.GameOver)
            gameState.Reset();
    }

}
