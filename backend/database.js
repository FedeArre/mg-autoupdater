const mysql = require('mysql');

let connection = null;
let db = {};

db.setupDatabaseConnection = function(host, username, password, database_name) {
	connection = mysql.createPool({
		connectionLimit: 50,
		host: host,
		user: username,
		password: password,
		database: database_name
	});
	console.log("MySQL connection setup");
};

db.exposeCurrentPool = () => { return connection; }

db.addNewMod = function(modId, modName, version, downloadLink, file_name, discordName){
	return new Promise((resolve, reject) => {
		connection.query("INSERT INTO mods(mod_id, mod_name, current_version, current_download_link, file_name, created_by, last_update) VALUES (?, ?, ?, ?, ?, ?, ?)", 
		[modId, modName, version, downloadLink, file_name, discordName, new Date()], function(error, result){
			if(!error)
			{
				connection.query("INSERT INTO mods_download(mod_id, version, downloadCounter) VALUES(?, ?, 0)", [result.insertId, version], function (error2, result2){
					if(error)
						reject(error);
					else
						resolve(true);
				});
			}
			else
			{
				reject(error);
			}
		});
	});
}

db.doesModExist = function(modId){
	return new Promise((resolve, reject) => {
		connection.query("SELECT mods.internal_id FROM mods WHERE mods.mod_id = ?", [modId], function(error, result) {
			if(!error)
			{
				resolve(result);
			} 
			else 
			{
				reject(error);
			}
		});
	});
}

db.updateMod = function(modId, newVersion, newDownloadLink){
	return new Promise((resolve, reject) => {
		connection.query("UPDATE mods SET current_version = ?, current_download_link = ? WHERE mod_id = ?", [newVersion, newDownloadLink, modId], function(error, result){
			if(!error)
			{
				resolve(result);
			}
			else
			{
				reject(error);
			}
		});
	});
}

db.updateModDownloads = function(internal_mod_id, newVersion){
	return new Promise((resolve, reject) => {
		connection.query("INSERT INTO mods_download VALUES (?, ?, 0)", [internal_mod_id, newVersion], function(error, result){
			if(!error)
			{
				resolve(result);
			}
			else
			{
				reject(error);
			}
		});
	});
}

db.getModData = function(modId){
	return new Promise((resolve, reject) => {
		connection.query("SELECT * FROM mods WHERE mod_id = ?", [modId], function(error, result, fields){
			if(!error)
			{
				result = JSON.parse(JSON.stringify(result));
				resolve(result[0]);
			}
			else
			{
				reject(error);
			}
		});
	});
}

db.telemetryHello = function () {
	connection.query("UPDATE telemetry SET count = count + 1 WHERE type = ?", ["hello"], function (error, result) {
		if (error) {
			console.log("Error with telemetry hello.");
			console.log(error);
		}
	});
}

db.updateDownloadCounter = function(internal_mod_id, current_version) {
	connection.query("UPDATE mods_download SET downloadCounter = downloadCounter + 1 WHERE mod_id = ? AND version = ?", [internal_mod_id, current_version], function (error, result) {
		if (error){
			console.log("Error while updating download counter.");
			console.log(error);
		}
	});
}

db.getAutoupdaterData = function(){
	return new Promise((resolve, reject) => {
		connection.query("SELECT * FROM autoupdater_information LIMIT 1", function(error, result){
			if(error){
				console.log("Error while updating download counter.");
				console.log(error);
				reject(error);
			}
			else {
				result = JSON.parse(JSON.stringify(result));
				resolve(result);
			}
		});
	});
}
module.exports = db;