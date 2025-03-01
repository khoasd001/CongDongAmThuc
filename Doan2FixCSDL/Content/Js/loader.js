// Show loader when the page starts loading
window.addEventListener('beforeunload', function () {
    document.getElementById('loading-overlay').style.display = 'flex';
});

// Hide loader when the page is fully loaded
window.addEventListener('load', function () {
    document.getElementById('loading-overlay').style.display = 'none';
});
