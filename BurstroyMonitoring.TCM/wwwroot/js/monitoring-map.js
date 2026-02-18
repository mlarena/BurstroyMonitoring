// Глобальные переменные
let map;
let markers = [];
let isDetailsCollapsed = false;
let isExpanded = false;
let searchTimeout = null;
let selectedAutocompleteIndex = -1;

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    initMap();
    loadMapData();
    initSearch();
    
    // Инициализация панели деталей
    const panel = document.getElementById('monitoringDetailsPanel');
    const clearButton = panel?.querySelector('.monitoring-button.monitoring-button-secondary');
    
    if (panel && !isDetailsCollapsed) {
        panel.style.height = 'calc(100% - 20px)';
        if (clearButton) {
            clearButton.style.display = 'inline-flex';
        }
    }
    
    // ОТЛАДКА: Проверяем все датчики в DOM после загрузки
    setTimeout(() => {
        console.log('=== ОТЛАДКА: Список датчиков в DOM ===');
        document.querySelectorAll('.monitoring-sensor-item').forEach((item, index) => {
            console.log(`Датчик ${index + 1}:`, {
                id: item.getAttribute('onclick')?.match(/\d+/)?.[0],
                sensorTypeId: item.getAttribute('data-sensor-type'),
                sensorName: item.getAttribute('data-sensor-name'),
                html: item.outerHTML.substring(0, 200) + '...'
            });
        });
    }, 1000);
});

function initMap() {
    // Центр России
    map = L.map('monitoring-map').setView([55.7558, 37.6173], 5);

    // Добавляем слой карты
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);
    
    // Сохраняем карту в глобальной переменной
    window.monitoringMap = map;
}

function initSearch() {
    const searchInput = document.getElementById('postSearch');
    const autocomplete = document.getElementById('autocompleteResults');
    
    searchInput.addEventListener('input', function(e) {
        clearTimeout(searchTimeout);
        selectedAutocompleteIndex = -1;
        
        const query = e.target.value.trim();
        if (query.length < 2) {
            autocomplete.style.display = 'none';
            return;
        }
        
        searchTimeout = setTimeout(() => {
            searchPosts(query);
        }, 300);
    });
    
    // Обработка нажатия клавиш
    searchInput.addEventListener('keydown', function(e) {
        const autocomplete = document.getElementById('autocompleteResults');
        const items = autocomplete.querySelectorAll('.monitoring-autocomplete-item');
        
        if (autocomplete.style.display !== 'block' || items.length === 0) {
            return;
        }
        
        switch(e.key) {
            case 'ArrowDown':
                e.preventDefault();
                navigateAutocomplete(1);
                break;
            case 'ArrowUp':
                e.preventDefault();
                navigateAutocomplete(-1);
                break;
            case 'Enter':
                e.preventDefault();
                selectAutocompleteItem();
                break;
            case 'Escape':
                autocomplete.style.display = 'none';
                searchInput.value = '';
                break;
        }
    });
    
    // Закрываем автодополнение при клике вне
    document.addEventListener('click', function(e) {
        if (!searchInput.contains(e.target) && !autocomplete.contains(e.target)) {
            autocomplete.style.display = 'none';
        }
    });
}

function navigateAutocomplete(direction) {
    const items = document.querySelectorAll('.monitoring-autocomplete-item');
    if (items.length === 0) return;
    
    // Снимаем выделение с текущего элемента
    if (selectedAutocompleteIndex >= 0 && selectedAutocompleteIndex < items.length) {
        items[selectedAutocompleteIndex].classList.remove('selected');
    }
    
    // Вычисляем новый индекс
    selectedAutocompleteIndex += direction;
    
    // Зацикливаем навигацию
    if (selectedAutocompleteIndex < 0) {
        selectedAutocompleteIndex = items.length - 1;
    } else if (selectedAutocompleteIndex >= items.length) {
        selectedAutocompleteIndex = 0;
    }
    
    // Добавляем выделение новому элементу
    items[selectedAutocompleteIndex].classList.add('selected');
    
    // Прокручиваем к выделенному элементу
    items[selectedAutocompleteIndex].scrollIntoView({
        block: 'nearest',
        behavior: 'smooth'
    });
}

