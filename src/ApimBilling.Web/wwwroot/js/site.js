// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Show loading overlay when navigating or submitting forms
document.addEventListener('DOMContentLoaded', function () {
    var overlay = document.getElementById('loading-overlay');

    function showLoading() {
        if (overlay) {
            overlay.style.display = 'flex';
        }
    }

    // Show overlay on form submissions (only if form passes validation)
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (form && form.tagName === 'FORM' && form.checkValidity()) {
            showLoading();
        }
    });

    // Show overlay on navigation link clicks (excludes dropdowns, tabs, anchors, etc.)
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a[href]');
        if (link &&
            link.href &&
            !link.href.startsWith('javascript:') &&
            !link.href.includes('#') &&
            !link.getAttribute('data-bs-toggle') &&
            !link.classList.contains('dropdown-toggle') &&
            link.target !== '_blank') {
            showLoading();
        }
    });

    // Hide overlay if user navigates back (browser cache/bfcache)
    window.addEventListener('pageshow', function (e) {
        if (e.persisted && overlay) {
            overlay.style.display = 'none';
        }
    });
});
