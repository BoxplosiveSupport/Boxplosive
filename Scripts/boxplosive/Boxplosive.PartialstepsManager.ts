module Boxplosive {
  export class PartialstepsManager {
    public getURL: string;
    public postURL: string;
    protected element: HTMLElement;
    protected $element: JQuery;
    protected partialSteps: PartialStep[] = [];
    protected partialStatus: Object[] = [];
    private _exitmessage: string;
    constructor(element: HTMLElement) {
      this.element = element;
      this.$element = $(element);
      this.getURL = this.$element.data('get');
      this.postURL = this.$element.data('post');
      this._exitmessage = this.$element.data('exitmessage');
    }
    init() {
      var self = this;
      this.$element.find('[data-partial=step]').each(function () {
        var partialStep = new PartialStep(this, self);
        self.partialSteps.push(partialStep);
        partialStep.init();
      });
      this.$element.find('[data-partial=status]').each(function () {
        var partialStatus = new Partial(this);
        self.partialStatus.push(partialStatus);
        partialStatus.init();
      });
      if (this._exitmessage) {
        $(window).on('beforeunload', function () {
          var changed = false;
          $.each(self.partialSteps, function () {
            changed = this.changed() || changed;
          });
          if (changed) {
            return self._exitmessage;
          }
        });
      }
    }
    editAllowed(partialStep: PartialStep): boolean {
      var valid = true;
      $.each(this.partialSteps, function () {
        if (this == partialStep) {
          return;
        } else if (this.valid()) {
          if (this.mode() == 'edit') {
            this.save();
          }
        } else {
          valid = false;
        }
      });
      return valid;
    }
    updateStatus() {
      $.each(this.partialStatus, function () {
        this.update();
      });
    }
    editNext(partialStep: PartialStep) {
      var next = this.partialSteps[$.inArray(partialStep, this.partialSteps) + 1];
      if (next) {
        next.edit();
      }
    }
  }
}

$(() => {
  $('[data-partialsteps]').each(function () {
    new Boxplosive.PartialstepsManager(this).init();
  })
});