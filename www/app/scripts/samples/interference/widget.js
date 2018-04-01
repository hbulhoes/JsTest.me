(function() {
    class WidgetElement extends HTMLElement {
        constructor() {
            super();
        }

        connectedCallback() {
            var div = document.createElement('div');
            var btn = document.createElement('button');
            var self = this;

            btn.innerText = this.getAttribute('button-text');
            btn.addEventListener('click', function (e) { showAlert(self, btn, div); });

            div.appendChild(btn);
            this.appendChild(div);
        }
    }

    function showAlert(target, btn, div) {
        var cssUrl = '/styles/samples/interference/widget.css';
        if (document.querySelector('link[href*="' + cssUrl + '"]') == null) {
            var link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = cssUrl;
            document.body.append(link);
        }

        btn.style.display = 'none';
        div.classList.add('default-alert');
        div.innerText = target.getAttribute('alert-text');
    }

    // Define the new element
    customElements.define('my-widget', WidgetElement);
})();
