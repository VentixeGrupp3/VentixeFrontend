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

function initializeModals() {
    document.querySelectorAll('[data-target]').forEach(trigger => {
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            const targetSelector = this.getAttribute('data-target');
            const modal = document.querySelector(targetSelector);
            if (modal) {
                openModal(modal);
            }
        });
    });

    document.querySelectorAll('.close, [data-dismiss="modal"]').forEach(closeBtn => {
        closeBtn.addEventListener('click', function(e) {
            e.preventDefault();
            const modal = this.closest('.modal');
            if (modal) {
                closeModal(modal);
            }
        });
    });

    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('click', function(e) {
            if (e.target === this) {
                closeModal(this);
            }
        });
    });

    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const openModal = document.querySelector('.modal[style*="block"]');
            if (openModal) {
                closeModal(openModal);
            }
        }
    });

    document.querySelectorAll('.modal form').forEach(form => {
        form.addEventListener('submit', function(e) {
            console.log('Form submitting normally');
        });
    });
}

function openModal(modal) {
    modal.style.display = 'block';
    document.body.style.overflow = 'hidden';
}

function closeModal(modal) {
    modal.style.display = 'none';
    document.body.style.overflow = '';
}

window.SiteUtils = {
    fadeOutAlert,
    validateInput,
    scrollToElement,
    showLoading,
    hideLoading,
    openModal,
    closeModal
};