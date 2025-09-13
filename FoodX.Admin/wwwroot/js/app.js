// Global error handler
window.addEventListener('error', function(event) {
    console.error('Global error:', event.error);
    event.preventDefault();
    return true;
});

// Handle unhandled promise rejections
window.addEventListener('unhandledrejection', function(event) {
    console.error('Unhandled promise rejection:', event.reason);
    event.preventDefault();
    return true;
});

// Blazor-specific error handling
window.addEventListener('DOMContentLoaded', function() {
    // Hide Blazor error UI by default
    const errorUi = document.getElementById('blazor-error-ui');
    if (errorUi) {
        errorUi.style.display = 'none';
        
        // Add dismiss functionality
        const dismissButton = errorUi.querySelector('.dismiss');
        if (dismissButton) {
            dismissButton.addEventListener('click', function() {
                errorUi.style.display = 'none';
            });
        }
        
        // Add reload functionality
        const reloadButton = errorUi.querySelector('.reload');
        if (reloadButton) {
            reloadButton.addEventListener('click', function() {
                window.location.reload();
            });
        }
    }
});

// Fix for text rendering issues
document.addEventListener('DOMContentLoaded', function() {
    // Force re-render of navigation text if corrupted
    setTimeout(function() {
        // Check all navigation items for corrupted text
        const navItems = document.querySelectorAll('.mud-nav-link-text, .mud-nav-group-header-text');
        navItems.forEach(function(item) {
            const text = item.textContent || item.innerText;
            if (text) {
                // Check for common corruption patterns
                if (text.includes('�') || text.includes('???') || text.includes('') || 
                    /[^\x00-\x7F]/.test(text) && !text.match(/[a-zA-Z]/)) {
                    // Try to extract clean text
                    const cleanText = text.replace(/[�\x00-\x08\x0B\x0C\x0E-\x1F\x7F]/g, '');
                    
                    // Common replacements for known issues
                    if (cleanText.includes('Data') && cleanText.includes('Admin')) {
                        item.textContent = 'Admin';
                    } else if (cleanText.includes('Super') && cleanText.includes('Admin')) {
                        item.textContent = 'SuperAdmin';
                    } else if (cleanText.length > 0) {
                        item.textContent = cleanText;
                    }
                }
                
                // Also check for specific known corruptions
                if (text.match(/D.*a.*t.*a.*A.*d.*m.*i.*n/i)) {
                    item.textContent = 'Admin';
                }
            }
        });
        
        // Force a reflow to ensure text renders correctly
        const navMenu = document.querySelector('.mud-navmenu');
        if (navMenu) {
            navMenu.style.display = 'none';
            navMenu.offsetHeight; // Trigger reflow
            navMenu.style.display = '';
        }
    }, 100);
    
    // Retry after a longer delay if needed
    setTimeout(function() {
        const navGroups = document.querySelectorAll('.mud-nav-group');
        navGroups.forEach(function(group) {
            const titleAttr = group.getAttribute('title');
            if (titleAttr === 'Admin' || titleAttr === 'SuperAdmin') {
                const textElement = group.querySelector('.mud-nav-link-text');
                if (textElement && textElement.textContent !== titleAttr) {
                    textElement.textContent = titleAttr;
                }
            }
        });
    }, 500);
});

// Ensure proper font loading
if (document.fonts && document.fonts.ready) {
    document.fonts.ready.then(function() {
        console.log('Fonts loaded successfully');
    });
}

// Fix for dropdown interaction issues on Account/Manage page
document.addEventListener('DOMContentLoaded', function() {
    // Ensure MudSelect dropdowns are interactive
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList') {
                // Check for MudSelect elements
                const selects = document.querySelectorAll('.mud-select');
                selects.forEach(function(select) {
                    // Ensure the select is clickable
                    const input = select.querySelector('.mud-input-slot, .mud-select-input');
                    if (input && !input.hasAttribute('data-click-fixed')) {
                        input.setAttribute('data-click-fixed', 'true');
                        input.style.cursor = 'pointer';
                        input.style.pointerEvents = 'auto';
                    }
                });
                
                // Fix any popover positioning issues
                const popovers = document.querySelectorAll('.mud-popover, .mud-select-popover');
                popovers.forEach(function(popover) {
                    if (!popover.style.zIndex || popover.style.zIndex < 1400) {
                        popover.style.zIndex = 1400;
                        popover.style.position = 'absolute';
                    }
                });
                
                // Ensure select items are clickable
                const selectItems = document.querySelectorAll('.mud-select-item');
                selectItems.forEach(function(item) {
                    item.style.cursor = 'pointer';
                });
            }
        });
    });
    
    // Start observing
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
    
    // Initial fix for existing elements
    setTimeout(function() {
        const selects = document.querySelectorAll('.mud-select');
        selects.forEach(function(select) {
            const input = select.querySelector('.mud-input-slot, .mud-select-input');
            if (input) {
                input.style.cursor = 'pointer';
                input.style.pointerEvents = 'auto';
            }
        });
    }, 500);
});

// File download utility functions for CSV import
window.downloadFileFromUrl = function(dataUrl, fileName) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.downloadCsvTemplate = function(csvContent, fileName) {
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Clean up the URL object
    URL.revokeObjectURL(url);
};

// Helper function to trigger file input click for AI Search
window.triggerFileInput = function(inputId) {
    const fileInput = document.getElementById(inputId);
    if (fileInput) {
        fileInput.click();
    }
};