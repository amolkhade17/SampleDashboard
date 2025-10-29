// Dashboard JavaScript
$(document).ready(function() {
    // Sidebar Toggle
    $('#sidebarToggle').click(function() {
        $('#sidebar').toggleClass('collapsed');
        $('.main-content').toggleClass('expanded');
    });

    // Mobile Sidebar Toggle
    if ($(window).width() < 768) {
        $('#sidebarToggle').click(function() {
            $('#sidebar').toggleClass('active');
        });
    }

    // Active Menu Highlighting
    const currentPath = window.location.pathname;
    $('.sidebar .nav-link').each(function() {
        const href = $(this).attr('href');
        if (currentPath.includes(href) && href !== '/') {
            $('.sidebar .nav-link').removeClass('active');
            $(this).addClass('active');
        }
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert').fadeOut('slow');
    }, 5000);

    // Form validation enhancement
    $('form').submit(function() {
        const btn = $(this).find('button[type="submit"]');
        btn.prop('disabled', true);
        btn.html('<i class="fas fa-spinner fa-spin me-2"></i>Processing...');
    });

    // Tooltip initialization
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Confirmation dialogs
    $('.btn-danger').click(function(e) {
        if ($(this).data('confirm')) {
            if (!confirm('Are you sure you want to perform this action?')) {
                e.preventDefault();
            }
        }
    });
});

// Window resize handler
$(window).resize(function() {
    if ($(window).width() > 768) {
        $('#sidebar').removeClass('active');
    }
});
