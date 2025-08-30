using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Discord
{
    /// <summary>
    /// Interaction logic for CanvasView.xaml
    /// </summary>
    public partial class CanvasView : UserControl
    {
        public event Action<string> OnDraw;

        private Point _lastMousePosition;
        private bool _isDrawing = false;
        private Brush _currentColor = Brushes.Black;
        private double _strokeThickness = 2;
        public CanvasView()
        {
            InitializeComponent();
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                _isDrawing = true;
                _lastMousePosition = e.GetPosition(DrawingCanvas);
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the color from the button that was clicked
            Button clickedButton = (Button)sender;
            _currentColor = clickedButton.Background;
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                Point currentMousePosition = e.GetPosition(DrawingCanvas);
                _strokeThickness = SizeSlider.Value;

                // Create a new Line shape
                Line line = new Line
                {
                    X1 = _lastMousePosition.X,
                    Y1 = _lastMousePosition.Y,
                    X2 = currentMousePosition.X,
                    Y2 = currentMousePosition.Y,
                    Stroke = _currentColor,         // USE THE VARIABLE
                    StrokeThickness = _strokeThickness     // And this too
                };

                // Add the line to the canvas
                DrawingCanvas.Children.Add(line);

                string colorHex = new BrushConverter().ConvertToString(_currentColor);
                // Use InvariantCulture to ensure '.' is used as the decimal separator
                string paintData = string.Format(CultureInfo.InvariantCulture, "§PAINT§{0},{1},{2},{3},{4},{5}",
                    _lastMousePosition.X, _lastMousePosition.Y,
                    currentMousePosition.X, currentMousePosition.Y,
                    colorHex, _strokeThickness);

                // 3. Fire the event to send the data to MainWindow
                OnDraw?.Invoke(paintData);

                // Update the last position for the next segment
                _lastMousePosition = currentMousePosition;
            }
        }

        public void DrawLineFromServer(string paintData)
        {
            // Example data: "§PAINT§10.5,20.2,30,40,#FFFF0000,5"
            // Remove the prefix
            string data = paintData.Substring("§PAINT§".Length);
            string[] parts = data.Split(',');

            try
            {
                // Parse the data
                double x1 = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double y1 = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double x2 = double.Parse(parts[2], CultureInfo.InvariantCulture);
                double y2 = double.Parse(parts[3], CultureInfo.InvariantCulture);
                Brush color = (Brush)new BrushConverter().ConvertFromString(parts[4]);
                double thickness = double.Parse(parts[5], CultureInfo.InvariantCulture);

                // Create and draw the line
                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = color,
                    StrokeThickness = thickness
                };
                DrawingCanvas.Children.Add(line);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse paint data: {ex.Message}");
            }
        }
    }
}
