module.exports = class GameState {
  timeLeft = 120;
  started = false;
  ended = false;

  constructor(timeLeft = 120, started = true, ended = false) {
    this.timeLeft = timeLeft;
    this.started = started;
    this.ended = ended;
  }
};
