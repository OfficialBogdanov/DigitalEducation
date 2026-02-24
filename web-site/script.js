const themeToggle = document.getElementById('theme-toggle');
const body = document.body;
const heroImage = document.querySelector('.hero-screenshot');
const lightbox = document.getElementById('lightbox');
const lightboxImg = document.getElementById('lightbox-img');
const closeBtn = document.querySelector('.close');
const backToTop = document.getElementById('backToTop');

function updateIcons() {
    const isDark = body.classList.contains('theme-dark');
    const folder = isDark ? 'White' : 'Black';
    const icons = document.querySelectorAll('img.theme-icon');
    icons.forEach(img => {
        const iconName = img.dataset.icon;
        if (iconName) {
            img.src = `../DigitalEducation/Assets/Icons/${folder}/${iconName}.png`;
        }
    });
}

function setTheme(theme) {
    body.className = theme;
    localStorage.setItem('theme', theme);
    updateIcons();
}

function toggleTheme() {
    const currentTheme = body.classList.contains('theme-light') ? 'theme-light' : 'theme-dark';
    const newTheme = currentTheme === 'theme-light' ? 'theme-dark' : 'theme-light';
    setTheme(newTheme);
}

const savedTheme = localStorage.getItem('theme');
if (savedTheme) {
    setTheme(savedTheme);
} else {
    setTheme('theme-light');
}

themeToggle.addEventListener('click', toggleTheme);

heroImage.addEventListener('click', () => {
    lightboxImg.src = heroImage.src;
    lightbox.classList.add('show');
});

function closeLightbox() {
    lightbox.classList.remove('show');
}

closeBtn.addEventListener('click', closeLightbox);

lightbox.addEventListener('click', (e) => {
    if (e.target === lightbox) {
        closeLightbox();
    }
});

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && lightbox.classList.contains('show')) {
        closeLightbox();
    }
});

window.addEventListener('scroll', () => {
    if (window.scrollY > 300) {
        backToTop.classList.remove('hidden');
    } else {
        backToTop.classList.add('hidden');
    }
});

backToTop.addEventListener('click', () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
});

backToTop.classList.add('hidden');