function selectAutocompleteItem() {
    const items = document.querySelectorAll('.monitoring-autocomplete-item');
    if (items.length === 0 || selectedAutocompleteIndex < 0) return;
    
    const selectedItem = items[selectedAutocompleteIndex];
    const id = selectedItem.getAttribute('data-id');
    const lat = parseFloat(selectedItem.getAttribute('data-lat'));
    const lng = parseFloat(selectedItem.getAttribute('data-lng'));
    
    if (id && lat && lng) {
        selectPost(id, lat, lng);
    }
}

async function searchPosts(query) {
    try {
        const response = await fetch(`/MonitoringMap/SearchPosts?query=${encodeURIComponent(query)}`);
        if (response.ok) {
            const posts = await response.json();
            displayAutocomplete(posts);
        }
    } catch (error) {
        console.error('Ошибка поиска:', error);
    }
}

function displayAutocomplete(posts) {
    const autocomplete = document.getElementById('autocompleteResults');
    
    if (posts.length === 0) {
        autocomplete.innerHTML = '<div class="monitoring-autocomplete-item">Ничего не найдено</div>';
        autocomplete.style.display = 'block';
        return;
    }
    
    let html = '';
    posts.forEach((post, index) => {
        html += `
            <div class="monitoring-autocomplete-item" 
                 data-id="${post.id}"
                 data-lat="${post.latitude}"
                 data-lng="${post.longitude}"
                 onclick="selectPost(${post.id}, ${post.latitude}, ${post.longitude})"
                 onmouseover="hoverAutocompleteItem(this)">
                ${post.name}
            </div>
        `;
    });
    
    autocomplete.innerHTML = html;
    autocomplete.style.display = 'block';
}

function hoverAutocompleteItem(element) {
    // Снимаем выделение со всех элементов
    document.querySelectorAll('.monitoring-autocomplete-item').forEach(item => {
        item.classList.remove('selected');
    });
    
    // Добавляем выделение наведенному элементу
    element.classList.add('selected');
    
    // Обновляем индекс выбранного элемента
    const items = Array.from(document.querySelectorAll('.monitoring-autocomplete-item'));
    selectedAutocompleteIndex = items.indexOf(element);
}

function selectPost(id, lat, lng) {
    // Закрываем автодополнение
    const autocomplete = document.getElementById('autocompleteResults');
    autocomplete.style.display = 'none';
    
    // Очищаем поле поиска
    document.getElementById('postSearch').value = '';
    selectedAutocompleteIndex = -1;
    
    // Перемещаем карту к выбранному посту
    map.setView([lat, lng], 13);
    
    // Показываем детали поста
    showDetails(id, 'post');
    
    // Подсвечиваем маркер
    highlightMarker(id, 'post');
}

function highlightMarker(id, type) {
    markers.forEach(item => {
        if (item.type === type && item.data.id === id) {
            const marker = item.marker;
            marker.openPopup();
            
            // Временное увеличение маркера
            const icon = marker.getIcon();
            if (icon.options.html) {
                const tempHtml = icon.options.html.replace('24px', '30px').replace('12px', '15px');
                marker.setIcon(L.divIcon({
                    ...icon.options,
                    html: tempHtml,
                    iconSize: [30, 30],
                    iconAnchor: [15, 15]
                }));
                
                // Возвращаем исходный размер через 2 секунды
                setTimeout(() => {
                    marker.setIcon(icon);
                }, 2000);
            }
        }
    });
}

async function loadMapData() {
    try {
        const response = await fetch('/MonitoringMap/GetMapData');
        const data = await response.json();
        
        clearMarkers();
        
        // Добавляем все посты (и активные и неактивные)
        data.posts.forEach(post => {
            if (post.latitude && post.longitude) {
                addPostMarker(post);
            }
        });
        
        // Добавляем все датчики без постов (и активные и неактивные)
        data.sensors.forEach(sensor => {
            if (sensor.latitude && sensor.longitude && sensor.monitoringPostId === null) {
                addSensorMarker(sensor);
            }
        });
        
        // Обновляем статистику в заголовке
        updateStatistics(data);
        
    } catch (error) {
        console.error('Ошибка загрузки данных:', error);
    }
}

