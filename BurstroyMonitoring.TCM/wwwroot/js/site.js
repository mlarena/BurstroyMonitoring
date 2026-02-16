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


// Автообновление таблицы на страницах Index (DovData, DspdData, DustData и др.)
function initAutoRefresh() {
    const checkbox = document.getElementById('autoRefresh');
    if (!checkbox) {
        // Если чекбокса нет на странице — выходим
        return;
    }

    const tableContainer = document.querySelector('.table-responsive');
    if (!tableContainer) {
        console.warn('Контейнер таблицы (.table-responsive) не найден');
        return;
    }

    const currentUrl = window.location.href;

    // Восстанавливаем состояние из localStorage
    const savedState = localStorage.getItem('autoRefreshEnabled');
    if (savedState === 'true') {
        checkbox.checked = true;
    }

    let intervalId = null;

    function startAutoRefresh() {
        if (intervalId) clearInterval(intervalId);

        intervalId = setInterval(() => {
            fetch(currentUrl, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }
                return response.text();
            })
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newTable = doc.querySelector('.table-responsive');

                if (newTable) {
                    tableContainer.innerHTML = newTable.innerHTML;
                    console.log('Таблица обновлена автоматически');
                } else {
                    console.warn('Не удалось найти новую таблицу в ответе сервера');
                }
            })
            .catch(err => {
                console.error('Ошибка при автообновлении таблицы:', err);
            });
        }, 3000); // 30 секунд
    }

    function stopAutoRefresh() {
        if (intervalId) {
            clearInterval(intervalId);
            intervalId = null;
        }
    }

    // Обработчик изменения состояния чекбокса
    checkbox.addEventListener('change', function () {
        if (this.checked) {
            startAutoRefresh();
            localStorage.setItem('autoRefreshEnabled', 'true');
        } else {
            stopAutoRefresh();
            localStorage.setItem('autoRefreshEnabled', 'false');
        }
    });

    // Если при загрузке страницы галочка уже включена — запускаем сразу
    if (checkbox.checked) {
        startAutoRefresh();
    }
}

// Запускаем инициализацию после загрузки DOM
document.addEventListener('DOMContentLoaded', initAutoRefresh);