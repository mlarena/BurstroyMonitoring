(function() {
    const THEME_KEY = 'bsp-theme';
    const LIGHT_THEME = 'light';
    const DARK_THEME = 'dark';

    // Функция для определения системной темы
    function getSystemTheme() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? DARK_THEME : LIGHT_THEME;
    }

    // Функция для получения сохраненной темы
    function getStoredTheme() {
        return localStorage.getItem(THEME_KEY);
    }

    // Функция для применения темы
    function setTheme(theme) {
        const body = document.body;
        const html = document.documentElement;
        
        // Удаляем все классы темы
        body.classList.remove('theme-light', 'theme-dark');
        html.classList.remove('theme-light', 'theme-dark');
        
        // Добавляем новый класс темы
        body.classList.add(`theme-${theme}`);
        html.classList.add(`theme-${theme}`); // Добавляем и на html для надежности
        
        // Сохраняем тему в localStorage
        localStorage.setItem(THEME_KEY, theme);
        
        // Обновляем текст кнопки
        updateToggleButtonText(theme);
        
        // Добавляем атрибут data-theme для возможных CSS правил
        body.setAttribute('data-theme', theme);
        html.setAttribute('data-theme', theme);
        
        // Триггерим событие для других скриптов
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    // Функция для обновления текста кнопки
    function updateToggleButtonText(theme) {
        const toggleBtn = document.getElementById('theme-toggle');
        if (toggleBtn) {
            toggleBtn.textContent = theme === LIGHT_THEME ? '🌙 Темная тема' : '☀️ Светлая тема';
            toggleBtn.title = 'Правый клик для сброса к системной теме';
        }
    }

    // Функция для переключения темы
    function toggleTheme() {
        const currentTheme = document.body.classList.contains('theme-light') ? LIGHT_THEME : DARK_THEME;
        const newTheme = currentTheme === LIGHT_THEME ? DARK_THEME : LIGHT_THEME;
        setTheme(newTheme);
    }

    // Функция для сброса темы к системной
    function resetToSystemTheme() {
        localStorage.removeItem(THEME_KEY);
        const systemTheme = getSystemTheme();
        setTheme(systemTheme);
    }

    // Функция для инициализации темы
    function initTheme() {
        // Сначала проверяем сохраненную тему
        let theme = getStoredTheme();
        
        // Если нет сохраненной темы, используем системную
        if (!theme) {
            theme = getSystemTheme();
        }
        
        // Применяем тему
        setTheme(theme);
        
        // Создаем наблюдатель за изменением системной темы
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        
        // Используем современный API
        if (mediaQuery.addEventListener) {
            mediaQuery.addEventListener('change', (e) => {
                // Меняем тему только если нет сохраненной пользовательской темы
                if (!getStoredTheme()) {
                    const newSystemTheme = e.matches ? DARK_THEME : LIGHT_THEME;
                    setTheme(newSystemTheme);
                }
            });
        } else {
            // Fallback для старых браузеров
            mediaQuery.addListener((e) => {
                if (!getStoredTheme()) {
                    const newSystemTheme = e.matches ? DARK_THEME : LIGHT_THEME;
                    setTheme(newSystemTheme);
                }
            });
        }
    }

    // Инициализация при загрузке страницы
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            initTheme();
            
            const toggleBtn = document.getElementById('theme-toggle');
            if (toggleBtn) {
                toggleBtn.addEventListener('click', toggleTheme);
                
                // Добавляем контекстное меню для сброса к системной теме
                toggleBtn.addEventListener('contextmenu', (e) => {
                    e.preventDefault();
                    resetToSystemTheme();
                });
            }
            
            // Добавляем стили для плавного перехода при загрузке
            document.body.style.transition = 'background-color 0.3s ease, color 0.3s ease, border-color 0.3s ease, box-shadow 0.3s ease';
        });
    } else {
        initTheme();
        
        const toggleBtn = document.getElementById('theme-toggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', toggleTheme);
            toggleBtn.addEventListener('contextmenu', (e) => {
                e.preventDefault();
                resetToSystemTheme();
            });
        }
    }

    // Экспортируем функции для возможного использования в других скриптах
    window.themeManager = {
        setTheme,
        toggleTheme,
        resetToSystemTheme,
        getCurrentTheme: () => document.body.classList.contains('theme-light') ? LIGHT_THEME : DARK_THEME,
        getSystemTheme
    };
})();

//---------------новое
// Базовый JavaScript для всего приложения

// Функция для показа/скрытия загрузки
function showLoading(selector) {
    $(selector).html('<div class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Загрузка...</span></div></div>');
}

// Функция для форматирования даты
function formatDate(date) {
    if (!date) return 'Нет данных';
    return new Date(date).toLocaleString('ru-RU');
}

// Инициализация при загрузке страницы
$(document).ready(function() {
    console.log('Приложение загружено');
    
    // Добавление обработчиков для всех форм с классом .ajax-form
    $('.ajax-form').on('submit', function(e) {
        e.preventDefault();
        const form = $(this);
        const url = form.attr('action');
        const method = form.attr('method') || 'POST';
        const data = form.serialize();
        
        $.ajax({
            url: url,
            type: method,
            data: data,
            success: function(response) {
                if (form.data('target')) {
                    $(form.data('target')).html(response);
                }
            },
            error: function(xhr) {
                alert('Произошла ошибка: ' + xhr.statusText);
            }
        });
    });
});