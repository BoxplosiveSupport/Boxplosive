module Boxplosive {
  export class PartialStep extends Partial {
    private manager: PartialstepsManager;
    constructor(element: HTMLElement, manager: PartialstepsManager) {
        super(element);
      this.manager = manager;
      this.getURL = this.$element.data('get') || this.manager.getURL;
      this.postURL = this.$element.data('post') || this.manager.postURL;
    }
    init() {
      var self = this;
      this.$element.on('click', '[data-action=saveandeditnext]', function (e) {
        e.preventDefault();
        self.manager.editNext(self);
      });
      super.init();
    }
    edit(submitelem?: HTMLElement) {
      if (this.manager.editAllowed(this)) {
        super.edit(submitelem);
      }
    }
    protected updateView(html) {
      super.updateView(html);
      this.manager.updateStatus();
    }
    protected disable() {
      super.disable();
      $.extend(this.loadingButtons, this.$element.find('[data-action=saveandeditnext]:not(.loading)').addClass('loading'));
    }
  }
}