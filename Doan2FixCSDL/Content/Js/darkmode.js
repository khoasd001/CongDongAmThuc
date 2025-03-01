document.addEventListener('DOMContentLoaded', function () {
    var darkModeToggle = document.getElementById('toggle1');
    var overlay = document.createElement('div');
    overlay.id = 'dark-mode-overlay';
    overlay.className = 'dark-mode-overlay';
    document.body.appendChild(overlay);

    // Check localStorage for dark mode state
    if (localStorage.getItem('darkMode') === 'enabled') {
        enableDarkMode();
        darkModeToggle.checked = true;
    }

    darkModeToggle.addEventListener('change', function () {
        if (this.checked) {
            enableDarkMode();
        } else {
            disableDarkMode();
        }
    });

    function enableDarkMode() {
        overlay.style.display = 'block';
        document.body.classList.add('dark-mode');
        document.querySelector('header').classList.add('dark-mode');
        document.querySelector('footer').classList.add('dark-mode');
        document.querySelector('.hieu-ung-menu').classList.add('dark-mode');
        document.querySelectorAll('nav ul li a').forEach(function (element) {
            element.classList.add('dark-mode');
        });
        document.querySelectorAll('.auth-buttons a').forEach(function (element) {
            element.classList.add('dark-mode');
        });
        localStorage.setItem('darkMode', 'enabled');
    }

    function disableDarkMode() {
        overlay.style.display = 'none';
        document.body.classList.remove('dark-mode');
        document.querySelector('header').classList.remove('dark-mode');
        document.querySelector('footer').classList.remove('dark-mode');
        document.querySelector('.hieu-ung-menu').classList.remove('dark-mode');
        document.querySelectorAll('nav ul li a').forEach(function (element) {
            element.classList.remove('dark-mode');
        });
        document.querySelectorAll('.auth-buttons a').forEach(function (element) {
            element.classList.remove('dark-mode');
        });
        localStorage.setItem('darkMode', 'disabled');
    }
});
