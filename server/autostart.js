const { spawn } = require("child_process");
const fs = require("fs");
const os = require("os");
const pidusage = require("pidusage");
const ping = require("ping");

const EXECUTABLE = "./game_build/game.exe";
const PING_TARGET = "localhost";
const CSV_FILE = "game_stats.csv";
const INTERVAL = 2000; // ms

function clearCSV() {
  fs.writeFileSync(CSV_FILE, "timestamp,cpu,memory,ping\n");
}

function appendCsv({ timestamp, cpu, memory, ping }) {
  fs.appendFileSync(CSV_FILE, `${timestamp},${cpu},${memory},${ping}\n`);
}

async function monitorProcess(pid) {
  clearCSV();
  const timer = setInterval(async () => {
    try {
      const stats = await pidusage(pid);
      const res = await ping.promise.probe(PING_TARGET);
      appendCsv({
        timestamp: new Date().toISOString(),
        cpu: stats.cpu.toFixed(2),
        memory: stats.memory,
        ping: res.time || 0,
      });
    } catch (e) {
      clearInterval(timer);
    }
  }, INTERVAL);
}

function startAndMonitor() {
  const child = spawn(EXECUTABLE, [], { detached: true });
  monitorProcess(child.pid);
  child.on("exit", () => process.exit(0));
}

module.exports = { startAndMonitor };
