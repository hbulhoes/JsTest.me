(function() {
        function countLines(text) {
            if (!text) {
                return 0;
            }

            var c = 1;

            for (var i = 0; i !== text.length; i++) {
                if (text.charAt(i) === '\n') {
                    c++;
                }
            }

            return c;
        }

        function stripTheLinesContaining(text, regex) {
            var lines = text.split('\n'),
                i = 0,
                res = '';

            for (; i !== lines.length; i++) {
                if (!regex.test(lines[i])) {
                    res += (res && '\n' || '') + lines[i];
                }
            }

            return res;
        }

        window.performSanityCheck = function() {
            var root = document.getElementById("shadowRoot"),
                realCall = root.attachShadow;

            root.attachShadow = function sniffer() {
                try {
                    realCall.apply(root, arguments);
                } catch (e) {
                    var x = new TypeError();
                    x.message = e.message;
                    x.stack = stripTheLinesContaining(e.stack, /sniffer/);
                    throw x;
                }
            };

            var wasIntercepted = false;

            try {
                var f = {
                    toString: function () {
                        try {
                            throw new Error('Catch this');
                        } catch (e) {
                            console.info(e.stack);
                            wasIntercepted = countLines(e.stack) > 3;
                        }

                        throw 'Not really implemented, sorry';
                    }
                };

                var shadow = root.attachShadow({mode: f});
            } catch (e) {
                if (wasIntercepted) {
                    console.info(e.stack);
                    throw 'Element.attachShadow foi interceptada.';
                } else {
                    console.info('Element.attachShadow não foi interceptada.');
                }
            }

            return true;
        }
    }
)();
