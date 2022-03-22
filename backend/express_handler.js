const express = require('express');
const api = require('./api');
const { getModData } = require('./database');

// Basic setup
const app = express();
app.use(express.json());
app.listen(3000);

// This method gets a list of mods ()
app.post('/mods', async(request, response) => {
    let modList = request.body;
    if(modList === undefined || modList["mods"] === undefined)
    {
        response.sendStatus(400);
        return;
    }

    let modsRequiringUpdate = [];

    for(let i = 0; i < modList["mods"].length; i++){
        let data = await getModData(modList["mods"][i]["modId"]);

        if(data === undefined)
            continue;

    
        if(data["current_version"] != modList["mods"][i]["version"])
        {
            modsRequiringUpdate.push(data);
        }
    }

    response.send(modsRequiringUpdate);
});

app.post("/mod_download", function(request, response){
    let mod = request.body;

    response.sendStatus(200);
});
