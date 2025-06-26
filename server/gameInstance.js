const express = require("express");
const router = express.Router();
const Player = require("./player.js");
const GameState = require("./gameState.js");
const Match = require("./match.js");
const fs = require("fs");

module.exports = class GameInstance {
  expressApp;
  apiRouter;
  matches = [];
  endedMatches = [];
  tickLoop = null;
  defaultGameTime = 120;

  constructor() {
    console.log("Instance Created");
  }

  async Initialize(app) {
    this.expressApp = app;
    this.apiRouter = router;
    await this.SetupRoutes();
    this.expressApp.use(this.apiRouter);
  }

  async OnPlayerJoined(player) {
    console.log(this.matches);
    this.endedMatches = this.endedMatches.filter(
      (match) => match.player.nickname !== player.nickname
    );
    this.matches.push(new Match(player, new GameState(this.defaultGameTime)));
    if (this.tickLoop === null) {
      this.StartTicking();
    }
  }

  // 4.1: Readjusted how some routes are declared and implemented. WebSockets could be used instead of HTTP for real-time updates, but for simplicity, I keep HTTP here.
  async SetupRoutes() {
    // 4.3: Supports requests to save player score by name in a JSON file format.
    this.apiRouter.post("/save", (req, res) => {
      const { username, score } = req.body;
      if (!username || typeof score !== "number") {
        return res.status(400).send({ error: "Invalid username or score." });
      }
      const leaderboardPath = __dirname + "/leaderboard.json";
      let leaderboard = [];
      try {
        if (fs.existsSync(leaderboardPath)) {
          leaderboard = JSON.parse(fs.readFileSync(leaderboardPath, "utf8"));
        }
      } catch (err) {
        return res.status(500).send({ error: "Failed to read leaderboard." });
      }

      const existing = leaderboard.find((entry) => entry.username === username);
      if (existing) {
        if (score > existing.score) existing.score = score;
      } else {
        leaderboard.push({ username, score });
      }
      try {
        fs.writeFileSync(leaderboardPath, JSON.stringify(leaderboard, null, 2));
      } catch (err) {
        return res.status(500).send({ error: "Failed to save leaderboard." });
      }
      res.send({ status: "ok" });
    });

    this.apiRouter.get("/leaderboard", (req, res) => {
      const leaderboardPath = __dirname + "/leaderboard.json";
      let leaderboard = [];
      try {
        if (fs.existsSync(leaderboardPath)) {
          leaderboard = JSON.parse(fs.readFileSync(leaderboardPath, "utf8"));
        }
      } catch (err) {
        return res.status(500).send({ error: "Failed to read leaderboard." });
      }
      leaderboard.sort((a, b) => b.score - a.score);
      res.send({ entries: leaderboard });
    });

    this.apiRouter.post("/join", (req, res) => {
      const newPlayer = new Player(req.body.username);
      if (
        this.matches.find(
          (match) => match.player.nickname === newPlayer.nickname
        )
      ) {
        return res.status(400).send({
          error: "Player with similar username has an existing session.",
        });
      }
      res.send({ status: "ok" });
      this.OnPlayerJoined(newPlayer);
    });

    this.apiRouter.get("/gameended", (req, res) => {
      return this.endedMatches.some((match) => {
        match.player.nickname === req.body.username;
      })
        ? res.status(200).send({ ended: true })
        : res.status(404).send({ ended: false });
    });

    this.apiRouter.get("/gamestarted", (req, res) => {
      res.send({ started: this.started });
    });

    this.apiRouter.get("/timeLeft", (req, res) => {
      if (
        this.endedMatches.find(
          (match) => match.player.nickname === req.body.username
        )
      ) {
        return res.status(404).send({ timeLeft: -1 });
      }
      res.send({
        timeLeft: this.matches.find(
          (match) => match.player.nickname === req.body.username
        ).gameState.timeLeft,
      });
    });

    this.apiRouter.get("/aggression", (req, res) => {
      const timeLeft = this.matches.find(
        (match) => match.player.nickname === req.body.username
      )?.gameState.timeLeft;
      console.log(
        "Sending aggression: ",
        (1 - timeLeft / this.defaultGameTime) * 6
      );
      res.send({ aggression: (1 - timeLeft / this.defaultGameTime) * 6 });
    });

    // 4.2: Upon loss, win or timeout the client can ask the server to end the match. Server automatically ends the match after the time runs out anyway.
    this.apiRouter.post("/endmatch", (req, res) => {
      this.endedMatches.find(
        (match) => match.player.nickname === req.body.username
      );
      if (endedMatches !== undefined) {
        res.status(200);
      }
      let match = this.matches.find(
        (match) => match.player.nickname === req.body.username
      );
      match.gameState.ended = true;
      this.endedMatches.push(match);
      this.matches = this.matches.filter(
        (match) => match.player.nickname !== req.body.username
      );
      res.status(200);
    });
  }

  async StartTicking() {
    console.log("Starting tick loop");
    this.tickLoop = setInterval(() => {
      this.matches.map((match) => {
        if (match.gameState.ended) {
          return;
        }
        match.gameState.timeLeft--;
        if (match.gameState.timeLeft <= 0) {
          match.gameState.ended = true;
          this.endedMatches.push(match);
          console.log(`Match for player ${match.player.nickname} ended`);
        }
        console.log(
          `Match for player ${match.player.nickname} time left: ${match.gameState.timeLeft}`
        );
      });

      this.matches = this.matches.filter(
        (match) => match.gameState.timeLeft > 0
      );

      if (this.matches.length === 0) {
        console.log("All matches ended, stopping tick loop");
        clearInterval(this.tickLoop);
        this.tickLoop = null;
      }
    }, 1000);
  }
};
