const Player = require("./player");
const GameState = require("./gameState");

module.exports = class Match {
  player;
  gameState;

  constructor(player, gameState) {
    this.player = player;
    this.gameState = gameState;
  }
};
