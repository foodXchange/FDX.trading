// Lazy loading module for images
export function initLazyLoad(imageElement, dotNetRef) {
    if (!imageElement) return;
    
    // Check if Intersection Observer is supported
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    const src = img.dataset.src;
                    
                    if (src) {
                        // Create a new image to preload
                        const tempImg = new Image();
                        
                        tempImg.onload = () => {
                            img.src = src;
                            img.classList.add('loaded');
                            img.removeAttribute('data-src');
                            
                            // Notify Blazor component if needed
                            if (dotNetRef) {
                                dotNetRef.invokeMethodAsync('OnImageVisible');
                            }
                        };
                        
                        tempImg.onerror = () => {
                            img.classList.add('error');
                        };
                        
                        tempImg.src = src;
                    }
                    
                    observer.unobserve(img);
                }
            });
        }, {
            rootMargin: '50px 0px',
            threshold: 0.01
        });
        
        imageObserver.observe(imageElement);
    } else {
        // Fallback for browsers that don't support Intersection Observer
        const src = imageElement.dataset.src;
        if (src) {
            imageElement.src = src;
            imageElement.classList.add('loaded');
        }
    }
}

// Batch lazy load for multiple images
export function initBatchLazyLoad(selector) {
    const images = document.querySelectorAll(selector || 'img[data-src]');
    
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    const src = img.dataset.src;
                    
                    if (src) {
                        img.src = src;
                        img.classList.add('loaded');
                        img.removeAttribute('data-src');
                        observer.unobserve(img);
                    }
                }
            });
        }, {
            rootMargin: '50px 0px',
            threshold: 0.01
        });
        
        images.forEach(img => imageObserver.observe(img));
    } else {
        // Fallback for older browsers
        images.forEach(img => {
            const src = img.dataset.src;
            if (src) {
                img.src = src;
                img.classList.add('loaded');
            }
        });
    }
}

// Export for global use
window.lazyLoadImages = {
    init: initBatchLazyLoad
};