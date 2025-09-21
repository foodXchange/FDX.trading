window.initializeCsvUpload = function (dotnetRef) {
    const dropZone = document.getElementById('csvDropZone');
    const fileInput = document.getElementById('csvFileInput');

    if (!dropZone || !fileInput) return;

    // Prevent default drag behaviors
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
        document.body.addEventListener(eventName, preventDefaults, false);
    });

    // Highlight drop zone when item is dragged over it
    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, highlight, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, unhighlight, false);
    });

    // Handle dropped files
    dropZone.addEventListener('drop', handleDrop, false);

    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    function highlight(e) {
        dropZone.classList.add('drag-hover');
    }

    function unhighlight(e) {
        dropZone.classList.remove('drag-hover');
    }

    function handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;

        if (files.length > 0) {
            const file = files[0];

            // Check if it's a CSV file
            if (file.type === 'text/csv' || file.name.endsWith('.csv')) {
                // Trigger the file input change event
                fileInput.files = files;
                const event = new Event('change', { bubbles: true });
                fileInput.dispatchEvent(event);

                // Notify Blazor component
                dotnetRef.invokeMethodAsync('HandleFileDrop', file.name);
            } else {
                alert('Please drop a CSV file');
            }
        }
    }
};

window.updateUploadProgress = function (percentage) {
    const progressBar = document.querySelector('.upload-progress-bar');
    const progressText = document.querySelector('.upload-progress-text');

    if (progressBar) {
        progressBar.style.width = percentage + '%';
    }

    if (progressText) {
        progressText.textContent = percentage + '%';
    }
};

window.showUploadSuccess = function () {
    const dropZone = document.getElementById('csvDropZone');
    if (dropZone) {
        dropZone.classList.add('upload-success');
        setTimeout(() => {
            dropZone.classList.remove('upload-success');
        }, 3000);
    }
};

window.showUploadError = function () {
    const dropZone = document.getElementById('csvDropZone');
    if (dropZone) {
        dropZone.classList.add('upload-error');
        setTimeout(() => {
            dropZone.classList.remove('upload-error');
        }, 3000);
    }
};