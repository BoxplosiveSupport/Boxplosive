$.fn.initCharts = function () {
  if (typeof (Chart) !== 'undefined') {
    $(this).each(function () {
      var $elem = $(this),
      type = $elem.data('chart');
      if (typeof Chart.types[type] !== 'undefined' && $elem.is(':visible') && !$elem.data('chartinstance')) {
        var $canvas = $elem.find('canvas'),
            $data = $elem.find('[data-data]'),
            $options = $elem.find('[data-options]'),
            $legend = $elem.find('[data-legend]'),
            ctx = $canvas.get(0).getContext("2d");
        chart = new Chart(ctx)[type]($data.length ? JSON.parse($data.html()) : {}, $options.length ? JSON.parse($options.html()) : {});
        if (type == "Bar" && (chart.options.fillColors || chart.options.highlightFills)) {
          var fillColorIndex = 0,
              highlightFillIndex = 0;
          chart.eachBars(function (bar, index) {
            if (chart.options.fillColors.length) {
              bar.fillColor = chart.options.fillColors[fillColorIndex];
              fillColorIndex = fillColorIndex < (chart.options.fillColors.length - 1) ? fillColorIndex + 1 : 0;
            }
            if (chart.options.highlightFills.length) {
              bar.highlightFill = chart.options.highlightFills[highlightFillIndex];
              highlightFillIndex = highlightFillIndex < (chart.options.highlightFills.length - 1) ? highlightFillIndex + 1 : 0;
            }
          });
          chart.update();
        }
        if ($legend.length) {
          $legend.html(chart.generateLegend());
        }
        $elem.data('chartinstance', chart);
      }
    });
  }
};

$(function () {
  // If chartjs is available, set default options and run init on chart dom nodes
  if (typeof (Chart) != 'undefined') {
    Chart.defaults.global.responsive = true;
    Chart.defaults.global.onAnimationComplete = function () {
      if (this.options.stickTooltips) {
        if (this.name == "Bar") {
          this.showTooltip(this.datasets[0].bars, true);
        }
        if (this.name == "Pie") {
          this.showTooltip(this.segments, true);
        }
      }
    }
    // Improve performance for IE8 and other browsers without canvas support. Just reducing the animation steps to 1, to keep the ability to manipulate options after animation end event.
    if (!Modernizr.canvas) {
      Chart.defaults.global.animationSteps = 1;
    }

    setTimeout(function () {
      $('[data-chart]').initCharts();
    }, 100);
  }
});