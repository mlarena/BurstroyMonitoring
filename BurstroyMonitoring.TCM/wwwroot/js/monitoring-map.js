// Глобальные переменные
let map;
let markers = [];
let isExpanded = false;
let searchTimeout = null;
let selectedAutocompleteIndex = -1;

const cfg = window.mapConfig ?? { containerId: 'monitoring-map', widget: false };

// Инициализация — вызывается сразу, т.к. скрипт рендерится после div карты
initMap();
loadMapData();
if (!cfg.widget) initSearch();

function initMap() {
    // Центр России
    map = L.map(cfg.containerId).setView([55.7558, 37.6173], 5);

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
        html: `
            <div class="marker-pin-wrapper ${post.isActive ? 'active-pulse' : ''}">
                <div class="marker-pin" style="background-color: ${bgColor};">
                    <i class="bi ${post.isMobile ? 'bi-truck' : 'bi-building'}"></i>
                </div>
            </div>`,
        iconSize: [30, 30],
        iconAnchor: [15, 15]
    });

    const marker = L.marker([post.latitude, post.longitude], { icon })
        .addTo(map)
        .bindTooltip(`
            <div class="marker-tooltip">
                <div class="tooltip-title">${post.name}</div>
                <div class="tooltip-address">${post.address || 'Адрес не указан'}</div>
            </div>
        `, {
            direction: 'top',
            offset: [0, -15],
            className: 'modern-marker-tooltip'
        })
        .bindPopup(`
            <div class="monitoring-popup-card">
                <div class="popup-header">
                    <div class="popup-title">${post.name}</div>
                    <div class="popup-address"><i class="bi bi-geo-alt-fill me-1"></i>${post.address || 'Адрес не указан'}</div>
                </div>
                
                <div class="popup-body">
                    <div class="popup-info-row">
                        <span class="badge ${post.isActive ? 'bg-success' : 'bg-danger'} mb-2">
                            ${post.isActive ? 'Активен' : 'Неактивен'}
                        </span>
                        <span class="badge bg-info text-dark mb-2 ms-1">
                            ${post.isMobile ? 'Мобильный' : 'Стационарный'}
                        </span>
                    </div>
                    
                    <div class="popup-sensor-section">
                        <div class="sensor-section-title">Датчики (${post.sensorCount})</div>
                        <div class="popup-sensor-list">
                            ${post.sensors && post.sensors.length > 0 ?
                                post.sensors.map(s => `
                                    <div class="popup-sensor-item">
                                        <div class="sensor-icon-wrapper">
                                            <i class="bi bi-cpu"></i>
                                        </div>
                                        <div class="sensor-info" style="flex:1; cursor:pointer;"
                                             onclick="showSensorDetailsFromPopup(${s.id}, ${s.sensorTypeId}, '${s.endPointsName}')">
                                            <div class="sensor-endpoint">${s.endPointsName}</div>
                                        </div>
                                        <a href="/Sensors/Edit/${s.id}" title="Настройки датчика"
                                           onclick="event.stopPropagation()" class="btn btn-sm btn-link p-0 ms-2 text-muted">
                                            <i class="bi bi-gear"></i>
                                        </a>
                                    </div>
                                `).join('') :
                                '<div class="text-muted small p-2">Нет доступных датчиков</div>'
                            }
                        </div>
                    </div>
                </div>
                <div class="popup-footer" style="padding: 8px;">
                    <a href="/MonitoringPosts/Edit/${post.id}" class="btn btn-sm btn-outline-secondary w-100">
                        <i class="bi bi-gear me-1"></i>Настройки поста
                    </a>
                </div>
            </div>
        `, {
            maxWidth: 300,
            className: 'modern-monitoring-popup'
        });

    marker.on('click', function() {
        map.panTo([post.latitude, post.longitude]);
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
        map.panTo([sensor.latitude, sensor.longitude]);
        showSensorDetailsFromPopup(sensor.id, sensor.sensorTypeId, sensor.name);
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

// Функция для показа данных датчика из попапа или при клике на маркер датчика
async function showSensorDetailsFromPopup(sensorId, sensorTypeId, sensorName) {
    // Показываем панель
    const panel = document.getElementById('sensorDataPanel');
    if (panel) {
        panel.classList.remove('hidden');
    }

    // Обновляем кнопку настроек датчика
    const settingsBtn = document.getElementById('sensorSettingsBtn');
    if (settingsBtn) {
        settingsBtn.href = `/Sensors/Edit/${sensorId}`;
        settingsBtn.classList.remove('d-none');
    }

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
            
            // Извлекаем данные из скрытого заголовка в PartialView
            const infoHeader = tempDiv.querySelector('.sensor-info-header');
            let postName = '', postAddress = '', sensorType = '', endpoint = '', received = '';
            
            if (infoHeader) {
                postName = infoHeader.getAttribute('data-post-name') || '';
                postAddress = infoHeader.getAttribute('data-post-address') || '';
                sensorType = infoHeader.getAttribute('data-sensor-type') || '';
                endpoint = infoHeader.getAttribute('data-endpoint') || '';
                received = infoHeader.getAttribute('data-received') || '';
            }
            
            // Обновляем заголовок
            const sensorTitle = document.getElementById('sensorTitle');
            if (sensorTitle) {
                sensorTitle.innerHTML = `
                    <div style="color: #fff; font-weight: bold;">${postName}</div>
                    <div style="color: #fff; font-size: 0.85em; margin-bottom: 2px;">${postAddress}</div>
                    <div style="color: #fff; font-size: 0.9em;">${sensorType}: ${endpoint}</div>
                    <div style="color: #fff; font-size: 0.8em; margin-top: 4px;">
                        <i class="far fa-clock"></i> ${received}
                    </div>
                `;
            }
            
            sensorContent.innerHTML = html;
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

// Функция для показа данных датчика
async function showSensorDetails(sensorId, element) {
    // Показываем панель
    const panel = document.getElementById('sensorDataPanel');
    if (panel) {
        panel.classList.remove('hidden');
    }

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
    
    const panel = document.getElementById('sensorDataPanel');
    if (panel) {
        panel.classList.add('hidden');
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

function toggleExpand() {
    console.log('toggleExpand called, current isExpanded:', isExpanded);
    
    const mapContainer = document.querySelector('.monitoring-map-container');
    const expandBtn = document.getElementById('toggleExpandBtn');
    const expandIcon = expandBtn ? expandBtn.querySelector('i') : null;
    const expandText = expandBtn ? expandBtn.querySelector('span') : null;
    
    console.log('Elements found:', {
        mapContainer: !!mapContainer,
        expandBtn: !!expandBtn,
        expandIcon: !!expandIcon,
        expandText: !!expandText
    });
    
    if (!isExpanded) {
        if (mapContainer) {
            mapContainer.classList.add('expanded');
            console.log('Added expanded class to mapContainer');
        } else {
            console.error('mapContainer element not found!');
        }
        
        if (expandIcon) {
            expandIcon.className = 'bi bi-arrows-angle-contract';
        }
        
        if (expandText) {
            expandText.textContent = 'Свернуть';
        }
        
        document.addEventListener('keydown', handleExpandEscape);
        isExpanded = true;
    } else {
        if (mapContainer) {
            mapContainer.classList.remove('expanded');
            console.log('Removed expanded class from mapContainer');
        }
        
        if (expandIcon) {
            expandIcon.className = 'bi bi-arrows-angle-expand';
        }
        
        if (expandText) {
            expandText.textContent = 'Развернуть';
        }
        
        document.removeEventListener('keydown', handleExpandEscape);
        isExpanded = false;
    }
    
    if (window.monitoringMap) {
        console.log('Invalidating map size...');
        setTimeout(() => {
            window.monitoringMap.invalidateSize();
            console.log('Map size invalidated');
        }, 300);
    } else {
        console.warn('window.monitoringMap not found');
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