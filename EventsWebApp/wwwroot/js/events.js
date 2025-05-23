document.addEventListener('DOMContentLoaded', function() {
    initializeSearch();
    initializeFilters();
    initializeViewToggle();
    initializeTabFilters();
    
    console.log('Events page functionality initialized');
});

function initializeSearch() {
    const searchInput = document.querySelector('.search-input');
    if (!searchInput) return;
    
    let searchTimeout;
    searchInput.addEventListener('input', function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            filterEvents();
        }, 300); // 300ms delay to avoid excessive filtering
    });
}

function initializeFilters() {
    const categoryFilter = document.querySelector('.filter-dropdown');
    if (!categoryFilter) return;
    
    categoryFilter.addEventListener('change', filterEvents);
}

function initializeViewToggle() {
    const viewToggleBtns = document.querySelectorAll('.view-toggle-btn');
    if (viewToggleBtns.length === 0) return;
    
    viewToggleBtns.forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const view = this.dataset.view;
            toggleView(view, this);
        });
    });
}

function initializeTabFilters() {
    const tabItems = document.querySelectorAll('.tab-item');
    if (tabItems.length === 0) return;
    
    tabItems.forEach(tab => {
        tab.addEventListener('click', function(e) {
            e.preventDefault();
            
            tabItems.forEach(t => t.classList.remove('active'));
            
            this.classList.add('active');
            
            const status = this.dataset.status;
            filterEventsByStatus(status);
        });
    });
}

function filterEvents() {
    const eventsContainer = document.getElementById('eventsContainer');
    if (!eventsContainer) return;
    
    const searchInput = document.querySelector('.search-input');
    const categoryFilter = document.querySelector('.filter-dropdown');
    
    const searchTerm = searchInput ? searchInput.value.toLowerCase().trim() : '';
    const selectedCategory = categoryFilter ? categoryFilter.value : '';
    const eventCards = eventsContainer.querySelectorAll('.event-card');
    
    let visibleCount = 0;
    
    eventCards.forEach(card => {
        const titleElement = card.querySelector('.event-card-title');
        const categoryElement = card.querySelector('.category-badge');
        
        if (!titleElement) return;
        
        const title = titleElement.textContent.toLowerCase();
        const category = categoryElement ? categoryElement.textContent.trim() : '';
        
        const matchesSearch = !searchTerm || title.includes(searchTerm);
        const matchesCategory = !selectedCategory || category === selectedCategory;
        
        if (matchesSearch && matchesCategory) {
            card.style.display = '';
            visibleCount++;
        } else {
            card.style.display = 'none';
        }
    });
    
    updateEventsCount(visibleCount);
}

function filterEventsByStatus(status) {
    const eventsContainer = document.getElementById('eventsContainer');
    if (!eventsContainer) return;
    
    const eventCards = eventsContainer.querySelectorAll('.event-card');
    
    eventCards.forEach(card => {
        card.style.display = '';
    });
}

function toggleView(view, clickedBtn) {
    const eventsContainer = document.getElementById('eventsContainer');
    if (!eventsContainer) return;
    
    const viewToggleBtns = document.querySelectorAll('.view-toggle-btn');
    viewToggleBtns.forEach(btn => btn.classList.remove('active'));
    
    clickedBtn.classList.add('active');
    
    if (view === 'list') {
        eventsContainer.classList.remove('events-grid');
        eventsContainer.classList.add('events-list');
    } else {
        eventsContainer.classList.remove('events-list');
        eventsContainer.classList.add('events-grid');
    }
}

function updateEventsCount(count) {
    const paginationText = document.querySelector('.pagination span');
    if (paginationText) {
        paginationText.textContent = `Showing ${count} events`;
    }
}

function handleEventCardClick(eventId) {
    console.log('Event card clicked:', eventId);
}

window.EventsPage = {
    filterEvents,
    toggleView,
    updateEventsCount
};