function updateStatistics(data) {
    const activePosts = data.posts.filter(p => p.isActive).length;
    const inactivePosts = data.posts.length - activePosts;
    const activeSensors = data.sensors.filter(s => s.isActive).length;
    const inactiveSensors = data.sensors.length - activeSensors;
    
    const titleSpan = document.querySelector('.monitoring-title-count');
    if (titleSpan) {
        titleSpan.innerHTML = `(Постов: ${data.posts.length}, Датчиков: ${data.sensors.length})`;
    }
}

function addPostMarker(post) {
    // Разные цвета для активных/неактивных и мобильных/стационарных
    let bgColor;
    if (!post.isActive) {
        bgColor = '#6c757d'; // Серый для неактивных
    } else {
        bgColor = post.isMobile ? '#ff9800' : '#2196f3'; // Оранжевый для мобильных, синий для стационарных
    }
    
    const icon = L.divIcon({
        className: 'custom-div-icon',
        html: `<div style="background-color: ${bgColor}; 
                    width: 24px; height: 24px; border-radius: 50%; 
                    border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);
                    display: flex; align-items: center; justify-content: center;
                    opacity: ${post.isActive ? '1' : '0.6'};">
                <i class="fas fa-building" style="color: white; font-size: 12px;"></i>
            </div>`,
        iconSize: [24, 24],
        iconAnchor: [12, 12]
    });

    const marker = L.marker([post.latitude, post.longitude], { icon })
        .addTo(map)
        .bindPopup(`
            <div style="font-size: 13px;">
                <b>${post.name}</b><br/>
                ${post.description || ''}<br/>
                Датчиков: ${post.sensorCount}<br/>
                ${post.isMobile ? 'Мобильный' : 'Стационарный'}<br/>
                <span style="color: ${post.isActive ? '#28a745' : '#dc3545'};">
                    ${post.isActive ? 'Активен' : 'Неактивен'}
                </span>
            </div>
        `);

    marker.on('click', function() {
        showDetails(post.id, 'post');
    });

    markers.push({ marker, type: 'post', data: post });
}

function addSensorMarker(sensor) {
    // Разные цвета для разных типов датчиков и статусов
    let bgColor;
    const typeColor = getSensorTypeColor(sensor.sensorTypeId);
    
    if (!sensor.isActive) {
        bgColor = '#6c757d'; // Серый для неактивных
    } else {
        bgColor = typeColor; // Цвет по типу для активных
    }
    
    const icon = L.divIcon({
        className: 'custom-div-icon',
        html: `<div style="background-color: ${bgColor}; 
                    width: 20px; height: 20px; border-radius: 50%; 
                    border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);
                    display: flex; align-items: center; justify-content: center;
                    opacity: ${sensor.isActive ? '1' : '0.6'};">
                <i class="fas fa-microchip" style="color: white; font-size: 10px;"></i>
            </div>`,
        iconSize: [20, 20],
        iconAnchor: [10, 10]
    });

    const marker = L.marker([sensor.latitude, sensor.longitude], { icon })
        .addTo(map)
        .bindPopup(`
            <div style="font-size: 13px;">
                <b>${sensor.name}</b><br/>
                ${sensor.serialNumber}<br/>
                Тип: ${getSensorTypeName(sensor.sensorTypeId)}<br/>
                <span style="color: ${sensor.isActive ? '#28a745' : '#dc3545'};">
                    ${sensor.isActive ? 'Активен' : 'Неактивен'}
                </span>
            </div>
        `);

    marker.on('click', function() {
        showDetails(sensor.id, 'sensor');
    });

    markers.push({ marker, type: 'sensor', data: sensor });
}

