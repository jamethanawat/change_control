$(document).ready(function() {

    $('.inputAnimate').each(function() {

        var self = $(this);
        var input = self.children('input');
        var span = $('<span />').prependTo(self);
        var div = $('<div />').appendTo(self);
        var em = $('<em />').appendTo(self);

        input.keypress(function (e) {
            if(e.which && e.charCode) {
                resizeForText(self, span, $(this).val() + String.fromCharCode(e.keyCode | e.charCode));
            }
        });

        input.keyup(function(e) {
            if(e.keyCode === 8 || e.keyCode === 46) {
                resizeForText(self, span, $(this).val());
            }
        });

        resizeForText(self, span, input.val());

    });

});

function resizeForText(self, span, text) {
    text = (!text) ? ' ' : text;
    span.text(text);
    self.css('--offsetLeft', span.width() + 3);
    self.css('--offsetLeftScale', span.width() + 19);
}