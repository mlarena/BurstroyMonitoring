(function() {
    let currentGroup = "DSPD";
    let currentDays = 1;
    let currentInterval = "hour";
    let activeMetricKey = null; // Ключ выбранной метрики (null = все)
    let chartData = null;

    const margin = { top: 60, right: 150, bottom: 70, left: 80 };
    const width = 1200 - margin.left - margin.right;
    const height = 600 - margin.top - margin.bottom;

    const svg = d3.select("#chart")
        .append("g")
        .attr("transform", `translate(${margin.left},${margin.top})`);

    const xScale = d3.scaleTime().range([0, width]);
    const yLeft = d3.scaleLinear().range([height, 0]);
    const yRight = d3.scaleLinear().range([height, 0]);
    const yRight2 = d3.scaleLinear().range([height, 0]);

    const xAxisGroup = svg.append("g").attr("transform", `translate(0, ${height})`);
    const yLeftAxisGroup = svg.append("g").attr("class", "axis-left");
    const yRightAxisGroup = svg.append("g").attr("class", "axis-right").attr("transform", `translate(${width}, 0)`);
    const yRight2AxisGroup = svg.append("g").attr("class", "axis-right2").attr("transform", `translate(${width + 70}, 0)`);
    const gridGroup = svg.append("g").attr("class", "grid");
    
    const bgRect = svg.append("rect")
        .attr("width", width).attr("height", height)
        .style("fill", "transparent")
        .style("pointer-events", "all");

    const linesGroup = svg.append("g").attr("class", "lines").style("pointer-events", "none");
    const pointsGroup = svg.append("g").attr("class", "points");

    const verticalLine = svg.append("line")
        .attr("class", "vertical-line")
        .attr("y1", 0).attr("y2", height)
        .attr("x1", -10).attr("x2", -10)
        .style("opacity", 0);

    const tooltip = d3.select("#tooltip");
    const showAllCheckbox = d3.select("#showAllMetrics");

    function loadAndRender() {
        const response = fetchChartData(currentGroup, currentDays, currentInterval);
        chartData = response.measurements.map(d => ({
            ...d,
            date: new Date(d.receivedAt)
        }));
        render();
    }

    function render() {
        const metrics = metricsGroups[currentGroup];
        const metricKeys = activeMetricKey ? [activeMetricKey] : Object.keys(metrics);

        xScale.domain(d3.extent(chartData, d => d.date));

        const getDomain = (axis) => {
            const keys = metricKeys.filter(k => metrics[k].axis === axis);
            if (keys.length === 0) return [0, 100];
            const min = d3.min(chartData.flatMap(d => keys.map(k => d[k])));
            const max = d3.max(chartData.flatMap(d => keys.map(k => d[k])));
            return [min * 0.9, max * 1.1];
        };

        yLeft.domain(getDomain("left"));
        yRight.domain(getDomain("right"));
        yRight2.domain(getDomain("right2"));

        yLeftAxisGroup.transition().call(d3.axisLeft(yLeft).ticks(8));
        yRightAxisGroup.transition().call(d3.axisRight(yRight).ticks(6));
        yRight2AxisGroup.transition().call(d3.axisRight(yRight2).ticks(6));
        xAxisGroup.transition().call(d3.axisBottom(xScale).ticks(10));

        gridGroup.call(d3.axisLeft(yLeft).ticks(6).tickSize(-width).tickFormat(""));

        linesGroup.selectAll("path").remove();
        metricKeys.forEach(key => {
            const m = metrics[key];
            const scale = m.axis === 'left' ? yLeft : (m.axis === 'right' ? yRight : yRight2);
            linesGroup.append("path")
                .datum(chartData)
                .attr("fill", "none")
                .attr("stroke", m.color)
                .attr("stroke-width", 2)
                .attr("opacity", 0.7)
                .attr("d", d3.line()
                    .x(d => xScale(d.date))
                    .y(d => scale(d[key]))
                    .curve(d3.curveMonotoneX)
                );
        });

        pointsGroup.selectAll("g").remove();
        metricKeys.forEach(key => {
            const m = metrics[key];
            const scale = m.axis === 'left' ? yLeft : (m.axis === 'right' ? yRight : yRight2);
            const g = pointsGroup.append("g");

            g.selectAll("circle")
                .data(chartData)
                .enter()
                .append("circle")
                .attr("cx", d => xScale(d.date))
                .attr("cy", d => scale(d[key]))
                .attr("r", 3)
                .attr("fill", m.color)
                .attr("stroke", "white")
                .attr("stroke-width", 0.5);
        });

        updateStatsPanel(chartData[chartData.length - 1]);
    }

    function updateStatsPanel(pointData) {
        if (!pointData) return;
        const metrics = metricsGroups[currentGroup];
        const grid = d3.select("#statsGrid").html("");
        Object.keys(metrics).forEach(key => {
            const m = metrics[key];
            const val = pointData[key];
            const card = grid.append("div")
                .attr("class", "stat-card")
                .classed("active", key === activeMetricKey)
                .on("click", () => {
                    activeMetricKey = key;
                    render();
                });

            const labelWrapper = card.append("div").attr("class", "label-content");
            labelWrapper.append("span").attr("class", "stat-dot").style("background", m.color);
            labelWrapper.append("span").attr("class", "stat-label").text(m.name);
            
            card.append("span").attr("class", "stat-value").text(`${val.toFixed(2)} ${m.unit}`);
        });
    }

    function showTooltip(event, d) {
        const metrics = metricsGroups[currentGroup];
        const showAll = showAllCheckbox.property("checked");
        tooltip.style("display", "block");
        
        let html = `<div class="tooltip-title">${d.date.toLocaleString()}</div>`;
        
        if (showAll) {
            Object.keys(metrics).forEach(key => {
                const m = metrics[key];
                const val = d[key];
                html += `<div class="tooltip-row">
                    <span style="color: ${m.color}">●</span>
                    <span>${m.name}</span>
                    <span class="tooltip-value">${val.toFixed(2)} ${m.unit}</span>
                </div>`;
            });
        } else {
            const [mouseX, mouseY] = d3.pointer(event, svg.node());
            let closestKey = null;
            let minDist = Infinity;

            Object.keys(metrics).forEach(key => {
                const m = metrics[key];
                const scale = m.axis === 'left' ? yLeft : (m.axis === 'right' ? yRight : yRight2);
                const dist = Math.abs(scale(d[key]) - mouseY);
                if (dist < minDist) {
                    minDist = dist;
                    closestKey = key;
                }
            });

            if (closestKey) {
                const m = metrics[closestKey];
                const val = d[closestKey];
                html += `<div class="tooltip-row">
                    <span style="color: ${m.color}">●</span>
                    <span>${m.name}</span>
                    <span class="tooltip-value">${val.toFixed(2)} ${m.unit}</span>
                </div>`;
            }
        }
        tooltip.html(html);
        moveTooltip(event);
    }

    function moveTooltip(event) {
        tooltip
            .style("left", (event.clientX + 15) + "px")
            .style("top", (event.clientY + 15) + "px");
    }

    function hideTooltip() {
        tooltip.style("display", "none");
    }

    // Инициализация кнопок групп
    const groupSelector = d3.select("#groupSelector");
    Object.keys(metricsGroups).forEach(key => {
        groupSelector.append("button")
            .attr("class", "group-btn")
            .classed("active", key === currentGroup)
            .text(key)
            .on("click", function() {
                currentGroup = key;
                activeMetricKey = null; // Сбрасываем фильтр конкретной метрики при смене группы
                d3.selectAll(".group-btn").classed("active", false);
                d3.select(this).classed("active", true);
                loadAndRender();
            });
    });

    // Обработка фильтров периода
    d3.selectAll(".range-btn").on("click", function() {
        currentDays = +d3.select(this).attr("data-days");
        d3.selectAll(".range-btn").classed("active", false);
        d3.select(this).classed("active", true);
        loadAndRender();
    });

    // Обработка фильтров агрегации
    d3.selectAll(".aggregation-btn").on("click", function() {
        currentInterval = d3.select(this).attr("data-interval");
        d3.selectAll(".aggregation-btn").classed("active", false);
        d3.select(this).classed("active", true);
        loadAndRender();
    });

    // Сброс фильтра метрики
    d3.select("#resetFilterBtn").on("click", () => {
        activeMetricKey = null;
        render();
    });

    // Обработка мыши на фоне
    bgRect
        .on("mousemove", function(event) {
            const [x] = d3.pointer(event);
            const date = xScale.invert(x);
            const closest = chartData.reduce((prev, curr) => 
                Math.abs(curr.date - date) < Math.abs(prev.date - date) ? curr : prev
            );
            verticalLine.attr("x1", xScale(closest.date)).attr("x2", xScale(closest.date)).style("opacity", 1);
            updateStatsPanel(closest);
            showTooltip(event, closest);
        })
        .on("mouseleave", () => {
            verticalLine.style("opacity", 0);
            updateStatsPanel(chartData[chartData.length - 1]);
            hideTooltip();
        });

    loadAndRender();
})();
