using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KTotito;

public partial class MainWindow : Window {

    private readonly Dictionary<Player, ImageSource> imageSources = new(){
        { Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png"))},
        { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png"))}
    };

    private readonly Image[,] imageControls = new Image[3, 3];
    private readonly GameState gameState = new GameState();

    public MainWindow() {
        InitializeComponent();
        SetupGameGrid();

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

    private void TransitionToEndScreen(string text, ImageSource winnerImage) {
        TurnPanel.Visibility = Visibility.Hidden;
        GameCanvas.Visibility = Visibility.Hidden;
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
        EndScreen.Visibility = Visibility.Visible;
    }

    private void OnMoveMade(int r, int c) {
        Player player = gameState.GameGrid[r, c];
        imageControls[r, c].Source = imageSources[player];
        PlayerImage.Source = imageSources[gameState.CurrentPlayer]; 
    }

    private void OnGameFinished(GameResult result) {
        if(result.Winner == Player.None) 
            TransitionToEndScreen("It's a tie!", null);
        else
            TransitionToEndScreen("Winner:", imageSources[result.Winner]);
    }

    private void OnGameRestarted() {
        
    }

    private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e) {
        double squeareSize = GameGrid.Width / 3;
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squeareSize);
        int col = (int)(clickPosition.X / squeareSize);

        gameState.MakeMove(row, col);
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        
    }

}
