// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows;
using OxyPlot;
using OxyPlot.Wpf;
namespace SimpleDemo
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow" /> class.
    /// </summary>
    public MainWindow()
    {
      this.InitializeComponent();
    }


    public void LoadedHandler(object sender, RoutedEventArgs e)
    {
      var Plot = sender as PlotView;

      var pc = Plot.ActualController;
      pc.UnbindMouseDown(OxyMouseButton.Left);
      pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control);
      pc.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift);

      pc.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                   (view, controller, args) =>
                      controller.AddMouseManipulator(view, new WpbTrackerManipulator(view), args)));

    }

  }
}