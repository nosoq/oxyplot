// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents the view-model for the main window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SimpleDemo
{

  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using OxyPlot;
  using OxyPlot.Axes;
  using OxyPlot.Series;

  public class WpbTrackerItem
  {
    public string Title { get; set; }
    public OxyColor Color { get; set; }
    public System.Windows.Media.SolidColorBrush WpfColor
    {
      get
      {
        return new System.Windows.Media.SolidColorBrush(
          System.Windows.Media.Color.FromArgb(
          Color.A,
          Color.R,
          Color.G,
          Color.B
         ));
      }
    }
    public System.Windows.Media.Color FillColor
    {
      get
      {
        return
              System.Windows.Media.Color.FromArgb(
              Color.A,
              Color.R,
              Color.G,
              Color.B
            );
      }
    }

    public double Value { get; set; }
    public double CircleX { get; set; }
    public double CircleY { get; set; }
  }

  public class WpbTrackerManipulator : MouseManipulator
  {
    /// <summary>
    /// The current series.
    /// </summary>
    private DataPointSeries currentSeries;

    public WpbTrackerManipulator(IPlotView plotView)
      : base(plotView)
    {
    }

    /// <summary>
    /// Occurs when a manipulation is complete.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Completed(OxyMouseEventArgs e)
    {
      base.Completed(e);
      e.Handled = true;

      currentSeries = null;
      PlotView.HideTracker();
    }

    /// <summary>
    /// Occurs when the input device changes position during a manipulation.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Delta(OxyMouseEventArgs e)
    {
      base.Delta(e);
      e.Handled = true;

      if (currentSeries == null)
      {
        PlotView.HideTracker();
        return;
      }

      var actualModel = PlotView.ActualModel;
      if (actualModel == null)
      {
        return;
      }

      if (!actualModel.PlotArea.Contains(e.Position.X, e.Position.Y))
      {
        return;
      }

      var time = currentSeries.InverseTransform(e.Position).X;
      var points = currentSeries.Points;
      DataPoint dp = points.FirstOrDefault(d => d.X >= time);

      // Exclude default DataPoint.
      // It has insignificant downside and is more performant than using First above
      // and handling exceptions.
      if (dp.X != 0 || dp.Y != 0)
      {
        int index = points.IndexOf(dp);
        var items = PlotView.ActualModel.Series.Cast<DataPointSeries>()
          .Select(s => CreateItem(s, e.Position))
          .OrderByDescending(item => item.Value);

        var result = new WpbTrackerHitResult(items)
        {
          Series = currentSeries,
          DataPoint = dp,
          Index = index,
          Item = dp,
          Position = e.Position,
          PlotModel = PlotView.ActualModel
        };
        PlotView.ShowTracker(result);
      }
    }

    private WpbTrackerItem CreateItem(DataPointSeries series, ScreenPoint position)
    {
      var x = currentSeries.InverseTransform(position).X;
      var points = series.Points;
      DataPoint dp = points.FirstOrDefault(d => d.X >= x);

      

      return new WpbTrackerItem()
      {
        Color = ((LineSeries)series).Color,
        Title = series.Title,
        Value = dp.Y,
        CircleX = series.XAxis.Transform(dp.X),
        CircleY = series.YAxis.Transform(dp.Y)

      };
    }

    /// <summary>
    /// Occurs when an input device begins a manipulation on the plot.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Started(OxyMouseEventArgs e)
    {
      base.Started(e);
      currentSeries = PlotView.ActualModel.Series
                       .FirstOrDefault(s => s.IsVisible) as DataPointSeries;
      Delta(e);
    }
  }

  public class WpbTrackerHitResult : TrackerHitResult
  {
    public string FormatedXValue
    {
      get
      {
        return string.Format("{0} : {1}", XAxis.Title, XAxis.FormatValue(this.DataPoint.X));
      }
    }

    public WpbTrackerItem[] Items { get; private set; }

    public WpbTrackerHitResult(IEnumerable<WpbTrackerItem> items)
    {
      this.Items = items.ToArray();
    }
  }

  /// <summary>
  /// Represents the view-model for the main window.
  /// </summary>
  public class MainViewModel
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel" /> class.
    /// </summary>
    public MainViewModel()
    {
    }


    #region Константы

    /// <summary>
    /// Значение отступа от оси по умолчанию.
    /// </summary>
    private const double DefaultPaddingValue = 0.05;

    /// <summary>
    /// Форматная строка для отображения заголовка серии графика.
    /// </summary>
    private const string SeriesTitleFormat = "{0}";

    /// <summary>
    /// Форматная строка для отображения значения по оси X.
    /// </summary>
    /// <remarks>Результирующая строка будет "Заголовок оси X: значение по оси X".</remarks>
    private const string XDateAxisTitleFormat = "{1}: {2:dd.MM.yyyy}";

    /// <summary>
    /// Форматная строка для отображения значения по оси X.
    /// </summary>
    /// <remarks>Результирующая строка будет "Заголовок оси X: значение по оси X".</remarks>
    private const string XDefaultAxisTitleFormat = "{1}: {2}";

    /// <summary>
    /// Форматная строка для отображения значения по оси Y.
    /// </summary>
    /// <remarks>Результирующая строка будет "Заголовок оси Y: значение по оси Y".</remarks>
    private const string YDefaultAxisTitleFormat = "{3}: {4}";

    /// <summary>
    /// Базовый цвет.
    /// </summary>
    private static readonly OxyColor BaseColor = OxyColor.Parse("#ff666666");

    /// <summary>
    /// Дополнительный цвет.
    /// </summary>
    private static readonly OxyColor MinorColor = OxyColor.Parse("#ffDBDBDB");// OxyColor.Parse("#ffe9e9e9");

    private static readonly OxyColor AxisColor = OxyColor.Parse("#ffDBDBDB");

    #endregion

    private PlotModel plotModel;

    /// <summary>
    /// Модель oxyPlot для отрисовки графика.
    /// </summary>
    public PlotModel Model
    {
      get
      {
        this.plotModel = CreatePlotModel();
        this.plotModel.Axes.Add(CreateXAxis());
        this.plotModel.Axes.Add(CreateYAxis());

        var seriaColor = OxyColor.Parse("#6EA5D4");
        var line = new LineSeries()
        {
          Title = "Серия",
          MarkerFill = seriaColor,
          MarkerType = OxyPlot.MarkerType.None,
          StrokeThickness = 2,
          MarkerSize = 3,
          CanTrackerInterpolatePoints = false,
          Smooth = false,
          Color = seriaColor,
          TrackerFormatString = GetTrackerFormatString("DateTime")
        };

        var startDate = new DateTime(2015, 1, 1);
        var rnd = new Random();
        for (int i = 0; i < 152; i++)
          line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(startDate.AddDays(i)), 210.0 / (i + 1)));

        this.plotModel.Series.Add(line);

        var seriaColor2 = OxyColor.Parse("#ff0000");

        var line2 = new LineSeries()
        {
          Title = "Серия2",
          MarkerFill = seriaColor2,
          MarkerType = OxyPlot.MarkerType.None,
          StrokeThickness = 2,
          MarkerSize = 3,
          CanTrackerInterpolatePoints = false,
          Smooth = false,
          Color = seriaColor2,
          TrackerFormatString = GetTrackerFormatString("DateTime")
        };

        for (int i = 5; i < 150; i++)
          line2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(startDate.AddDays(i)), 220.0 / (152 - i + 1)));

        this.plotModel.Series.Add(line2);

        return this.plotModel;
      }
    }

    /// <summary>
    /// Создать модель данных графика.
    /// </summary>
    /// <returns>Модель данных.</returns>
    private PlotModel CreatePlotModel()
    {
      var model = new PlotModel()
      {
        IsLegendVisible = false,
        LegendMargin = 0,
        LegendPadding = 0,
        LegendOrientation = LegendOrientation.Horizontal,
        LegendPlacement = LegendPlacement.Outside,
        LegendPosition = LegendPosition.BottomCenter,
        LegendTextColor = BaseColor,
        LegendTitleColor = BaseColor,
        TextColor = BaseColor,
        Padding = new OxyThickness(0),
        DefaultFontSize = 10,
        LegendFontSize = 11,
        PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
        PlotAreaBorderColor = AxisColor,
      };

      return model;
    }

    /// <summary>
    /// Создать ось.
    /// </summary>
    /// <param name="type">Тип данных оси.</param>
    /// <param name="title">Подпись оси.</param>
    /// <returns>Ось.</returns>
    private static Axis CreateBaseAxis(string type, string title)
    {
      var axis = type == "DateTime" ? new DateTimeAxis { StringFormat = "dd.MM.yyyy" } : new LinearAxis();
      axis.Title = title;
      axis.IsZoomEnabled = false;
      axis.IsPanEnabled = false;

      axis.MajorGridlineStyle = LineStyle.Dash;

      axis.TextColor = BaseColor;
      axis.TicklineColor = AxisColor;
      axis.AxislineColor = AxisColor;
      axis.ExtraGridlineColor = MinorColor;
      axis.MajorGridlineColor = MinorColor;
      axis.MinorGridlineColor = MinorColor;
      axis.TitleColor = BaseColor;

      return axis;
    }

    /// <summary>
    /// Создать ось X.
    /// </summary>
    /// <returns>Ось.</returns>
    private Axis CreateXAxis()
    {
      var xAxis = CreateBaseAxis("DateTime", "Дата");
      xAxis.Position = AxisPosition.Bottom;

      xAxis.MinimumPadding = 0;
      xAxis.MaximumPadding = 0;

      return xAxis;
    }

    /// <summary>
    /// Создать ось Y.
    /// </summary>
    /// <returns>Ось.</returns>
    private Axis CreateYAxis()
    {
      var yAxis = CreateBaseAxis("Double", "Заданий");
      yAxis.Position = AxisPosition.Left;

      yAxis.MinimumPadding = 0;
      yAxis.MaximumPadding = DefaultPaddingValue;

      return yAxis;
    }

    /// <summary>
    /// Получить форматную строку для отображения значения на графике.
    /// </summary>
    /// <param name="axisType">Тип оси X.</param>
    /// <returns>Форматная строка для отображения значения на графике.</returns>
    private string GetTrackerFormatString(string axisType)
    {
      // http://docs.oxyplot.org/en/latest/models/series/LineSeries.html?highlight=trackerformatstring
      return string.Join(Environment.NewLine,
        SeriesTitleFormat,
        axisType == "DateTime" ? XDateAxisTitleFormat : XDefaultAxisTitleFormat,
        YDefaultAxisTitleFormat);
    }


  }
}