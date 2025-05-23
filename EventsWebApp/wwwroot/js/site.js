document.addEventListener('DOMContentLoaded', function() {
    initializeAlerts();
    initializeFormsValidation();
    
    console.log('Site loaded and initialized');
});

function initializeAlerts() {
    const alerts = document.querySelectorAll('.alert');
    
    alerts.forEach(alert => {
        if (alert.classList.contains('alert-success')) {
            setTimeout(() => {
                fadeOutAlert(alert);
            }, 5000);
        }
        
        alert.addEventListener('click', function() {
            fadeOutAlert(this);
        });
        
        alert.setAttribute('role', 'status');
        alert.setAttribute('tabindex', '0');
    });
}

function initializeFormsValidation() {
    const forms = document.querySelectorAll('form');
    
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = this.querySelector('input[type="submit"], button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.textContent = 'Saving...';
                submitBtn.disabled = true;
            }
        });
        
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateInput(this);
            });
        });
    });
}

function fadeOutAlert(alert) {
    alert.style.transition = 'opacity 0.3s ease-out';
    alert.style.opacity = '0';
    
    setTimeout(() => {
        if (alert.parentNode) {
            alert.parentNode.removeChild(alert);
        }
    }, 300);
}

function validateInput(input) {
    const isValid = input.checkValidity();
    
    input.classList.remove('input-validation-error', 'input-validation-success');
    
    if (input.value.trim() !== '') {
        input.classList.add(isValid ? 'input-validation-success' : 'input-validation-error');
    }
}

function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }
}

function showLoading(element) {
    if (element) {
        element.classList.add('loading');
        element.setAttribute('aria-busy', 'true');
    }
}

function hideLoading(element) {
    if (element) {
        element.classList.remove('loading');
        element.removeAttribute('aria-busy');
    }
}

window.SiteUtils = {
    fadeOutAlert,
    validateInput,
    scrollToElement,
    showLoading,
    hideLoading
};