// Extends bootstrap tab
(function ($) {
  // set original show function from prototype to call later as super
  var _originalShow = $.fn.tab.Constructor.prototype.show,
      _originalActivate = $.fn.tab.Constructor.prototype.activate;

  $.extend($.fn.tab.Constructor.prototype, {
    show: function () {
      // Set active class to .panel-tabs parent element and set all panels to equal heights.
      this.element.parents('.panel-tabs').addClass('active').find('.panel').equalHeights();
      // call super
      _originalShow.call(this);
    },
    activate: function (element, container, callback) {
      _originalActivate.call(this, element, container, callback);
      element.find('[data-chart]').initCharts();
    }
  });
})(jQuery);

// Force to close all tabs with a click
$(document).on('click', '[data-closealltabs]', function (e) {
  e.preventDefault();
  var tabpanel = $(this).parents('[role=tabpanel]');
  // Remove all active classes to ensure a correct style reset
  tabpanel.find('.panel-tabs, .panel-tabs .active, .tab-pane.active').removeClass('active');
  // Reset aria attributes set by bootstrap scripts
  tabpanel.find('[aria-expanded=true]').attr('[aria-expanded=false]');
});