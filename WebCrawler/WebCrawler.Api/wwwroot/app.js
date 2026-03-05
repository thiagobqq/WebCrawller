const queueCountEl = document.getElementById("queueCount");
const pagesCountEl = document.getElementById("pagesCount");
const queueListEl = document.getElementById("queueList");
const pagesTableBodyEl = document.getElementById("pagesTableBody");
const logListEl = document.getElementById("logList");
const apiStatusEl = document.getElementById("apiStatus");
const lastUpdateEl = document.getElementById("lastUpdate");
const enqueueBtn = document.getElementById("enqueueBtn");
const pauseBtn = document.getElementById("pauseBtn");
const resumeBtn = document.getElementById("resumeBtn");
const refreshBtn = document.getElementById("refreshBtn");
const urlInput = document.getElementById("urlInput");
const chartCanvas = document.getElementById("metricsChart");
const ctx = chartCanvas.getContext("2d");
const loadPagesBtn = document.getElementById("loadPagesBtn");
const newPagesAlert = document.getElementById("newPagesAlert");

const historyPoints = [];
const maxHistoryPoints = 20;
let connection = null;
let lastPageCount = 0;
let currentPageCount = 0;

function addLog(message, level = "info") {
  const now = new Date().toLocaleTimeString();
  const item = document.createElement("div");
  item.className = "log-item";
  item.textContent = `[${now}] [${level.toUpperCase()}] ${message}`;
  logListEl.prepend(item);

  while (logListEl.children.length > 60) {
    logListEl.removeChild(logListEl.lastChild);
  }
}

function setApiStatus(text, className) {
  apiStatusEl.textContent = text;
  apiStatusEl.className = `badge ${className}`;
}

function safeText(value) {
  if (value === null || value === undefined) return "-";
  return String(value);
}

function updateQueue(urls) {
  queueListEl.innerHTML = "";

  if (!urls || urls.length === 0) {
    const empty = document.createElement("div");
    empty.className = "list-item";
    empty.textContent = "Queue is empty.";
    queueListEl.appendChild(empty);
    return;
  }

  urls.slice(0, 100).forEach((url, index) => {
    const item = document.createElement("div");
    item.className = "list-item";
    item.textContent = `${index + 1}. ${safeText(url)}`;
    queueListEl.appendChild(item);
  });
}