function getSensorTypeColor(typeId) {
    switch(typeId) {
        case 1: return '#4caf50'; // DSPD - зеленый
        case 2: return '#2196f3'; // IWS - синий
        case 3: return '#ff9800'; // DOV - оранжевый
        case 4: return '#9c27b0'; // DUST - фиолетовый
        case 5: return '#f44336'; // MUEKS - красный
        default: return '#607d8b'; // серый для неизвестных
    }
}

function getSensorTypeName(typeId) {
    switch(typeId) {
        case 1: return 'DSPD';
        case 2: return 'IWS';
        case 3: return 'DOV';
        case 4: return 'DUST';
        case 5: return 'MUEKS';
        default: return 'Неизвестный';
    }
}

function clearMarkers() {
    markers.forEach(item => map.removeLayer(item.marker));
    markers = [];
}

async function showDetails(id, type) {
    try {
        // Раскрываем панель, если она свернута
        if (isDetailsCollapsed) {
            toggleDetailsPanel();
        }
        
        const response = await fetch(`/MonitoringMap/GetDetails?id=${id}&type=${type}`);
        if (response.ok) {
            const html = await response.text();
            document.getElementById('monitoringDetailsContent').innerHTML = html;
            
            // Показываем панель, если скрыта
            const panel = document.getElementById('monitoringDetailsPanel');
            panel.classList.remove('hidden');
            
            // Загружаем данные датчиков, если это пост
            if (type === 'post') {
                await loadSensorData(id);
            }
        }
    } catch (error) {
        console.error('Ошибка загрузки деталей:', error);
    }
}

async function loadSensorData(postId) {
    try {
        const response = await fetch(`/MonitoringMap/GetSensorData?postId=${postId}`);
        if (response.ok) {
            const html = await response.text();
            const sensorList = document.getElementById('sensorList');
            if (sensorList) {
                sensorList.innerHTML = html;
            }
        }
    } catch (error) {
        console.error('Ошибка загрузки данных датчиков:', error);
        const sensorList = document.getElementById('sensorList');
        if (sensorList) {
            sensorList.innerHTML = '<span class="text-danger">Ошибка загрузки данных датчиков</span>';
        }
    }
}

// Функция для показа данных датчика
async function showSensorDetails(sensorId, element) {
    // Снимаем активный класс со всех элементов
    document.querySelectorAll('.monitoring-sensor-item').forEach(item => {
        item.classList.remove('active');
    });
    
    // Добавляем активный класс текущему элементу
    element.classList.add('active');
    
    // Получаем данные из атрибутов
    const sensorTypeId = element.getAttribute('data-sensor-type');
    const sensorName = element.getAttribute('data-sensor-name');
    
    // Показываем загрузку
    const sensorContent = document.getElementById('sensorDataContent');
    sensorContent.innerHTML = `
        <div class="loading-spinner">
            <i class="fas fa-spinner fa-spin"></i> Загрузка данных датчика...
        </div>
    `;
    
    try {
        const response = await fetch(`/MonitoringMap/GetLatestSensorData?sensorId=${sensorId}&sensorTypeId=${sensorTypeId}`);
        
        if (response.ok) {
            const html = await response.text();
            
            // Создаем временный контейнер для парсинга HTML
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;
            
            // Извлекаем временную метку
            let timestamp = '';
            const timestampElement = tempDiv.querySelector('.timestamp-value');
            if (timestampElement) {
                timestamp = timestampElement.textContent.trim();
            }
            
            // Обновляем заголовок с временем
            const sensorTitle = document.getElementById('sensorTitle');
            if (sensorTitle) {
                if (timestamp) {
                    sensorTitle.innerHTML = `${sensorName} (ID: ${sensorId}, тип: ${sensorTypeId}) <i class="far fa-clock" style="margin-left: 10px;"></i> ${timestamp}`;
                } else {
                    sensorTitle.innerHTML = `${sensorName} (ID: ${sensorId}, тип: ${sensorTypeId})`;
                }
            }
            
            // Удаляем строку с временной меткой из HTML
            let cleanHtml = html.replace(/<tr class="timestamp-row">[\s\S]*?<\/tr>/, '');
            
            sensorContent.innerHTML = cleanHtml;
        } else {
            sensorContent.innerHTML = `
                <div class="no-data-message">
                    <i class="fas fa-exclamation-triangle"></i> Ошибка загрузки данных
                </div>
            `;
        }
    } catch (error) {
        console.error('Ошибка:', error);
        sensorContent.innerHTML = `
            <div class="no-data-message">
                <i class="fas fa-times-circle"></i> Ошибка соединения
            </div>
        `;
    }
}

