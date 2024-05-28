module Boxplosive {
  export class Repeatable {
    protected element: HTMLElement;
    protected $element: JQuery;
    protected $repeatablelist: JQuery;
    protected template: string;
    protected max: number;
    protected maxMessage: string = 'You reached a miximum of {0} items';
    protected theregex: RegExp = /\$?\{(\d+)\}/g;
    protected orderUp: boolean = false;
    constructor(element: HTMLElement, option?: string, args?: Object) {
      this.element = element;
      this.$element = $(element);
      var data = this.$element.data('box.repeatable');
      if (data instanceof Repeatable) {
        if (typeof option == 'string') data[option](args);
        return data;
      }
      this.$repeatablelist = this.$element.find('[data-repeatablelist]').first(),
      this.template = this.$element.find('[data-template]').first().html(),
      this.orderUp = this.$element.data('orderup') || this.orderUp;
      this.max = this.$element.data('max') && parseInt(this.$element.data('max'), 10) ? parseInt(this.$element.data('max'), 10) : null;
      this.maxMessage = this.$element.data('msg-max') ? this.$element.data('msg-max') : this.maxMessage;
      if (this.max) {
        this.maxMessage = $.validator.format(this.maxMessage, this.max.toString());
      }
      this.$element.data('box.repeatable', this);
      this.init(option, args);
    }
    init(option?: string, args?: Object) {
      if (typeof option == 'string') this[option](args);
    }
    add(args) {
      var length = this.$repeatablelist.children('[data-repeatableitem]:visible').length,
        data = {
          index: this.$repeatablelist.children('[data-repeatableitem]').length
        }
      if (this.max && length >= this.max) {
        alert(this.maxMessage);
      } else {
          $.extend(data, args);
        var html = $(Mustache.render(this.template, data));
        html.find('[data-boxspinner] input').boxSpinner();
        html.find('[data-upload]').each(function () {
          new Boxplosive.Upload(this).init();
        });
        if (this.orderUp) {
          this.$repeatablelist.prepend(html);
        } else {
          this.$repeatablelist.append(html);
        }
      }
    }
    remove($element) {
      if ($element.length) {
        $element.hide().find('[data-removetag]').val('true');
      }
    }
  }
}

$(document).on('click', '[data-action=addrepeatable]', function (e) {
  e.preventDefault();
  var $this = $(this),
    $parent = $this.closest('[data-repeatable]');
  new Boxplosive.Repeatable($parent.get(0), 'add');
});

$(document).on('click', '[data-repeatableitem] [data-action=remove]', function (e) {
  e.preventDefault();
  e.stopPropagation();
  var $this = $(this),
    $item = $this.closest('[data-repeatableitem]'),
    $parent = $this.closest('[data-repeatable]');
  new Boxplosive.Repeatable($parent.get(0), 'remove', $item);
});

$(document).on('boxautocompleteselect', '[data-repeatable] [data-boxautocomplete=newrepeatable]', function (e, ui) {
  var $this = $(this),
    $parent = $this.closest('[data-repeatable]');
  new Boxplosive.Repeatable($parent.get(0), 'add', ui.item);
  $this.val('');
  return false;
});

$(document).on('click', '#multiitembtnok', function (e) {
    var $parent = $(this).closest('[data-repeatable]');
    var $repeatablelist = $parent.find('[data-repeatablelist]');

    // Clear all previous selected products
    $repeatablelist.empty();

    var html = '';
    var $firstMultiItemSelectsLi = $('#multiitemselects li').first();
    var $multiItemCountElement = $firstMultiItemSelectsLi.children('.multiitemcount');
    if ($multiItemCountElement.length) {
        var multiItemCountText = $multiItemCountElement.text() + ' Products uploaded';
        var data = { element: $firstMultiItemSelectsLi, count: multiItemCountText };

        var template = $parent.find('[data-template-multiitemscount]').first().html();
        html += Mustache.render(template, data);
    }
    else {
        var template = $parent.find('[data-template]').first().html();
        var index = 0;
        $('#multiitemselects li').each(function () {
            var id = $(this).children('.id').text();
            var label = $(this).children('.label').text() + ' (' + id + ')';
            var data = { element: $(this), id: id, index: index, isChecklist: false, label: label, value: label };

            html += Mustache.render(template, data);

            index += 1;
        });
    }

    $repeatablelist.html(html);
});