function updatePages(pages) {
  pagesTableBodyEl.innerHTML = "";

  if (!pages || pages.length === 0) {
    const tr = document.createElement("tr");
    tr.innerHTML = "<td colspan='3'>No pages indexed yet.</td>";
    pagesTableBodyEl.appendChild(tr);
    return;
  }

  pages.slice(0, 40).forEach((page) => {
    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td>${safeText(page.id)}</td>
      <td>${safeText(page.title)}</td>
      <td><a href="${safeText(page.url)}" target="_blank" rel="noopener noreferrer">${safeText(page.url)}</a></td>
    `;
    pagesTableBodyEl.appendChild(tr);
  });
}

function pushHistory(queueCount, pagesCount) {
  historyPoints.push({
    t: new Date(),
    queueCount,
    pagesCount
  });

  if (historyPoints.length > maxHistoryPoints) {
    historyPoints.shift();
  }
}

function drawChart() {
  const w = chartCanvas.width;
  const h = chartCanvas.height;
  const pad = 26;

  ctx.clearRect(0, 0, w, h);
  ctx.fillStyle = "#121926";
  ctx.fillRect(0, 0, w, h);

  ctx.strokeStyle = "#2f3a4d";
  ctx.lineWidth = 1;
  ctx.strokeRect(pad, 14, w - pad * 2, h - pad - 14);

  if (historyPoints.length < 2) {
    ctx.fillStyle = "#b7c4db";
    ctx.fillText("Waiting for data points...", 34, h / 2);
    return;
  }

  const maxQueue = Math.max(...historyPoints.map((p) => p.queueCount));
  const maxPages = Math.max(...historyPoints.map((p) => p.pagesCount));
  const maxY = Math.max(1, maxQueue, maxPages);

  const xStep = (w - pad * 2) / (historyPoints.length - 1);
  const chartHeight = h - pad - 18;

  function toY(value) {
    return 14 + chartHeight - (value / maxY) * chartHeight;
  }

  function plotLine(getValue, color) {
    ctx.beginPath();
    ctx.strokeStyle = color;
    ctx.lineWidth = 2;

    historyPoints.forEach((point, i) => {
      const x = pad + i * xStep;
      const y = toY(getValue(point));
      if (i === 0) ctx.moveTo(x, y);
      else ctx.lineTo(x, y);
    });

    ctx.stroke();
  }

  plotLine((p) => p.queueCount, "#6fb6ff");
  plotLine((p) => p.pagesCount, "#63e59d");

  ctx.fillStyle = "#6fb6ff";
  ctx.fillRect(w - 180, 22, 10, 10);
  ctx.fillStyle = "#e8eefb";
  ctx.fillText("Queue", w - 164, 31);

  ctx.fillStyle = "#63e59d";
  ctx.fillRect(w - 110, 22, 10, 10);
  ctx.fillStyle = "#e8eefb";
  ctx.fillText("Pages", w - 94, 31);

  ctx.fillStyle = "#b7c4db";
  ctx.fillText(`Scale max: ${maxY}`, 34, h - 8);
}

async function getJson(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`Request failed: ${url} - ${response.status}`);
  }

  return response.json();
}

async function loadPages() {
  try {
    const pagesData = await getJson("/api/pages/pages");
    const pages = Array.isArray(pagesData) ? pagesData : [];
    
    updatePages(pages);
    lastPageCount = pages.length;
    currentPageCount = pages.length;
    pagesCountEl.textContent = String(pages.length);
    newPagesAlert.style.display = "none";
    
    addLog(`Loaded ${pages.length} pages`, "info");
  } catch (error) {
    addLog(error.message || "Failed to load pages", "error");
  }
}

function notifyNewPage() {
  currentPageCount++;
  pagesCountEl.textContent = String(currentPageCount);
  
  if (currentPageCount > lastPageCount) {
    newPagesAlert.style.display = "inline-block";
    newPagesAlert.textContent = `🔔 ${currentPageCount - lastPageCount} new pages available`;
  }
  
  pushHistory(Number(queueCountEl.textContent), currentPageCount);
  drawChart();
}

async function callCrawlerControl(route, actionLabel) {
  try {
    const response = await fetch(route, { method: "POST" });
    if (!response.ok) {
      throw new Error(`${actionLabel} failed (${response.status})`);
    }

    addLog(`${actionLabel} success`, "info");
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
      await connection.invoke("GetCrawlQueue");
    }
  } catch (error) {
    addLog(error.message || `${actionLabel} failed`, "error");
  }
}

enqueueBtn.addEventListener("click", async () => {
  const url = urlInput.value.trim();
  if (!url) {
    addLog("Provide a valid URL to enqueue", "warn");
    return;
  }

  try {
    const response = await fetch("/api/crawler/enqueue", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(url)
    });

    if (!response.ok) {
      throw new Error(`Enqueue failed (${response.status})`);
    }

    addLog(`URL enqueued: ${url}`, "info");
    urlInput.value = "";
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
      await connection.invoke("GetCrawlQueue");
    }
  } catch (error) {
    addLog(error.message || "Failed to enqueue URL", "error");
  }
});

pauseBtn.addEventListener("click", async () => {
  await callCrawlerControl("/api/crawler/pause", "Pause crawler");
});

resumeBtn.addEventListener("click", async () => {
  await callCrawlerControl("/api/crawler/resume", "Resume crawler");
});

refreshBtn.addEventListener("click", async () => {
  addLog("Manual refresh queue", "info");
});

loadPagesBtn.addEventListener("click", async () => {
  await loadPages();
});

newPagesAlert.addEventListener("click", async () => {
  await loadPages();
});

async function startSignalR() {
  connection = new signalR.HubConnectionBuilder()
    .withUrl("/crawlerHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.on("ReceiveCrawlQueue", (data) => {
    const queueCount = Number(data.count || 0);
    const urls = Array.isArray(data.urls) ? data.urls : [];
    
    queueCountEl.textContent = String(queueCount);
    updateQueue(urls);
    
    lastUpdateEl.textContent = `Last update: ${new Date().toLocaleTimeString()}`;
  });

  connection.on("ReceiveMessage", (message) => {
    addLog(message, "info");
  });

  connection.on("ReceiveCrawlProgress", (url, progress) => {
    addLog(`Processing: ${url} (${progress}%)`, "info");
  });

  connection.on("QueueUpdated", (data) => {
    const queueCount = Number(data.count || 0);
    const urls = Array.isArray(data.urls) ? data.urls : [];
    
    queueCountEl.textContent = String(queueCount);
    updateQueue(urls);
  });

  connection.on("PageSaved", (page) => {
    addLog(`Page saved: ${page.url}`, "info");
    notifyNewPage();
  });

  connection.onreconnecting(() => {
    setApiStatus("Reconnecting...", "badge-warn");
    addLog("SignalR reconnecting...", "warn");
  });

  connection.onreconnected(() => {
    setApiStatus("Connected", "badge-ok");
    addLog("SignalR reconnected", "info");
    connection.invoke("GetCrawlQueue").catch(err => addLog(err.toString(), "error"));
  });

  connection.onclose(() => {
    setApiStatus("Disconnected", "badge-err");
    addLog("SignalR disconnected", "error");
    setTimeout(startSignalR, 5000);
  });

  try {
    await connection.start();
    setApiStatus("Connected", "badge-ok");
    addLog("SignalR connected", "info");
    
    await connection.invoke("GetCrawlQueue");
  } catch (err) {
    setApiStatus("Connection failed", "badge-err");
    addLog(err.toString(), "error");
    setTimeout(startSignalR, 5000);
  }
}

addLog("Dashboard loaded", "info");
startSignalR();
loadPages();