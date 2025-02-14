using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace laba1
{
    public partial class MainWindow : Window
    {
        private readonly CheckerGame game;
        private Checker? selectedChecker;

        public MainWindow()
        {
            InitializeComponent();
            game = new CheckerGame();
            game.TurnChanged += OnTurnChanged;
            DrawBoard();
            DrawCheckers();
            BackgroundMusic.Play();
        }

        private void OnTurnChanged(CheckerColor currentTurn)
        {
            TurnIndicator.Text = currentTurn == CheckerColor.White ? "Ход Белых" : "Ход Чёрных";
        }


        private void DrawBoard()
        {
            BoardGrid.Children.Clear();
            BoardGrid.RowDefinitions.Clear();
            BoardGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < 8; i++)
            {
                RowDefinition rowDef = new() { Height = new GridLength(1, GridUnitType.Star) };
                ColumnDefinition colDef = new() { Width = new GridLength(1, GridUnitType.Star) };
                BoardGrid.RowDefinitions.Add(rowDef);
                BoardGrid.ColumnDefinitions.Add(colDef);
            }


            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Border cell = new()
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.Bisque : Brushes.SaddleBrown,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1)
                    };
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    Panel.SetZIndex(cell, -1); BoardGrid.Children.Add(cell);


                    Style buttonStyle = new(typeof(Button));

                    ControlTemplate template = new(typeof(Button));

                    FrameworkElementFactory borderFactory = new(typeof(Border))
                    {
                        Name = "border"
                    };
                    borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
                    borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
                    borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

                    FrameworkElementFactory contentPresenterFactory = new(typeof(ContentPresenter));
                    contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
                    borderFactory.AppendChild(contentPresenterFactory);

                    template.VisualTree = borderFactory;

                    buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, template));

                    buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Transparent));
                    buttonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
                    buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
                    buttonStyle.Setters.Add(new Setter(Button.FocusableProperty, false));

                    Button button = new()
                    {
                        Style = buttonStyle,
                        Tag = (row, col),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    Panel.SetZIndex(button, 1);
                    button.Click += Cell_Click;

                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);
                    BoardGrid.Children.Add(button);

                }
            }
        }

        private void DrawCheckers()
        {
            List<Ellipse> existingCheckers = BoardGrid.Children.OfType<Ellipse>().ToList();
            foreach (Ellipse piece in existingCheckers)
            {
                BoardGrid.Children.Remove(piece);
            }

            foreach (Checker checker in game.Board)
            {
                if (checker != null)
                {
                    Ellipse piece = new()
                    {
                        Width = 70,
                        Height = 70,
                        Fill = checker.Color == CheckerColor.White ? Brushes.White : Brushes.Black,
                        Stroke = Brushes.Red,
                        StrokeThickness = checker.Type == CheckerType.King ? 10 : 0,
                        Tag = checker
                    };

                    piece.MouseDown += Checker_Click;
                    piece.MouseLeftButtonDown += Checker_Click; piece.IsHitTestVisible = false;


                    Panel.SetZIndex(piece, 2);

                    Grid.SetRow(piece, checker.Row);
                    Grid.SetColumn(piece, checker.Col);
                    BoardGrid.Children.Add(piece);

                }
            }
        }


        private void ClearCellSelections()
        {
            foreach (Button btn in BoardGrid.Children.OfType<Button>())
            {
                btn.Background = Brushes.Transparent;
            }
        }



        private void HighlightCell(int row, int col)
        {
            Button? cellButton = BoardGrid.Children.OfType<Button>()
                .FirstOrDefault(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == col);
            if (cellButton != null)
            {
                cellButton.Background = Brushes.Green;
            }
        }


        private void Checker_Click(object sender, MouseButtonEventArgs e)
        {
            ClearCellSelections();

            if (sender is Ellipse clickedEllipse)
            {
                Checker? checker = clickedEllipse.Tag as Checker;
                selectedChecker = checker;
                HighlightCell(checker.Row, checker.Col);
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedCell)
            {
                (int row, int col) = ((int, int))clickedCell.Tag;

                if (selectedChecker == null)
                {
                    selectedChecker = game.Board[row, col];
                    if (selectedChecker != null)
                    {
                        ClearCellSelections();
                        HighlightCell(row, col);
                    }
                }
                else
                {
                    if (game.MoveChecker(selectedChecker.Row, selectedChecker.Col, row, col))
                    {
                        DrawBoard();
                        DrawCheckers();
                    }
                    selectedChecker = null;
                }
            }
        }
        private void ClearPieces()
        {
            var piecesToRemove = BoardGrid.Children.OfType<Ellipse>().ToList();
            foreach (var piece in piecesToRemove)
            {
                BoardGrid.Children.Remove(piece);
            }
        }

        private void RedrawBoard()
        {
            ClearPieces();

            foreach (Checker checker in game.Board)
            {
                if (checker != null)
                {
                    Ellipse piece = new()
                    {
                        Width = 70,
                        Height = 70,
                        Fill = checker.Color == CheckerColor.White ? Brushes.White : Brushes.Black,
                        Stroke = Brushes.Red,
                        StrokeThickness = checker.Type == CheckerType.King ? 10 : 0,
                        Tag = checker
                    };

                    piece.MouseDown += Checker_Click;
                    piece.IsHitTestVisible = false;  

                    Panel.SetZIndex(piece, 2);

                    Grid.SetRow(piece, checker.Row);
                    Grid.SetColumn(piece, checker.Col);
                    BoardGrid.Children.Add(piece);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение", MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }

        }


        private void ResetGameButton_Click(object sender, RoutedEventArgs e)
        {
            game.Restart();  
            RedrawBoard();   
            OnTurnChanged(game.CurrentTurn);
        }

        private void BackgroundMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            BackgroundMusic.Position = TimeSpan.Zero;
            BackgroundMusic.Play();
        }



    }
}
