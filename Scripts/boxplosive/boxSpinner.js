// Jquery UI widget extension for spinner
$.widget('custom.boxSpinner', $.ui.spinner, {
    options: {
        start: function (e, ui) {
            // Read data attributes and set valid values as option
            var $this = $(this),
              min = $this.attr('min'),
              max = $this.attr('max'),
              step = $this.attr('step');
            if (typeof min != 'undefined') {
                $this.boxSpinner('option', { 'min': min });
            }
            if (typeof max != 'undefined') {
                $this.boxSpinner('option', { 'max': max });
            }
            if (typeof step != 'undefined') {
                $this.boxSpinner('option', { 'step': step });
            }
        },
        stop: function (e, ui) {
            // Enabled or disable spinner buttons to show use if min or max has been reached.
            var $this = $(this),
              $parent = $this.parent(),
              $down = $parent.find('.ui-spinner-down'),
              $up = $parent.find('.ui-spinner-up'),
              min = $this.boxSpinner('option', 'min'),
              max = $this.boxSpinner('option', 'max');
            if (min != null && min == $this.boxSpinner('value')) {
                $down.prop('disabled', true);
            } else {
                $down.prop('disabled', false);
            }
            if (max != null && max == $this.boxSpinner('value')) {
                $up.prop('disabled', true);
            } else {
                $up.prop('disabled', false);
            }
        }
    },
    _uiSpinnerHtml: function () {
        // Prevent default spinner HTML
        return '';
    },
    _buttonHtml: function () {
        // Return bootstrap standard HTML for buttons
        return '<span class="input-group-btn">' +
          '<button class="ui-spinner-button ui-spinner-down btn btn-default icon icon-min" type="button" data-stepdown ></button>' +
          '<button class="ui-spinner-button ui-spinner-up btn btn-default icon icon-plus" type="button" data-stepup ></button>' +
          '</span>'
    }
});

// Bind boxSpinner to all initially loaded boxspinner elements
$('[data-boxspinner] input, input[data-boxspinner]').boxSpinner();

// Bind boxSpinner to all dinamically loaded boxspinner elements on focus. This is a fallback, binding should take place immediately after updating the DOM
$(document).on('focus', '[data-boxspinner] input, input[data-boxspinner]', function () {
  $(this).boxSpinner();
});

