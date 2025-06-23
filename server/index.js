const express = require("express");
const app = express();
const http = require("http");
const cors = require("cors");
const bodyParser = require("body-parser");
const server = http.Server(app);
const Instance = require("./gameInstance.js");

const port = process.env.PORT || 4444;

let corsOptions = { //In general allow cors for local tests.
    origin: '*',
    optionsSuccessStatus: 200 // some legacy browsers (IE11, various SmartTVs) choke on 204
}

app.use(cors(corsOptions));

app.use(express.json({limit:'5mb'}));
app.use(bodyParser.urlencoded({limit: '5mb', extended: true}));
app.use(bodyParser.json({limit : '5mb'}));

app.use(function(req, res, next) {
    res.header('Access-Control-Allow-Origin', "*");
    res.header('Access-Control-Allow-Methods', 'OPTIONS,GET,PUT,POST,DELETE');
    res.header('Access-Control-Allow-Headers', 'Content-Type, master-pass');
    next();
});

module.exports = app;

let InitializeServer = async () =>{
    let instance = new Instance();
    await instance.Initialize(app);
    server.listen(port, () => {console.log(`Server is up on port : ${port}`);})
    server.setTimeout(30*1000);   //30 seconds timeout
}

InitializeServer();
