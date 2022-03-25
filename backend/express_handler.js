const express = require('express');
const api = require('./api');
const { getModData, telemetryHello, updateDownloadCounter, getAutoupdaterData } = require('./database');
const rateLimit = require("express-rate-limit");

// Basic setup
const app = express();
app.use(express.json());
app.listen(3000);

// This method gets a list of mods
app.post('/mods', async(request, response) => {
    let modList = request.body;

    if(modList === undefined || modList["mods"] === undefined)
    {
        response.sendStatus(400);
        return;
    }

    let modsRequiringUpdate = [];
    try
    {
        for(let i = 0; i < modList["mods"].length; i++){
            let data = await getModData(modList["mods"][i]["modId"]);
    
            if(data === undefined)
                continue;
    
            if(data["current_version"] != modList["mods"][i]["version"])
            {
                modsRequiringUpdate.push(data);
            }
        }

        if(modList["telemetry"] == true)
            telemetryHello();
        
        response.send(modsRequiringUpdate);
    }

    catch(error){
        console.log("An issue ocurred on /mods API.");
        console.log(error);
        response.sendStatus(500);
    }
});

app.post("/mod_download", async(request, response) => {
    let mod = request.body;
    if(mod === undefined || mod["mod_id"] === undefined){
        response.sendStatus(400);
        return;
    }

    try {
        let data = await getModData(mod["mod_id"]);
        if(data === undefined){
            response.sendStatus(400);
            return;
        }

        updateDownloadCounter(data["internal_id"], data["current_version"]);
        response.send(data["current_download_link"]);
    } catch(error){
        console.log("An issue ocurred on /mod_download API.");
        console.log(error);
        response.sendStatus(500);
    }
});

app.get("/autoupdater", async(request, response) => {
    try
    {
        let data = await getAutoupdaterData();
        let returnData = new Object();
        returnData["current_version"] = data[0]["current_version"];
        returnData["download_link"] = data[0]["download_link"];
        response.send(returnData);
    }
    catch(error) {
        console.log("An issue ocurred on /autoupdater API.");
        console.log(error);
        response.sendStatus(500);
    }
});