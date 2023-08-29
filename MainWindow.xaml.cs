using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

    private void TransitionToEndScreen(string text, ImageSource winnerImage) {
        TurnPanel.Visibility = Visibility.Hidden;
        GameCanvas.Visibility = Visibility.Hidden;
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
        EndScreen.Visibility = Visibility.Visible;
    }

    private void TransitionToGameScreen() {
        EndScreen.Visibility = Visibility.Hidden;
        Line.Visibility = Visibility.Hidden;
        TurnPanel.Visibility = Visibility.Visible;
        GameCanvas.Visibility = Visibility.Visible;
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

    private void ShowLine(WinInfo winInfo) {
        (Point start, Point end) = FindLinePoints(winInfo);

        Line.X1 = start.X;
        Line.Y1 = start.Y;

        Line.X2 = end.X;
        Line.Y2 = end.Y;

        Line.Visibility = Visibility.Visible;
    }

    private void OnMoveMade(int r, int c) {
        Player player = gameState.GameGrid[r, c];
        imageControls[r, c].BeginAnimation(Image.SourceProperty, animations[player]);
        PlayerImage.Source = imageSources[gameState.CurrentPlayer]; 
    }

    private async void OnGameFinished(GameResult result) {
        await Task.Delay(1000);

        if (result.Winner == Player.None)
            TransitionToEndScreen("It's a tie!", null);
        else {
            ShowLine(result.WinInfo); 
            await Task.Delay(1000);
            TransitionToEndScreen("Winner:", imageSources[result.Winner]);
        }
    }

    private void OnGameRestarted() {
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++) {
                imageControls[r, c].BeginAnimation(Image.SourceProperty, null);
                imageControls[r, c].Source = null;
            }

        PlayerImage.Source = imageSources[gameState.CurrentPlayer];
        TransitionToGameScreen();
    }

    private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e) {
        double squeareSize = GameGrid.Width / 3;
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squeareSize);
        int col = (int)(clickPosition.X / squeareSize);

        gameState.MakeMove(row, col);
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        gameState.Reset();
    }

}