// Обновляем функцию clearSensorData
function clearSensorData(event) {
    if (event) {
        event.stopPropagation();
    }
    
    const sensorContent = document.getElementById('sensorDataContent');
    sensorContent.innerHTML = `
        <div class="monitoring-no-selection">
            <i class="fas fa-chart-line"></i>
            <p>Нажмите на датчик в списке для просмотра данных</p>
        </div>
    `;
    
    // Сбрасываем заголовок
    const sensorTitle = document.getElementById('sensorTitle');
    if (sensorTitle) {
        sensorTitle.innerHTML = 'Данные датчика';
    }
}

function clearDetails(event) {
    if (event) {
        event.stopPropagation();
    }
    
    document.getElementById('monitoringDetailsContent').innerHTML = `
        <div class="monitoring-no-selection">
            <i class="fas fa-mouse-pointer"></i>
            <p>Выберите объект на карте для просмотра деталей</p>
        </div>
    `;
}

function toggleDetailsPanel() {
    const panel = document.getElementById('monitoringDetailsPanel');
    const content = document.getElementById('monitoringDetailsContent');
    const clearButton = panel.querySelector('.monitoring-button.monitoring-button-secondary');
    
    if (isDetailsCollapsed) {
        // Раскрываем панель
        panel.classList.remove('collapsed');
        content.classList.remove('collapsed');
        
        // Показываем кнопку очистки
        if (clearButton) {
            clearButton.style.display = 'inline-flex';
        }
        
        // Восстанавливаем сохраненную высоту или используем стандартную
        if (window.savedPanelHeight) {
            panel.style.height = window.savedPanelHeight;
        } else {
            panel.style.height = 'calc(100% - 20px)';
        }
        
        isDetailsCollapsed = false;
    } else {
        // Сворачиваем панель
        panel.classList.add('collapsed');
        content.classList.add('collapsed');
        
        // Скрываем кнопку очистки
        if (clearButton) {
            clearButton.style.display = 'none';
        }
        
        // Сохраняем текущую высоту перед сворачиванием
        window.savedPanelHeight = panel.style.height || 'calc(100% - 20px)';
        
        // Устанавливаем высоту 50px
        panel.style.height = '50px';
        
        isDetailsCollapsed = true;
    }
}

function toggleExpand() {
    const mapCard = document.querySelector('.monitoring-map-card');
    const expandBtn = document.querySelector('[onclick="toggleExpand()"]');
    const expandIcon = expandBtn.querySelector('i');
    const expandText = expandBtn.querySelector('span');
    const viewContainer = document.querySelector('.monitoring-view-container');
    
    if (!isExpanded) {
        mapCard.classList.add('expanded');
        expandIcon.className = 'fas fa-compress';
        expandText.textContent = 'Свернуть';
        
        document.addEventListener('keydown', handleExpandEscape);
        
        isExpanded = true;
    } else {
        mapCard.classList.remove('expanded');
        expandIcon.className = 'fas fa-expand';
        expandText.textContent = 'Развернуть';
        
        if (viewContainer) {
            viewContainer.style.display = 'block';
        }
        
        document.removeEventListener('keydown', handleExpandEscape);
        
        isExpanded = false;
    }
    
    if (window.monitoringMap) {
        setTimeout(() => {
            window.monitoringMap.invalidateSize();
        }, 100);
    }
}

function handleExpandEscape(e) {
    if (e.key === 'Escape' && isExpanded) {
        toggleExpand();
    }
}

window.addEventListener('resize', function() {
    if (window.monitoringMap) {
        setTimeout(() => {
            window.monitoringMap.invalidateSize();
        }, 100);
    }
});