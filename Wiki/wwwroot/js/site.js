function toggleSidebar(force) {
    var sidebar = document.getElementById('atomicSidebar');
    var overlay = document.getElementById('atomicOverlay');

    var willOpen;
    if (typeof force === 'boolean') {
        willOpen = force;
    } else {
        willOpen = !sidebar.classList.contains('open');
    }

    if (willOpen) {
        sidebar.classList.add('open');
        overlay.classList.add('show');
    } else {
        sidebar.classList.remove('open');
        overlay.classList.remove('show');
    }
}
