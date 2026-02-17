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
    
    // Создаем уникальный ключ для localStorage на основе URL страницы
    // Убираем параметры запроса, чтобы фильтры не влияли на ключ
    const urlWithoutParams = window.location.pathname;
    const storageKey = 'autoRefreshEnabled_' + urlWithoutParams.replace(/\//g, '_');

    // Восстанавливаем состояние из localStorage с уникальным ключом
    const savedState = localStorage.getItem(storageKey);
    if (savedState === 'true') {
        checkbox.checked = true;
    } else {
        checkbox.checked = false;
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
        }, 30000); // 30 секунд 
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
            localStorage.setItem(storageKey, 'true');
        } else {
            stopAutoRefresh();
            localStorage.setItem(storageKey, 'false');
        }
    });

    // Если при загрузке страницы галочка уже включена — запускаем сразу
    if (checkbox.checked) {
        startAutoRefresh();
    }
}

// Запускаем инициализацию после загрузки DOM
document.addEventListener('DOMContentLoaded', initAutoRefresh);