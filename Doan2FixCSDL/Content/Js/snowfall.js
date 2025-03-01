document.addEventListener('DOMContentLoaded', function () {
    const toggleSnowfall = document.getElementById('toggle2');
    const body = document.body;

    function createSnowflake() {
        const snowflake = document.createElement('div');
        snowflake.classList.add('snowflake');
        snowflake.style.left = `${Math.random() * 100}vw`;
        snowflake.style.animationDuration = `${Math.random() * 3 + 5}s`; // Random duration between 5s and 8s
        snowflake.style.opacity = `${Math.random() * 0.5 + 0.5}`; // Ensuring they are always fairly bright
        body.appendChild(snowflake);

        // Remove snowflake after it falls
        setTimeout(() => {
            snowflake.remove();
            if (body.classList.contains('snowing')) {
                createSnowflake(); // Recursively create new snowflakes
            }
        }, (Math.random() * 3 + 5) * 1000); // Match the duration with animation
    }

    function startSnowfall() {
        body.classList.add('snowing');
        for (let i = 0; i < 100; i++) {
            setTimeout(createSnowflake, i * 200); // Staggered creation
        }
    }

    function stopSnowfall() {
        body.classList.remove('snowing');
        document.querySelectorAll('.snowflake').forEach(snowflake => snowflake.remove());
    }

    function updateSnowfallState(isEnabled) {
        if (isEnabled) {
            startSnowfall();
            localStorage.setItem('snowfallEnabled', 'true');
        } else {
            stopSnowfall();
            localStorage.setItem('snowfallEnabled', 'false');
        }
    }

    // Restore snowfall state on page load
    const isSnowfallEnabled = localStorage.getItem('snowfallEnabled') === 'true';
    toggleSnowfall.checked = isSnowfallEnabled;
    if (isSnowfallEnabled) {
        startSnowfall();
    }

    // Listen for toggle changes
    toggleSnowfall.addEventListener('change', function () {
        updateSnowfallState(toggleSnowfall.checked);
    });
});
