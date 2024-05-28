 module Boxplosive {
     export class PublishModule {
         protected element: HTMLElement;
         protected $element: JQuery;
         protected $publishModuleContent: JQuery;
         constructor(element: HTMLElement) {
             this.element = element;
             this.$element = $(element);
             this.$publishModuleContent = this.$element.find('#PublishModuleContent');
         }

         init() {
             var self = this;

             this.$element.on('change', '#Publish_CampaignType', function (e) {
                 e.preventDefault();
                 var data = { campaignType:  $(this).val() };
                 var getURL = $(this).data('get');
                 $.ajax({
                     url: getURL,
                     type: 'GET',
                     data: data,
                     success: function(data) {
                         self.$publishModuleContent.html(data);
                     },
                     error: function() {
                         //self.$publishModuleContent.html('Meh!');
                         //self.enable();
                     }
                 });
             });
         }
     }
}

$(() => {
    $('#PublishModule').each(function () {
        new Boxplosive.PublishModule(this).init();
    });
});