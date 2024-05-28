// Jquery UI widget extension for autocomplete
$.widget('custom.boxAutocomplete', $.ui.autocomplete, {
  // Extend item element with item data
  _renderItemData: function (ul, item, template) {
    item.element = this._renderItem(ul, item, template);
    return item.element.data('ui-autocomplete-item', item);
  },
  // Use template if defined, else fallback to default HTML
  _renderItem: function (ul, item, template) {
    var element;
    if (template) {
      element = $(Mustache.render(template, item));
    } else {
      var span = $('<span>').append(item.label);
      element = $('<li>').append(span);
    }
    element.appendTo(ul);
    return element;
  },
  _renderMenu: function (ul, items) {
    var self = this,
        results = items,
        inst = $(this.element).boxAutocomplete('instance'),
        isChecklist = (inst.type == 'checklist');
    // If a scrollsteplengt is defined, split result into 'pages' and only render first, apply infinite scroll events
    if (inst.scrollsteplength) {
      $(ul).unbind('scroll');
      var index = 0,
          pages = Math.ceil(items.length / inst.scrollsteplength);
      results = items.slice(0, inst.scrollsteplength);

      if (pages > 1) {
        $(ul).scroll(function () {
          if ($(this).scrollTop() >= this.scrollHeight - $(this).outerHeight()) {
            ++index;
            if (index >= pages) return;

            results = items.slice(index * inst.scrollsteplength, index * inst.scrollsteplength + inst.scrollsteplength);

            //append item to ul{
            $.each(results, function (index, item) {
              if (isChecklist) {
                item.isChecklist = true;
              }
              self._renderItemData(ul, item, inst.itemtemplate);
            });
            //refresh menu
            self.menu.refresh();
            // size and position menu
            ul.show();
            self._resizeMenu();
            ul.position($.extend({
              of: self.element
            }, self.options.position));
            if (self.options.autoFocus) {
              self.menu.next(new $.Event('mouseover'));
            }
          }
        });
      }
    }
    // Add isChecklist attribute to each item for easier check on click and select event
    $.each(results, function (index, item) {
      item.isChecklist = isChecklist && !inst.disableSelect;
      self._renderItemData(ul, item, inst.itemtemplate);
    });
  },
  options: {
    create: function () {
      //Extend instance with settings and references, because the instance is the most solid option of jQuery UI widgets to access during events.
      var $this = $(this),
          sub = $($this.data('sub')) || null,
          inst = $this.boxAutocomplete('instance');
      inst.type = $this.data('boxautocomplete');
      inst.itemtemplate = $($this.data('itemtemplate')).first().html();
      inst.idto = $($this.data('idto'));
      inst.scrollsteplength = parseInt($this.data('scrollsteplength'), 10);
      inst.$sub = $($this.data('sub')) || null;
      inst.$super = $($this.data('super')) || null;
      inst.checkurl = $this.data('checkurl');
      inst.disableSelect = $this.data('disableselect');
          
      // If minlenght equals 0, immediately initiate search
      if ($this.data('minlength') == 0) {
        $this.boxAutocomplete('search');
        // Bind parentselect event to trigger search
        $this.bind('parentselect', function () {
          $this.boxAutocomplete('search');
        });
      }
      // If a sub boxAutocomplete is available, use the small element with tabindex to set parent value of sub
      if (sub.length) {
        inst.menu.element.on('click keydown', 'small[tabindex]', function (e) {
          if (!$this.is(':disabled')) {
            if (!e.keyCode || (e.keyCode && e.keyCode === 13)) {
              e.preventDefault();
              e.stopPropagation();
              inst.options.setSub(sub, $(this).parents('.ui-menu-item').first());
            }
          }
        })
      };
      // Non standard jQuery plugin modification to prevent field from gaining focus after select
      inst._off(inst.menu.element, 'menuselect');
      inst._on(inst.menu.element, {
        menuselect: function (event, ui) {
          var item = ui.item.data('ui-autocomplete-item'),
            previous = this.previous;

          // only trigger when focus was lost (click on menu)
          if (this.element[0] !== this.document[0].activeElement) {
            this.previous = previous;
            // #6109 - IE triggers two focus events and the second
            // is asynchronous, so we need to reset the previous
            // term synchronously and asynchronously :-(
            this._delay(function () {
              this.previous = previous;
              this.selectedItem = item;
            });
          }

          if (false !== this._trigger('select', event, { item: item })) {
            this._value(item.value);
          }
          // reset the term after the select event
          // this allows custom select handling to work properly
          this.term = this._value();

          this.close(event);
          this.selectedItem = item;
        }
      });
      if (Modernizr && Modernizr.touch) {
        // Prevent select issues on touch devices by removing hover events
        inst.menu.element.off('menufocus hover mouseover mouseenter');
      }
      if (inst.type == 'checklist' && inst.checkurl) {
        $(document).on('click', '[data-boxautocompletecheckall=true][data-for="#' + this.id + '"], [data-boxautocompletecheckall=false][data-for="#' + this.id + '"]', function (e) {
          e.preventDefault();
          var $checkbtn = $(this);
          inst.options.startLoading($this, inst);
          // Set timeout to prevent flash of loading styling
          setTimeout(function () {
            $.ajax({
              url: inst.checkurl,
              dataType: 'json',
              data: { 'id': "-1", 'checked':$checkbtn.data('boxautocompletecheckall') },
              success: function (data) {
                // Only true or false are accepted as checked values
                if (data.checked === true || data.checked === false) {
                  $this.boxAutocomplete('search');
                } else if (inst.$sub.length) {
                  var subinst = inst.$sub.boxAutocomplete('instance');
                  if (subinst) {
                    subinst.options.stopLoading(inst.$sub, subinst);
                    inst.$sub.val('').prop('disabled', false);
                  }
                }
              },
              error: function () {
                inst.options.stopLoading($this, inst);
                if (inst.$sub.length) {
                  var subinst = inst.$sub.boxAutocomplete('instance');
                  if (subinst) {
                    subinst.options.stopLoading(inst.$sub, subinst);
                    inst.$sub.val('').prop('disabled', false);
                  }
                }
              }
            });
          }, 500);
        });
      }
    },
    search: function () {
      var $this = $(this),
          inst = $this.boxAutocomplete('instance');
      inst.menu.element.scrollTop(0);
      // If a sub is avaialable, empty result
      if (inst.$sub.length && inst.$sub.boxAutocomplete('instance')) {
        var $sub = inst.$sub,
            subinst = $sub.boxAutocomplete('instance');
        $sub.prop('disabled', true)
        subinst.menu.element.empty();
        subinst.options.stopLoading($sub, subinst);
      }
      inst.options.stopLoading($this, inst);
    },
    // Replace default soure function of autocomplete to be able to add parentid to json requets data
    source: function (request, response) {
      var $this = $(this),
        src = this.element.data('source'),
        data = {
          term: request.term
        }
      if (this.element.data('parent')) {
        data.parentid = this.element.data('parent').data('ui-autocomplete-item').id;
      }
      $.ajax({
        url: src,
        dataType: 'json',
        data: data,
        success: function (data) {
          response(data);
        }
      });
    },
    select: function (e, ui) {
      var $this = $(this),
          inst = $this.boxAutocomplete('instance'),
          subinst = null,
          superinst = null;
      // Prevent select while loading
      if ($this.is(':disabled')) {
        return false;
      }
      // Only if the autocomplete is a checklist, 
      if (inst.type == 'checklist') {
        // A checkurl is required
        if (inst.checkurl) {
          inst.options.startLoading($this, inst);
          // Set timeout to prevent flash of loading styling
          setTimeout(function () {
            $.ajax({
              url: inst.checkurl,
              dataType: 'json',
              data: { 'id': ui.item.id, 'checked': !ui.item.checked },
              success: function (data) {
                // Only true or false are accepted as checked values
                if (data.checked === true || data.checked === false) {
                  // Update item data
                  ui.item.subchecked = false;
                  ui.item.checked = data.checked;
                  // Rerender item HTML with updated data
                  inst.options.rerenderItem(ui.item.element, ui.item, inst.itemtemplate);
                  if (inst.$sub.length) {
                    inst.options.setSub(inst.$sub, ui.item.element);
                  }
                  // If a super is defined, set checked false and subchecked true, to enable different styling
                  if (inst.$super.length && $this.data('parent')) {
                    superinst = inst.$super.boxAutocomplete('instance');
                    if (superinst) {
                      var parentdata = $this.data('parent').data('ui-autocomplete-item');
                      parentdata.checked = false;
                      parentdata.subchecked = true;
                      superinst.options.rerenderItem(parentdata.element, parentdata, inst.itemtemplate);
                    }
                  }
                }
                inst.options.stopLoading($this, inst);
              },
              error: function () {
                inst.options.stopLoading($this, inst);
              }
            });
          }, 500);
        }
      } else if (inst.idto.length) {
        // If idto has been specified, set selected item id and trigger change
        inst.idto.val(ui.item.id).trigger('change');
      } else if (inst.disableSelect && inst.$sub.length) {
        // If the boxAutocomplete had been set to only select to filter a sub and not to check or choose an value
        inst.options.setSub(inst.$sub, ui.item.element);
        return false;
      }
    },
    // Prevent default focus (which sets focussed item value into input)
    focus: function (e, ui) {
      return false;
    },
    // Custom function to rerender a single item after check and uncheck
    rerenderItem: function (element, item, template) {
      var newelement;
      if (template) {
        newelement = $(Mustache.render(template, item));
      } else {
        var span = $('<span>').append(item.label);
        newelement = $('<li>').append(span);
      }
      element.html(newelement.html());
    },
    // Function to set parent id to sub and trigger parent select
    setSub: function (sub, element) {
      // Remove select classes from all items
      element.parents('.ui-menu').find('.ui-state-selected').removeClass('ui-state-selected');
      // Add select class to new parentselect item
      element.addClass('ui-state-selected');
      sub.data('parent', element).prop('disabled', false).trigger('parentselect');
    },
    startLoading: function ($element, inst) {
      $element.prop('disabled', true);
      inst.menu.element.addClass('ui-state-loading');
      $('[data-for="#' + $element.attr('id') + '"]').prop('disabled', true);
      if (inst.$sub.length) {
        subinst = inst.$sub.boxAutocomplete('instance');
        if (subinst) {
          subinst.options.startLoading(inst.$sub, subinst);
        }
      }
    },
    stopLoading: function ($element, inst) {
      $element.prop('disabled', false);
      $('[data-for="#' + $element.attr('id') + '"]').prop('disabled', false);
      inst.menu.element.removeClass('ui-state-loading');
    }
  }
});

// Bind to boxautocomplete elements on focus and parentselect element
$(document).on('focus parentselect', '[data-boxautocomplete]', function () {
  var $this = $(this);
  // Set default options from data attributes (which is not possible within widget extension)
  $this.boxAutocomplete({
    minLength: $this.data('minlength'),
    appendTo: $($this.data('appendto')) || null
  });
});

