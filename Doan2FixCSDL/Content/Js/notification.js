function toggleNotificationOverlay(e) {
    var overlay = document.getElementById('notification-overlay');
    if (overlay.style.display === 'none') {
        overlay.style.display = 'block';
    } else {
        overlay.style.display = 'none';
    }
    e.stopPropagation();
}

function hideNotificationOverlay() {
    var overlay = document.getElementById('notification-overlay');
    overlay.style.display = 'none';
}

window.onclick = function (e) {
    var overlay = document.getElementById('notification-overlay');
    if (!e.target.matches('.notification-icon .fa-bell')) {
        if (overlay && overlay.style.display === 'block') {
            overlay.style.display = 'none';
        }
    }
}

function updateNotificationCount() {
    // Fetch the list of notifications
    var notificationList = document.querySelectorAll('#notification-overlay ul li');

    // Filter out "No notifications" placeholder if present
    var count = Array.from(notificationList).filter(function (item) {
        return item.textContent !== 'Không có thông báo nào cả';
    }).length;

    // Update badge display
    var badge = document.getElementById('notification-badge');
    if (count > 0) {
        badge.style.display = 'flex';
        badge.textContent = count;
    } else {
        badge.style.display = 'none';
    }
}

// Call this function on page load
document.addEventListener('DOMContentLoaded', updateNotificationCount);

function markAllAsRead() {
    // Hide the notification count
    var badge = document.getElementById('notification-badge');
    badge.style.display = 'none';

    // Clear the notification list and replace with "No notifications"
    var notificationList = document.getElementById('notification-list');
    notificationList.innerHTML = '<li>Không có thông báo nào cả</li>';

    // Update UI or notify the user as needed
    console.log('All notifications marked as read');
}
