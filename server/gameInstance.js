const express       = require("express");
const router        = express.Router();
const Player        = require("./player.js");

module.exports = class GameInstance {
    expressApp;
    apiRouter;
    players = [];
    timeleft = 120; //seconds
    tickLoop = null;
    started = false;
    ended = false;

    constructor() {
        console.log("Instance Created");
    }

    async Initialize(app) {
        this.expressApp = app;
        this.apiRouter = router;
        await this.SetupRoutes();
        this.expressApp.use(this.apiRouter);
    }

    async OnPlayerJoined(){
        if(this.ended) return;
        console.log(this.players);
        if(this.players.length > 0 && this.tickLoop === null){
            this.started = true;
            this.tickLoop = setInterval(() => {
                console.log(this.timeleft);
                this.timeleft--;
                if(this.timeleft < 0){
                    this.ended = true;
                    clearInterval(this.tickLoop);
                    this.tickLoop = null;
                }
            }, 1000);
        }
    }

    async SetupRoutes(){
        this.apiRouter.post("/join", (req, res) => {
            let newPlayer = new Player(req.body.username);
            this.players.push(newPlayer);
            res.send({status:"ok"});
            this.OnPlayerJoined();
        })
        this.apiRouter.get("/gameended", (req, res) => {
            res.send({ended:this.ended});
        })
        this.apiRouter.get("/gamestarted", (req, res) => {
            res.send({started:this.started});
        })
        this.apiRouter.get("/timeleft", (req, res) => {
            res.send({timeleft:this.timeleft});
        })
        this.apiRouter.get("/agression", (req, res) => {
            res.send({aggression: ( (1 - (this.timeleft/120))*6) })
        })
    }
}